using System;
using System.IO;
using Amazon.Lambda.Serialization.Json;
using Amazon.S3.Transfer;
using MatrixMul.Core.Interfaces;
using MatrixMul.Core.Model;

namespace MatrixMul.Lambda
{
    public class S3MatrixMulRepository : IMatrixMulRepository
    {
        private readonly string bucketName;
        private readonly JsonSerializer serializer;
        private readonly TransferUtility transferUtility;

        public S3MatrixMulRepository(TransferUtility transferUtility, JsonSerializer serializer, string bucketName)
        {
            this.transferUtility = transferUtility;
            this.serializer = serializer;
            this.bucketName = bucketName;
        }

        public void StoreCalculation(string id, MatrixCalculation calculation)
        {
            var memoryStream = new MemoryStream();
            serializer.Serialize(calculation, memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            transferUtility.Upload(memoryStream, bucketName, id);
        }

        public MatrixCalculation GetCalculation(string id)
        {
            var c = serializer.Deserialize<MatrixCalculation>(transferUtility.OpenStream(bucketName, id));
            return c;
        }

        public void StoreResultMatrix(string id, Matrix matrix)
        {
            var memoryStream = new MemoryStream();
            serializer.Serialize(matrix, memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            transferUtility.Upload(memoryStream, bucketName, GetResultKey(id));
        }

        public Matrix GetResultMatrix(string id)
        {
            return serializer.Deserialize<Matrix>(transferUtility.OpenStream(bucketName, GetResultKey(id)));
        }

        public bool HasResultMatrix(string id)
        {
            return true;
        }

        public void StoreComputationTasksForWorker(string id, int workerId, ComputationTask[] tasks)
        {
            var memoryStream = new MemoryStream();
            serializer.Serialize(tasks, memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            transferUtility.Upload(memoryStream, bucketName, GetTaskKeyForWorker(id, workerId));
        }

        public ComputationTask[] GetComputationTasksForWorker(string id, int workerId)
        {
            return serializer.Deserialize<ComputationTask[]>(transferUtility.OpenStream(bucketName,
                GetTaskKeyForWorker(id, workerId)));
        }

        public void DeleteCalculation(string id)
        {
            transferUtility.S3Client.DeleteObjectAsync(bucketName, id).Wait();
        }

        public void StoreComputationResults(string id, int worker, ComputationResult[] results)
        {
            var memoryStream = new MemoryStream();
            serializer.Serialize(results, memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            transferUtility.Upload(memoryStream, bucketName, GetResultKeyForWorker(id, worker));
        }

        public ComputationResult[] GetComputationResults(string id, int workerId)
        {
            return serializer.Deserialize<ComputationResult[]>(transferUtility.OpenStream(bucketName,
                GetResultKeyForWorker(id, workerId)));
        }

        private static string GetResultKey(string id)
        {
            return $"{id}_result";
        }

        private static string GetTaskKeyForWorker(string id, int workerId)
        {
            return $"{id}_tasks_worker_{workerId}";
        }

        private static string GetResultKeyForWorker(string id, int workerId)
        {
            return $"{id}_results_worker_{workerId}";
        }
    }
}