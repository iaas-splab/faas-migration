using System.IO;
using System.Text;
using System.Threading.Tasks;
using MatrixMul.Core.Interfaces;
using MatrixMul.Core.Model;
using Minio;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MatrixMul
{
    public class S3Repository : IMatrixMulRepository
    {
        private string _bucketName;
        private string _endpoint;
        private string _accessKey;
        private string _secretKey;

        private MinioClient _client;

        public S3Repository(JObject input)
        {
            _bucketName = input["s3_bucket_name"].ToString();
            _endpoint = input["s3_endpoint"].ToString();
            _accessKey = input["s3_access_key"].ToString();
            _secretKey = input["s3_secret_key"].ToString();

            _client = new MinioClient(_endpoint, _accessKey, _secretKey);
        }

        public S3Repository(string bucketName, string endpoint, string accessKey, string secretKey)
        {
            _bucketName = bucketName;
            _endpoint = endpoint;
            _accessKey = accessKey;
            _secretKey = secretKey;

            _client = new MinioClient(_endpoint, _accessKey, _secretKey);
        }

        public void StoreCalculation(string id, MatrixCalculation calculation)
        {
//            var data = JsonConvert.SerializeObject(calculation);
//            var ms = new MemoryStream();
//            ms.Write(Encoding.UTF8.GetBytes(data));
//            ms.Seek(0, SeekOrigin.Begin);
//            Task.WaitAll(_client.PutObjectAsync(_bucketName, id, ms, ms.Length));
        }

        public MatrixCalculation GetCalculation(string id)
        {
            var ms = new MemoryStream();
            Task.WaitAll(_client.GetObjectAsync(_bucketName, id, (e) => e.CopyTo(ms)));

            return JsonConvert.DeserializeObject<MatrixCalculation>(Encoding.UTF8.GetString(ms.ToArray()));
        }

        public void DeleteCalculation(string id)
        {
            Task.WaitAll(_client.RemoveObjectAsync(_bucketName, id));
        }

        public void StoreResultMatrix(string id, Matrix matrix)
        {
            throw new System.NotImplementedException();
        }

        public Matrix GetResultMatrix(string id)
        {
            throw new System.NotImplementedException();
        }

        public bool HasResultMatrix(string id)
        {
            throw new System.NotImplementedException();
        }

        public void StoreComputationTasksForWorker(string id, int workerId, ComputationTask[] tasks)
        {
            throw new System.NotImplementedException();
        }

        public ComputationTask[] GetComputationTasksForWorker(string id, int workerId)
        {
            throw new System.NotImplementedException();
        }

        public void StoreComputationResults(string id, int worker, ComputationResult[] results)
        {
            throw new System.NotImplementedException();
        }

        public ComputationResult[] GetComputationResults(string id, int worker)
        {
            throw new System.NotImplementedException();
        }
    }
}