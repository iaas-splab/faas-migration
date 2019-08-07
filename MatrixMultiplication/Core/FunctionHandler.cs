using System;
using System.Collections.Generic;
using MatrixMul.Core.Interfaces;
using MatrixMul.Core.Model;

namespace MatrixMul.Core
{
    public class FunctionHandler
    {
        private readonly IMatrixMulRepository datastore;
        private readonly Random rnd = new Random();

        private ComputationHandler cHandler = new ComputationHandler();

        public TimeMeasurement Measurement { get; set; }

        public FunctionHandler(IMatrixMulRepository datastore)
        {
            this.datastore = datastore;
            this.Measurement = new TimeMeasurement();
        }

        public string CreateMatrix(int size, int maxValue = 50000)
        {
            var start = Util.GetUnixTimestamp();
            var uid = Guid.NewGuid().ToString();
            var mtxA = Util.GenerateMatrix(size, (x, y) => rnd.Next(maxValue));
            var mtxB = Util.GenerateMatrix(size, (x, y) => rnd.Next(maxValue));

            var c = new MatrixCalculation
            {
                A = mtxA,
                B = mtxB
            };

            datastore.StoreCalculation(uid, c);

            Measurement.AddMeasurement("CreateMatrix", start);
            return uid;
        }

        public void SerialMultiply(string id)
        {
            var start = Util.GetUnixTimestamp();
            var calc = datastore.GetCalculation(id);

            var result = cHandler.SerialMultiply(calc);
            datastore.StoreResultMatrix(id, result);
            Measurement.AddMeasurement("SerialMultiply", start);
        }

        public void ScheduleMultiplicationTasks(string id, int workerCount)
        {
            var start = Util.GetUnixTimestamp();
            var calc = datastore.GetCalculation(id);

            foreach (var workerTasks in cHandler.BuildTasks(workerCount, calc))
            {
                Console.WriteLine($"Scheduling worker {workerTasks.Key}");
                this.datastore.StoreComputationTasksForWorker(id, workerTasks.Key, workerTasks.Value.ToArray());
            }

            Measurement.AddMeasurement("DistributeWork", start);
        }

        public void ParallelMultiplyWorker(string id, int workerId)
        {
            var start = Util.GetUnixTimestamp();
            var tasks = datastore.GetComputationTasksForWorker(id, workerId);
            var calc = datastore.GetCalculation(id);
            var results = cHandler.PerformCalculations(workerId, new List<ComputationTask>(tasks), calc);

            datastore.StoreComputationResults(id, workerId, results.ToArray());
            Measurement.AddMeasurement("ParallelMultiplicaton-Worker-" + workerId, start);
        }

        public void BuildResultMatrix(string id, int workerCount)
        {
            var start = Util.GetUnixTimestamp();
            var calc = datastore.GetCalculation(id);

            List<List<ComputationResult>> results = new List<List<ComputationResult>>();
            for (int i = 0; i < workerCount; i++)
            {
                var data = datastore.GetComputationResults(id, i);
                results.Add(new List<ComputationResult>(data));
            }

            var rMatrix = cHandler.BuildResultMatrix(calc, results);
            datastore.StoreResultMatrix(id, rMatrix);
            Measurement.AddMeasurement("BuildResult", start);
        }
    }
}