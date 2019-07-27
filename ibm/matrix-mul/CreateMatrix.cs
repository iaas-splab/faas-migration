using System;
using MatrixMul.Core;
using Newtonsoft.Json.Linq;

namespace MatrixMul
{
    public class CreateMatrix
    {
        public JObject Main(JObject args)
        {
            var size = args.ContainsKey("size") ? int.Parse(args["size"].ToString()) : 50;
            var max = args.ContainsKey("max") ? int.Parse(args["max"].ToString()) : 5000;

            var repo = new S3Repository(args);
            var hndlr = new FunctionHandler(repo);

            var id = hndlr.CreateMatrix(size, max);

            args["id"] = id;
            args["size"] = size;
            args["max"] = max;
            
            Console.WriteLine(args.ToString());
            
            return args;
        }
    }
}