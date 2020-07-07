using System;
using System.Collections.Generic;

namespace Octgn.Sdk.Extensibility
{
    public interface IPlugin : IDisposable
    {
        IPluginDetails Details { get; set; }

        Package Package { get; }

        void Initialize(Package package);

        void Start();
    }
}