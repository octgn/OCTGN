using System;

namespace Octgn.Sdk.Extensibility
{
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class PluginDetailsAttribute : Attribute
    {
        public string Id { get; }

        public PluginDetailsAttribute(string id) {
            Id = id;
        }
    }
}
