using System;
using System.Collections.Generic;
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

namespace MatrixMul
{
    public static class Functions
    {
        [FunctionName("OrchestrateMatrixMultiplication")]
        public static async Task<Matrix> OrchestrateMultiplication(
            [OrchestrationTrigger] DurableOrchestrationContext context
        )
        {
            var matrixSize = context.GetInput<string>();
            var s = int.Parse(matrixSize);

            var calculation = await context.CallActivityAsync<MatrixCalculation>("GenerateMatrix", matrixSize);

            Matrix result = null;

            if (s < 10)
            {
                result = await context.CallActivityAsync<Matrix>("SerialMultiply", calculation);
            }
            else
            {
                var tasks = await context.CallActivityAsync<Dictionary<int, ComputationTask[]>>("DistributeWork",
                    new WorkDistributionContext
                    {
                        Calculation = calculation,
                        WorkerCount = 5
                    });

                var scheduledTasks = new Dictionary<int, Task<ComputationResult[]>>();
                foreach (var keyValuePair in tasks)
                {
                    scheduledTasks.Add(keyValuePair.Key, context.CallActivityAsync<ComputationResult[]>(
                        "ParallelMultiply", new ParallelWorkerContext
                        {
                            Calculation = calculation,
                            WorkerID = keyValuePair.Key,
                            Tasks = keyValuePair.Value
                        }));
                }

                var resultSet = new Dictionary<int, ComputationResult[]>();
                foreach (var keyValuePair in scheduledTasks)
                {
                    var results = await keyValuePair.Value;
                    resultSet.Add(keyValuePair.Key, results);
                }

                result = await context.CallActivityAsync<Matrix>("BuildResult", new BuildResultContext
                {
                    Calculation = calculation,
                    Results = resultSet
                });
            }
            
            return result;
        }

        [FunctionName("GenerateMatrix")]
        public static MatrixCalculation GenerateMatrix([ActivityTrigger] string size, ILogger log)
        {
            var s = int.Parse(size);

            log.LogInformation($"Creating Two {size}x{size} matrices");
            var repo = new InMemoryMatrixMulRepository();
            var hndlr = new FunctionHandler(repo);
            var id = hndlr.CreateMatrix(s, 500);

            log.LogInformation($"Created MatrixCalculations with ID {id}");

            return repo.GetCalculation(id);
        }

        [FunctionName("SerialMultiply")]
        public static Matrix SerialMultiply([ActivityTrigger] MatrixCalculation calculation, ILogger log)
        {
            var repo = new InMemoryMatrixMulRepository();
            repo.StoreCalculation("an_id", calculation);
            var hndlr = new FunctionHandler(repo);

            log.LogInformation("Serially multiplying two matrices");
            hndlr.SerialMultiply("an_id");

            return repo.GetResultMatrix("an_id");
        }

        [FunctionName("DistributeWork")]
        public static Dictionary<int, ComputationTask[]> DistributeWork([ActivityTrigger] WorkDistributionContext ctx,
            ILogger log)
        {
            var repo = new InMemoryMatrixMulRepository();
            repo.StoreCalculation("an_id", ctx.Calculation);
            var hndlr = new FunctionHandler(repo);

            log.LogInformation("Scheduling Tasks");
            hndlr.ScheduleMultiplicationTasks("an_id", ctx.WorkerCount);

            return repo.Tasks["an_id"];
        }

        [FunctionName("ParallelMultiply")]
        public static ComputationResult[] ParallelMultiply([ActivityTrigger] ParallelWorkerContext ctx, ILogger log)
        {
            var repo = new InMemoryMatrixMulRepository();
            repo.StoreCalculation("an_id", ctx.Calculation);
            repo.StoreComputationTasksForWorker("an_id", ctx.WorkerID, ctx.Tasks);
            var hndlr = new FunctionHandler(repo);

            log.LogInformation($"Worker #{ctx.WorkerID} Running parallel Multiplication tasks");
            hndlr.ParallelMultiplyWorker("an_id", ctx.WorkerID);

            return repo.GetComputationResults("an_id", ctx.WorkerID);
        }

        [FunctionName("BuildResult")]
        public static Matrix BuildResultMatrix([ActivityTrigger] BuildResultContext ctx, ILogger log)
        {
            var repo = new InMemoryMatrixMulRepository();
            repo.StoreCalculation("an_id", ctx.Calculation);
            repo.WorkerResults.Add("an_id", ctx.Results);
            var hndlr = new FunctionHandler(repo);

            log.LogInformation($"Building Result Matrix");
            hndlr.BuildResultMatrix("an_id", ctx.Results.Count);

            return repo.GetResultMatrix("an_id");
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

            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("OrchestrateMatrixMultiplication", matrixSize.ToString());

            log.LogInformation($"Started orchestration with ID = '{instanceId}' with Matrix Size n={matrixSize}.");

            return starter.CreateCheckStatusResponse(msg, instanceId);
        }
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