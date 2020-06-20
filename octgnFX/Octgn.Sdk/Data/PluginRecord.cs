using System;

namespace Octgn.Sdk.Data
{
    public class PluginRecord
    {
        public string Id { get; set; }

        public string PackageId { get; set; }

        public string PackageVersion { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Icon { get; set; }

        public string Path { get; set; }
    }
}