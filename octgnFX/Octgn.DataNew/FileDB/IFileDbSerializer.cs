namespace Octgn.DataNew.FileDB
{
    public interface IFileDbSerializer
    {
        ICollectionDefinition Def { get; set; }

        object Deserialize(string fileName);

        byte[] Serialize(object obj);
    }
}