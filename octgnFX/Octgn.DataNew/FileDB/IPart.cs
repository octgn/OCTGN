namespace Octgn.DataNew.FileDB
{
    public interface IPart
    {
        PartType Type { get; }
        string ThisPart { get;}
        string PartString();
    }
}