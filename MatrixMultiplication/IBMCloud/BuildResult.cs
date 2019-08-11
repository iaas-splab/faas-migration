using System;
using Newtonsoft.Json.Linq;

namespace MatrixMul.IBMCloud
{
    public class BuildResult
    {
        public JObject Main(JObject args)
        {
            Console.WriteLine(args.ToString());
            return args;
        }
    }
}