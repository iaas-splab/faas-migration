using System;
using MatrixMul.Core;
using Newtonsoft.Json.Linq;

namespace MatrixMul.IBMCloud
{
    public class SerialMultiply
    {
        public JObject Main(JObject args)
        {
            var repo = new S3Repository(args);
            var hndlr = new FunctionHandler(repo);

            hndlr.SerialMultiply(args["id"].ToString());

            Console.WriteLine(args.ToString());
            return args;
        }
    }
}