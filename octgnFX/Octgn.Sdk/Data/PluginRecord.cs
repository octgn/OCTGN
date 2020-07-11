using System;
using System.Diagnostics;

namespace Octgn.Sdk.Data
{
    [DebuggerDisplay("PluginRecord({PackageId}.{Id}@{PackageVersion}")]
    public class PluginRecord : IPluginDetails
    {
        public string Id { get; set; }

        public string PackageId { get; set; }

        public string PackageVersion { get; set; }

        public string Type { get; set; }

        public string Format { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Icon { get; set; }

        public string Path { get; set; }
    }
}