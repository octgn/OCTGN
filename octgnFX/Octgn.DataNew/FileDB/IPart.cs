namespace Octgn.DataNew.FileDB
{
    using System;

    public interface IPart
    {
        PartType PartType { get; }

        Type Type { get; }

        string ThisPart { get;}
        string PartString();
    }
}