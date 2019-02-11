using System;
using System.IO;

namespace Octgn.LogExporter
{
    public class LogFile
    {
        private readonly string _path;

        public LogFile(string directory) {
            _path = Path.Combine(directory, "Export.log");

            if (File.Exists(_path))
                throw new InvalidOperationException($"File {_path} already exists.");
        }

        public void Info(string message) {
            WriteLine("INFO ", message);
        }

        public void Error(string message) {
            WriteLine("ERROR", message);
        }
        
        public void Error(string message, Exception ex) {
            WriteLine("ERROR", message + Environment.NewLine + ex.ToString());
        }

        private void WriteLine(string eventType, string line) {
            using (var logFileStream = File.Open(_path, FileMode.Append, FileAccess.Write, FileShare.Read))
            using (var logStreamWriter = new StreamWriter(logFileStream)) {
                logStreamWriter.WriteLine(eventType + ": " + DateTime.Now.ToString("u") + ": " + line);
            }
        }
    }
}
