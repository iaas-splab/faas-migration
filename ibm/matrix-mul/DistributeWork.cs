using System;
using Newtonsoft.Json.Linq;

namespace MatrixMul
{
    public class DistributeWork
    {
        public JObject Main(JObject args)
        {
            Console.WriteLine(args.ToString());
            return args;
        }
    }
}