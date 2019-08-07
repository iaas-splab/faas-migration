using System;
using System.Collections.Generic;
using MatrixMul.Core.Model;

namespace MatrixMul.Core
{
    public class Util
    {
        public static long GetUnixTimestamp()
        {
            return (long) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        public static string GenerateUUID()
        {
            return Guid.NewGuid().ToString();
        }

        public static Matrix GenerateMatrix(int n, Func<int, int, int> genFunc)
        {
            Console.WriteLine("Creating Matrix");
            var datamatrix = new List<List<int>>();
            for (var x = 0; x < n; x++)
            {
                Console.WriteLine($"Filling Row {x}");
                var l = new List<int>();
                for (var y = 0; y < n; y++) l.Add(genFunc(x, y));

                datamatrix.Add(l);
            }

            return new Matrix
            {
                Data = datamatrix,
                Size = n
            };
        }
    }
}