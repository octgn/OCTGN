using System;

namespace Octgn.Sdk.Packaging
{
    public class OctgnPlugin : IPlugin
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Icon { get; set; }

        public string Path { get; set; }
    }
}
