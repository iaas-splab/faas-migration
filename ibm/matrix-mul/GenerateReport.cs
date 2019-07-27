using System;
using Newtonsoft.Json.Linq;

namespace MatrixMul
{
    public class GenerateReport
    {
        public JObject Main(JObject args)
        {
            Console.WriteLine(args.ToString());
            return args;
        }
    }
}