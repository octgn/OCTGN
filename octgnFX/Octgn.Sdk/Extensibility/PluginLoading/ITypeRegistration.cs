using System;

namespace Octgn.Sdk.Extensibility.PluginLoading
{
    public interface ITypeRegistration
    {
        void Register(Type type, params Type[] relatedTypes);

        void Unregister(Type type);
    }
}
