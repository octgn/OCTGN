﻿using Octgn.Sdk.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Octgn.Sdk.Extensibility.Desktop
{
    public static class ExtensionMethods
    {
        public static IEnumerable<PluginRecord> GameMenuPlugins(this DataContext context, PackageRecord package) {
            var query = context.PackagePlugins(package)
                .Where(plugin =>
                    plugin.Type == "octgn.plugin.gamemenu"
                );
            ;

            foreach (var plugin in query) {
                yield return plugin;
            }

            foreach (var dependencyRecord in package.Dependencies()) {
                var dependantPackage = context.Packages.Find(
                    dependencyRecord.Id,
                    dependencyRecord.Version
                );

                foreach (var yunderPlugin in GameMenuPlugins(context, dependantPackage)) {
                    yield return yunderPlugin;
                }
            }
        }
    }
}
