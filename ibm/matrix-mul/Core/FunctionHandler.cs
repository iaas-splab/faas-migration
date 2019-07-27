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

        public FunctionHandler(IMatrixMulRepository datastore)
        {
            this.datastore = datastore;
        }

        public string CreateMatrix(int size, int maxValue = 50000)
        {
            var uid = Guid.NewGuid().ToString();
            var mtxA = Util.GenerateMatrix(size, (x, y) => rnd.Next(maxValue));
            var mtxB = Util.GenerateMatrix(size, (x, y) => rnd.Next(maxValue));

            var c = new MatrixCalculation
            {
                A = mtxA,
                B = mtxB
            };

            datastore.StoreCalculation(uid, c);

            return uid;
        }

        public void SerialMultiply(string id)
        {
            var calc = datastore.GetCalculation(id);

            var result = cHandler.SerialMultiply(calc);
            datastore.StoreResultMatrix(id, result);
        }

        public void ScheduleMultiplicationTasks(string id, int workerCount)
        {
            var calc = datastore.GetCalculation(id);

            foreach (var workerTasks in cHandler.BuildTasks(workerCount, calc))
            {
                Console.WriteLine($"Scheduling worker {workerTasks.Key}");
                this.datastore.StoreComputationTasksForWorker(id, workerTasks.Key, workerTasks.Value.ToArray());
            }
        }

        public void ParallelMultiplyWorker(string id, int workerId)
        {
            var tasks = datastore.GetComputationTasksForWorker(id, workerId);
            var calc = datastore.GetCalculation(id);
            var results = cHandler.PerformCalculations(workerId, new List<ComputationTask>(tasks), calc);

            datastore.StoreComputationResults(id, workerId, results.ToArray());
        }

        public void BuildResultMatrix(string id, int workerCount)
        {
            var calc = datastore.GetCalculation(id);

            List<List<ComputationResult>> results = new List<List<ComputationResult>>();
            for (int i = 0; i < workerCount; i++)
            {
                var data = datastore.GetComputationResults(id, i);
                results.Add(new List<ComputationResult>(data));
            }

            var rMatrix = cHandler.BuildResultMatrix(calc, results);
            datastore.StoreResultMatrix(id, rMatrix);
        }
    }
}