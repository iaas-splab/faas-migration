using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace MatrixMul.Core.Model
{
    [DataContract]
    public class Matrix
    {
        [DataMember] public int Size { get; set; }
        [DataMember] public List<List<int>> Data { get; set; }

        public string ToString()
        {
            var builder = new StringBuilder();

            builder.Append($"{Size}x{Size} Matrix\n");

            for (var i = 0; i < Data.Count; i++)
            {
                var line = Data[i];
                for (var j = 0; j < line.Count; j++) builder.Append($"{line[i]:D5} ");

                builder.Append("\n");
            }

            return builder.ToString();
        }
    }
}