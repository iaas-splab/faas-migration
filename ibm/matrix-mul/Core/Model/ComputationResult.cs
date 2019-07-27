using System.Runtime.Serialization;

namespace MatrixMul.Core.Model
{
    [DataContract]
    public class ComputationResult : ComputationTask
    {
        [DataMember] public int Result { get; set; }
    }
}