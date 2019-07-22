using System;
using MatrixMul.Core;

namespace MatrixMul
{
    public class MulTest
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("MatrixMul - Development Test");

            var m = Util.GenerateMatrix(10, (x, y) => x * y % (x + y + 1));

            Console.WriteLine(m.ToString());
        }
    }
}