using System;
using System.IO;
using System.Xml.Serialization;

namespace Octgn.Sdk.Packaging.Readers
{
    public class XmlPackageFileReader : IPackageFileReader
    {
        private readonly XmlSerializer _serializer;

        public XmlPackageFileReader() {
            _serializer = new XmlSerializer(typeof(PackageFile));
        }

        public bool CanRead(string path) {
            return
                path.EndsWith("xml", StringComparison.OrdinalIgnoreCase);
        }

        public PackageFile ReadStream(string path, Stream stream) {
            if (!CanRead(path)) throw new UnsupportedPackageFileException($"Unsupported package file {path}");

            var package = (PackageFile)_serializer.Deserialize(stream);

            package.Path = path;

            return package;
        }
    }
}
