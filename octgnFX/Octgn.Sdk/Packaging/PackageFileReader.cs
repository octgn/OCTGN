using Octgn.Sdk.Packaging.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Octgn.Sdk.Packaging
{
    public class PackageFileReader : IPackageFileReader
    {
        public IList<IPackageFileReader> Readers { get; }

        public PackageFileReader() {
            Readers = new List<IPackageFileReader>() {
                new YamlPackageFileReader(),
                new XmlPackageFileReader()
            };
        }

        public virtual bool CanRead(string path) {
            foreach (var reader in Readers) {
                if (reader.CanRead(path)) return true;
            }

            return false;
        }

        public virtual PackageFile ReadStream(string path, Stream stream) {
            var reader = Readers.FirstOrDefault(r => r.CanRead(path));

            if (reader == null) throw new UnsupportedPackageFileException($"Package {path} is an unknown package file type.");

            return reader.ReadStream(path, stream);
        }

        public virtual PackageFile ReadFile(string path) {
            using (var reader = File.OpenRead(path)) {
                return ReadStream(path, reader);
            }
        }
    }
}
