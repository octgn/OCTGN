using System;
using System.IO;

namespace Octgn
{
    public static class StreamExtensionMethods
    {
        public static string ReadToEnd(this Stream stream) {
            using(var reader = new StreamReader(stream)) {
                return reader.ReadToEnd();
            }
        }
    }
}
