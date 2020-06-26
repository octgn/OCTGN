using Octgn.Sdk.Data;
using Octgn.Sdk.Packaging;
using System;

namespace Octgn.Sdk
{
    public class DataMapper
    {
        public T Map<T>(PackageFile package) where T : PackageRecord {
            return (T)new PackageRecord {
                Id = package.Id,
                Name = package.Name,
                Icon = package.Icon,
                Website = package.Website,
                Version = package.Version,
                Description = package.Description,
                OctgnVersion = package.OctgnVersion,
                Path = package.Path
            };
        }

        public T Map<T>(PluginFile plugin, string packageId, string packageVersion) where T : PluginRecord {
            return (T)new PluginRecord {
                Id = plugin.Id,
                Description = plugin.Description,
                Icon = plugin.Icon,
                Name = plugin.Name,
                Path = plugin.Path,
                PackageId = packageId,
                Type = plugin.Type,
                PackageVersion = packageVersion
            };
        }
    }
}
