using System;
using System.IO;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Octgn.Sdk.Packaging.Readers
{
    public class YamlPackageFileReader : IPackageFileReader
    {
        private readonly IDeserializer _deserializer;

        public YamlPackageFileReader() {
            var builder = new DeserializerBuilder()
                .WithNamingConvention(LowerCaseNamingConvention.Instance)
            ;

            _deserializer = builder.Build();
        }
        public bool CanRead(string path) {
            return
                path.EndsWith("yaml", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith("yml", StringComparison.OrdinalIgnoreCase);
        }

        public PackageFile ReadStream(string path, Stream stream) {
            if (!CanRead(path)) throw new UnsupportedPackageFileException($"Unsupported package file {path}");

            using (var reader = new StreamReader(stream, Encoding.UTF8, true, 4096, true)) {
                var package = _deserializer.Deserialize<PackageFile>(reader);

                package.Path = path;

                return package;
            }
        }
    }
}
