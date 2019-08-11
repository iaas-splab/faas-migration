using System;
using MatrixMul.Core;
using Newtonsoft.Json.Linq;

namespace MatrixMul.IBMCloud
{
    public class DistributeWork
    {
        public JObject Main(JObject args)
        {
            var repo = new S3Repository(args);
            var hndlr = new FunctionHandler(repo);

            hndlr.ScheduleMultiplicationTasks(args["id"].ToString(), int.Parse(args["worker_count"].ToString()));

            Console.WriteLine(args.ToString());
            return args;
        }
    }
}