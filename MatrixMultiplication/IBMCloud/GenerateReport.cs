using System;
using MatrixMul.Core;
using Newtonsoft.Json.Linq;

namespace MatrixMul.IBMCloud
{
    public class GenerateReport
    {
        public JObject Main(JObject args)
        {
            try
            {
                var repo = new S3Repository(args);
                var hndlr = new FunctionHandler(repo);

                var callback = "";
                if (args.ContainsKey("hasCallback") && args["hasCallback"].ToString() == "true")
                {
                    callback = args["callback"].ToString();
                }
                else
                {
                    callback = null;
                }

                var start = long.Parse(args["startTimestamp"].ToString());

                var report = hndlr.GenerateReport(callback, start, args["id"].ToString(),
                    int.Parse(args["worker_count"].ToString()));

                var l = new JObject(report);

                Console.WriteLine(l.ToString());
                return l;
            }
            catch (Exception e)
            {
                var j = new JObject();
                j["error"] = e.ToString();
                return j;
            }
        }
    }
}