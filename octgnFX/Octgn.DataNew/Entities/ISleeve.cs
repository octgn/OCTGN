using System;

namespace Octgn.DataNew.Entities
{
    public interface ISleeve : ICloneable
    {
        byte[] ImageData { get; set; }

        string Name { get; set; }

        string FilePath { get; set; }

        Guid GameId { get; set; }

        SleeveSource Source { get; set; }
    }
}