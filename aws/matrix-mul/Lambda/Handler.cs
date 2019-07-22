using System;
using System.Runtime.Serialization;
using Amazon;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;
using Amazon.S3;
using Amazon.S3.Transfer;
using MatrixMul.Core;
using MatrixMul.Core.Interfaces;

[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace MatrixMul.Lambda
{
    public class Handler
    {
        private string bucketName;
        private readonly string region;
        private readonly IAmazonS3 s3Client;
        private JsonSerializer serializer;
        private TransferUtility transferUtility;

        private IMatrixMulRepository _mulRepository;
        private FunctionHandler handler;

        public Handler()
        {
            bucketName = Environment.GetEnvironmentVariable("DATA_BUCKET_NAME");
            region = Environment.GetEnvironmentVariable("DATA_BUCKET_REGION");
            s3Client = new AmazonS3Client(RegionEndpoint.GetBySystemName(region));
            transferUtility = new TransferUtility(s3Client);
            serializer = new JsonSerializer();

            _mulRepository = new S3MatrixMulRepository(transferUtility, serializer, bucketName);
            handler = new FunctionHandler(_mulRepository);
        }

        public FunctionContext CreateMatrix(FunctionContext ctx)
        {
            if (ctx.MatrixSize == 0)
            {
                Console.WriteLine("No Values Set. Using Defaults");
                ctx.MatrixSize = 200;
                ctx.MaxValue = 150;
            }

            var id = handler.CreateMatrix(ctx.MatrixSize, ctx.MaxValue);
            var res = new FunctionContext
            {
                CalculationID = id,
                MatrixSize = ctx.MatrixSize,
                MaxValue = ctx.MaxValue,
            };

            return res;
        }

        public FunctionContext SerialMultiply(FunctionContext ctx)
        {
            handler.SerialMultiply(ctx.CalculationID);
            return ctx;
        }

        public FunctionContext ScheduleMultiplyTasks(FunctionContext ctx)
        {
            int workerCnt = int.Parse(ctx.WorkerCount);
            handler.ScheduleMultiplicationTasks(ctx.CalculationID, workerCnt);
            return ctx;
        }

        public FunctionContext MultiplyTasksWorker(FunctionContext ctx)
        {
            int workerId = int.Parse(ctx.WorkerID);
            handler.ParallelMultiplyWorker(ctx.CalculationID, workerId);
            return ctx;
        }

        public FunctionContext BuildResultMatrix(FunctionContext ctx)
        {
            int workerCnt = int.Parse(ctx.WorkerCount);
            handler.BuildResultMatrix(ctx.CalculationID, workerCnt);

            return ctx;
        }

        public FunctionContext BuildReport(FunctionContext ctx)
        {
            return ctx;
        }
    }

    [DataContract]
    public class FunctionContext
    {
        [DataMember(IsRequired = false)] public int MatrixSize { get; set; }
        [DataMember(IsRequired = false)] public int MaxValue { get; set; }
        [DataMember(IsRequired = false)] public string CalculationID { get; set; }

        [DataMember(IsRequired = false)] public string WorkerID { get; set; }
        [DataMember(IsRequired = false)] public string WorkerCount { get; set; }
    }
}