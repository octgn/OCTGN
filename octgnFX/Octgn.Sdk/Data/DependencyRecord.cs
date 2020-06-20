using System;

namespace Octgn.Sdk.Data
{
    public class DependencyRecord
    {
        public string Id { get; set; }

        public string Version { get; set; }

        public override string ToString() => $"{Id}@{Version}";
    }
}
