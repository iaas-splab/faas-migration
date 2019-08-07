using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dynamitey.DynamicObjects;
using MatrixMul.Core;
using MatrixMul.Core.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace MatrixMul.Azure
{
    public static class Functions
    {
        [FunctionName("OrchestrateMatrixMultiplication")]
        public static async Task<TimeMeasurement> OrchestrateMultiplication(
            [OrchestrationTrigger] DurableOrchestrationContext context
        )
        {
            var configuration = context.GetInput<CalculationConfiguration>();
            var s = configuration.MatrixSize;

            var calculation = await context.CallActivityAsync<TMC<MatrixCalculation>>("GenerateMatrix",
                configuration);

            Matrix result = null;
            TimeMeasurement measurement = calculation.Measurement;

            if (s < 10)
            {
                var calcResult =
                    await context.CallActivityAsync<TMC<Matrix>>("SerialMultiply", calculation);
                result = calcResult.Value;
                measurement = calcResult.Measurement;
            }
            else
            {
                var tasks =
                    await context.CallActivityAsync<TMC<Dictionary<int, ComputationTask[]>>>(
                        "DistributeWork",
                        new TMC<WorkDistributionContext>(
                            new WorkDistributionContext
                            {
                                Calculation = calculation.Value,
                                WorkerCount = 5
                            }, measurement)
                    );
                measurement = tasks.Measurement;

                var scheduledTasks = new Dictionary<int, Task<TMC<ComputationResult[]>>>();
                foreach (var keyValuePair in tasks.Value)
                {
                    var task = context.CallActivityAsync<TMC<ComputationResult[]>>(
                        "ParallelMultiply", new TMC<ParallelWorkerContext>(
                            new ParallelWorkerContext
                            {
                                Calculation = calculation.Value,
                                WorkerID = keyValuePair.Key,
                                Tasks = keyValuePair.Value
                            }, measurement));
                    scheduledTasks.Add(keyValuePair.Key, task);
                }

                var resultSet = new Dictionary<int, ComputationResult[]>();
                foreach (var keyValuePair in scheduledTasks)
                {
                    var results = await keyValuePair.Value;
                    resultSet.Add(keyValuePair.Key, results.Value);
                    var names = measurement.Measurements.Select(e => e.Name);
                    measurement.Measurements.AddRange(
                        results.Measurement.Measurements.Where(e => !names.Contains(e.Name)));
                }

                var calcResult = await context.CallActivityAsync<TMC<Matrix>>("BuildResult",
                    new TMC<BuildResultContext>(new BuildResultContext
                    {
                        Calculation = calculation.Value,
                        Results = resultSet
                    }, measurement));
                measurement = calcResult.Measurement;
            }

            return measurement;
        }

        [FunctionName("GenerateMatrix")]
        public static TMC<MatrixCalculation> GenerateMatrix(
            [ActivityTrigger] CalculationConfiguration cfg, ILogger log)
        {
            var s = cfg.MatrixSize;

            log.LogInformation($"Creating Two {cfg}x{cfg} matrices");
            var repo = new InMemoryMatrixMulRepository();
            var hndlr = new FunctionHandler(repo);
            var id = hndlr.CreateMatrix(s, cfg.MaxValue);

            log.LogInformation($"Created MatrixCalculations with ID {id}");

            return new TMC<MatrixCalculation>(repo.GetCalculation(id), hndlr.Measurement);
        }

        [FunctionName("SerialMultiply")]
        public static TMC<Matrix> SerialMultiply(
            [ActivityTrigger] TMC<MatrixCalculation> calculation, ILogger log)
        {
            var repo = new InMemoryMatrixMulRepository();
            repo.StoreCalculation("an_id", calculation.Value);
            var hndlr = new FunctionHandler(repo);
            hndlr.Measurement = calculation.Measurement;

            log.LogInformation("Serially multiplying two matrices");
            hndlr.SerialMultiply("an_id");

            return new TMC<Matrix>(repo.GetResultMatrix("an_id"), hndlr.Measurement);
        }

        [FunctionName("DistributeWork")]
        public static TMC<Dictionary<int, ComputationTask[]>> DistributeWork(
            [ActivityTrigger] TMC<WorkDistributionContext> ctx,
            ILogger log)
        {
            var repo = new InMemoryMatrixMulRepository();
            repo.StoreCalculation("an_id", ctx.Value.Calculation);
            var hndlr = new FunctionHandler(repo);
            hndlr.Measurement = ctx.Measurement;

            log.LogInformation("Scheduling Tasks");
            hndlr.ScheduleMultiplicationTasks("an_id", ctx.Value.WorkerCount);

            return new TMC<Dictionary<int, ComputationTask[]>>(repo.Tasks["an_id"],
                hndlr.Measurement);
        }

        [FunctionName("ParallelMultiply")]
        public static TMC<ComputationResult[]> ParallelMultiply(
            [ActivityTrigger] TMC<ParallelWorkerContext> ctx, ILogger log)
        {
            var repo = new InMemoryMatrixMulRepository();
            repo.StoreCalculation("an_id", ctx.Value.Calculation);
            repo.StoreComputationTasksForWorker("an_id", ctx.Value.WorkerID, ctx.Value.Tasks);
            var hndlr = new FunctionHandler(repo);
            hndlr.Measurement = ctx.Measurement;

            log.LogInformation($"Worker #{ctx.Value.WorkerID} Running parallel Multiplication tasks");
            hndlr.ParallelMultiplyWorker("an_id", ctx.Value.WorkerID);

            return new TMC<ComputationResult[]>(
                repo.GetComputationResults("an_id", ctx.Value.WorkerID), hndlr.Measurement);
        }

        [FunctionName("BuildResult")]
        public static TMC<Matrix> BuildResultMatrix(
            [ActivityTrigger] TMC<BuildResultContext> ctx,
            ILogger log)
        {
            var repo = new InMemoryMatrixMulRepository();
            repo.StoreCalculation("an_id", ctx.Value.Calculation);
            repo.WorkerResults.Add("an_id", ctx.Value.Results);
            var hndlr = new FunctionHandler(repo);
            hndlr.Measurement = ctx.Measurement;

            log.LogInformation($"Building Result Matrix");
            hndlr.BuildResultMatrix("an_id", ctx.Value.Results.Count);

            return new TMC<Matrix>(repo.GetResultMatrix("an_id"), hndlr.Measurement);
        }

        [FunctionName("TriggerMatrixMultiplication")]
        public static async Task<HttpResponseMessage> StartMultiplication(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")]
            HttpRequestMessage msg,
            HttpRequest req,
            [OrchestrationClient] DurableOrchestrationClient starter,
            ILogger log)
        {
            var matrixSize = 125;
            var maxValue = 5000;
            if (req.Query.ContainsKey("size"))
            {
                try
                {
                    matrixSize = int.Parse(req.Query["size"]);
                }
                catch (Exception)
                {
                }
            }

            if (req.Query.ContainsKey("max"))
            {
                try
                {
                    maxValue = int.Parse(req.Query["max"]);
                }
                catch (Exception)
                {
                }
            }

            var hasCallback = req.Query.ContainsKey("callback");
            var callback = "";
            if (hasCallback)
            {
                callback = req.Query["callback"];
            }


            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("OrchestrateMatrixMultiplication",
                new CalculationConfiguration
                {
                    MaxValue = maxValue,
                    MatrixSize = matrixSize,
                    DoCallback = hasCallback,
                    CallbackURL = callback
                });

            log.LogInformation($"Started orchestration with ID = '{instanceId}' with Matrix Size n={matrixSize}.");

            return starter.CreateCheckStatusResponse(msg, instanceId);
        }
    }

    public class TMC<T>
    {
        public TMC(T value, TimeMeasurement measurement)
        {
            Value = value;
            Measurement = measurement;
        }

        public T Value { get; set; }
        public TimeMeasurement Measurement { get; set; }
    }

    public class CalculationConfiguration
    {
        public int MatrixSize { get; set; }
        public int MaxValue { get; set; }
        public bool DoCallback { get; set; }
        public string CallbackURL { get; set; }
    }

    public class WorkDistributionContext
    {
        public MatrixCalculation Calculation { get; set; }
        public int WorkerCount { get; set; }
    }

    public class ParallelWorkerContext
    {
        public MatrixCalculation Calculation { get; set; }
        public ComputationTask[] Tasks { get; set; }
        public int WorkerID { get; set; }
    }

    public class BuildResultContext
    {
        public MatrixCalculation Calculation { get; set; }
        public Dictionary<int, ComputationResult[]> Results { get; set; }
    }
}