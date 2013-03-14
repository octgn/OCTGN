namespace Octgn.DataNew.FileDB
{
    public interface IFileDbSerializer
    {
        object Deserialize(string fileName);

        byte[] Serialize(object obj);
    }
}