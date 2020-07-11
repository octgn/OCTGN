using Octgn.Sdk.Extensibility.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Octgn.Sdk.Extensibility.PluginLoading
{
    public class PluginLoader
    {
        public IEnumerable<(string Format, IPluginFormat)> Registrations {
            get {
                foreach (var loaderRegistration in _formats) {
                    yield return (
                        loaderRegistration.Key,
                        loaderRegistration.Value);
                }
            }
        }

        public IReadOnlyDictionary<string, Type> PluginTypes => (IReadOnlyDictionary<string, Type>)_pluginTypes;

        private bool _isInitialized;

        private readonly IDictionary<string, IPluginFormat> _formats;
        private readonly IDictionary<string, Type> _pluginTypes;

        public PluginLoader() {
            _formats = new Dictionary<string, IPluginFormat>();
            _pluginTypes = new Dictionary<string, Type>();

            var integrated = new IntegratedPluginFormat();
            _formats[integrated.Id] = integrated;
        }

        public void Initialize(PluginIntegration integration) {
            //TODO: null checks

            if (_isInitialized) throw new AlreadyInitializedException($"{this} was already initialized.");
            _isInitialized = true;

            //TODO: Move to constant somewhere
            RegisterPluginType("octgn.plugin", typeof(Plugin));
            RegisterPluginType("octgn.plugin.menu", typeof(MenuPlugin));
            RegisterPluginType("octgn.plugin.format", typeof(IPluginFormat));

            RegisterPluginType("octgn.plugin.game", typeof(Plugin));
            RegisterPluginFormat(new JodsEngineGamePluginFormat());
        }

        public IPlugin Load(Package package, IPluginDetails pluginDetails) {
            //TODO: null checks

            if (!_isInitialized) throw new NotInitializedException($"{this} is not initialized.");

            if (!_pluginTypes.TryGetValue(pluginDetails.Type, out var pluginType)) {
                throw new UnknownPluginTypeException($"Plugin type {pluginDetails.Type} is unknown.");
            }

            var loader = GetFormat(pluginDetails.Format);

            var plugin = loader.Load(package, pluginDetails, pluginType);

            if (plugin.Details == null) {
                plugin.Details = pluginDetails;
            }

            return plugin;
        }

        public void RegisterPluginType(string pluginTypeId) {
            //TODO: null checks

            if (!_isInitialized) throw new NotInitializedException($"{this} is not initialized.");

            _pluginTypes[pluginTypeId] = null;
        }

        public void RegisterPluginType(
            string pluginTypeId,
            Type pluginType,
            params Type[] additionalTypes) {
            //TODO: null checks

            if (!_isInitialized) throw new NotInitializedException($"{this} is not initialized.");

            _pluginTypes[pluginTypeId] = pluginType;

            foreach (var format in _formats.Values.OfType<ITypeRegistration>()) {
                format.Register(pluginType, additionalTypes);
            }
        }

        public void UnregisterPluginType(
            string pluginTypeId,
            Type pluginType) {
            //TODO: null checks

            if (!_isInitialized) throw new NotInitializedException($"{this} is not initialized.");

            _pluginTypes.Remove(pluginTypeId);

            foreach (var format in _formats.Values.OfType<ITypeRegistration>()) {
                format.Unregister(pluginType);
            }
        }

        public void RegisterPluginFormat(IPluginFormat loader) {
            //TODO: null checks

            if (!_isInitialized) throw new NotInitializedException($"{this} is not initialized.");

            _formats[loader.Id] = loader;
        }

        public IPluginFormat GetFormat(string format) {
            //TODO: null checks

            if (!_isInitialized) throw new NotInitializedException($"{this} is not initialized.");

            if (_formats.TryGetValue(format, out var loader)) {
                return loader;
            }

            throw new UnsupportedPluginFormatException($"Plugin format {format} is unsupported.");
        }

        public void UnregisterPluginFormat(string format) {
            //TODO: null checks

            if (!_isInitialized) throw new NotInitializedException($"{this} is not initialized.");

            _formats.Remove(format);
        }
    }
}
