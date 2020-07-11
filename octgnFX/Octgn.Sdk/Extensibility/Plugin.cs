using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Octgn.Sdk.Extensibility
{
    [DebuggerDisplay("Plugin({Details.Id})")]
    public class Plugin : IPlugin
    {
        private bool _isDisposed;

        [XmlIgnore]
        public IPluginDetails Details { get; set; }

        [XmlIgnore]
        public Package Package { get; private set; }

        public Plugin() {

        }

        public virtual void Initialize(Package package) {
            Package = package ?? throw new ArgumentNullException(nameof(package));
        }

        public virtual void Start() {
        }

        protected virtual void Dispose(bool disposing) {
            if (!_isDisposed) {
                if (disposing) {
                }

                _isDisposed = true;
            }
        }

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
