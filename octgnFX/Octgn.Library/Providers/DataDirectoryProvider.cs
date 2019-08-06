using log4net;
using System;
using System.IO;
using System.Reflection;

namespace Octgn.Library.Providers
{
    public class DataDirectoryProvider
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _rootDirectory;

        private readonly string _dataPathFile;

        private readonly string _defaultDataDirectory;

        public DataDirectoryProvider() {
            _rootDirectory = Path.GetDirectoryName(typeof(Config).Assembly.Location);

            _dataPathFile = Path.Combine(_rootDirectory, "data.path");

            _defaultDataDirectory = Path.Combine(_rootDirectory, "Data");
        }

        public string Get() {
            string dataDirectory = null;

            if (GetDataDirectoryFromEnvironment(ref dataDirectory)) {
                return dataDirectory;
            }

            if (GetDataDirectoryFromDataPath(ref dataDirectory)) {
                return dataDirectory;
            }

            if (GetDataDirectoryFromOldDefault(ref dataDirectory)) {
                return dataDirectory;
            }

            dataDirectory = _defaultDataDirectory;

            WriteDataPathFile(_dataPathFile, dataDirectory);

            return dataDirectory;
        }


        public const string OCTGNDATA_ENVIRONMENTALVARIABLE = "OCTGN_DATA";

        private bool GetDataDirectoryFromEnvironment(ref string dataDirectory) {
            dataDirectory = Environment.GetEnvironmentVariable(OCTGNDATA_ENVIRONMENTALVARIABLE, EnvironmentVariableTarget.Process);

            if (dataDirectory != null) {
                dataDirectory = CorrectDataPath(dataDirectory);

                ValidateDataPath(dataDirectory);

                return true;
            }

            dataDirectory = Environment.GetEnvironmentVariable(OCTGNDATA_ENVIRONMENTALVARIABLE, EnvironmentVariableTarget.User);

            if (dataDirectory != null) {
                dataDirectory = CorrectDataPath(dataDirectory);

                ValidateDataPath(dataDirectory);

                return true;
            }

            dataDirectory = Environment.GetEnvironmentVariable(OCTGNDATA_ENVIRONMENTALVARIABLE, EnvironmentVariableTarget.Machine);

            if (dataDirectory != null) {
                dataDirectory = CorrectDataPath(dataDirectory);

                ValidateDataPath(dataDirectory);

                return true;
            }

            return false;
        }

        private bool GetDataDirectoryFromDataPath(ref string dataDirectory) {
            dataDirectory = ReadDataPathFile(_dataPathFile);

            return dataDirectory != null;
        }

        private bool GetDataDirectoryFromOldDefault(ref string dataDirectory) {
            var oldLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Octgn");

            var oldConfigLocation = Path.Combine(oldLocation, "Config", "settings.json");

            if (File.Exists(oldConfigLocation)) {
                dataDirectory = oldLocation;

                WriteDataPathFile(_dataPathFile, dataDirectory);

                return true;
            }

            return false;
        }

        private string ReadDataPathFile(string dataPathFile) {
            if (!File.Exists(dataPathFile)) return null;

            var path = File.ReadAllText(dataPathFile);

            if (string.IsNullOrWhiteSpace(path))
                throw new InvalidOperationException($"File '{dataPathFile}' is corrupt, it's empty");

            Log.Info($"Read '{path}' from data path file '{dataPathFile}'");

            path = CorrectDataPath(path);

            Log.Info($"Corrected '{path}'");

            ValidateDataPath(path);

            return path;
        }

        private void WriteDataPathFile(string dataPathFile, string path) {
            Log.Info($"Writing '{dataPathFile}' with the contents '{path}'");

            File.WriteAllText(dataPathFile, path);
        }

        private string CorrectDataPath(string path) {
            var correctedPath = path;

            if (string.IsNullOrWhiteSpace(correctedPath))
                return path;

            correctedPath = Environment.ExpandEnvironmentVariables(correctedPath);

            if (!Path.IsPathRooted(correctedPath)) {
                correctedPath = Path.Combine(_rootDirectory, correctedPath);
            }

            if ('/' != Path.DirectorySeparatorChar) {
                correctedPath = correctedPath.Replace('/', Path.DirectorySeparatorChar);
            }

            if ('\\' != Path.DirectorySeparatorChar) {
                correctedPath = correctedPath.Replace('\\', Path.DirectorySeparatorChar);
            }

            correctedPath = correctedPath.TrimEnd(Path.DirectorySeparatorChar);

            return correctedPath;
        }

        private void ValidateDataPath(string path) {
            if (string.IsNullOrWhiteSpace(path))
                throw new InvalidOperationException($"Data Path is empty.");

            if (!Path.IsPathRooted(path))
                throw new InvalidOperationException($"Data Path '{path}' should not be relative.");

            if (File.Exists(path))
                throw new InvalidOperationException($"Data Path '{path}' is a file that already exists. It should be a directory.");
        }

    }
}
