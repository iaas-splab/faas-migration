namespace MatrixMul.Core.Interfaces
{
    public interface ISerializer<T>
    {
        byte[] Serialize(T c);
        T Deserialize(byte[] d);
    }
}