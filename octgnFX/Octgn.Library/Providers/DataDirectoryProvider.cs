/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

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

            _defaultDataDirectory = "Data";
        }

        public string Get() {
            string dataDirectory = null;

            if (GetDataDirectoryFromEnvironment(ref dataDirectory)) {
                Log.Info($"Got Data Directory from OCTGN_DATA: {dataDirectory}");

                var corrected = CorrectDataPath(dataDirectory);

                Log.Info($"Corrected: {corrected}");

                ValidateDataPath(corrected);

                return dataDirectory;
            }

            if (GetDataDirectoryFromDataPath(ref dataDirectory, out var dataPath)) {
                Log.Info($"Got Data Directory from {dataPath}: {dataDirectory}");

                var corrected = CorrectDataPath(dataDirectory);

                Log.Info($"Corrected: {corrected}");

                ValidateDataPath(corrected);

                return dataDirectory;
            }

            if (GetDataDirectoryFromOldDefault(ref dataDirectory)) {
                Log.Info($"Got Data Directory from old default: {dataDirectory}");

                WriteDataPathFile(_dataPathFile, dataDirectory);

                return dataDirectory;
            }

            dataDirectory = CorrectDataPath(_defaultDataDirectory);

            ValidateDataPath(dataDirectory);

            WriteDataPathFile(_dataPathFile, _defaultDataDirectory);

            return _defaultDataDirectory;
        }


        public const string OCTGNDATA_ENVIRONMENTALVARIABLE = "OCTGN_DATA";

        private bool GetDataDirectoryFromEnvironment(ref string dataDirectory) {
            dataDirectory = Environment.GetEnvironmentVariable(OCTGNDATA_ENVIRONMENTALVARIABLE, EnvironmentVariableTarget.Process);

            if (dataDirectory != null) return true;

            dataDirectory = Environment.GetEnvironmentVariable(OCTGNDATA_ENVIRONMENTALVARIABLE, EnvironmentVariableTarget.User);

            if (dataDirectory != null) return true;

            dataDirectory = Environment.GetEnvironmentVariable(OCTGNDATA_ENVIRONMENTALVARIABLE, EnvironmentVariableTarget.Machine);

            if (dataDirectory != null) return true;

            return false;
        }

        private bool GetDataDirectoryFromDataPath(ref string dataDirectory, out string dataPath) {
            dataDirectory = ReadDataPathFile(dataPath = _dataPathFile);

            return dataDirectory != null;
        }

        private bool GetDataDirectoryFromOldDefault(ref string dataDirectory) {
            var oldLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Octgn");

            var oldConfigLocation = Path.Combine(oldLocation, "Config", "settings.json");

            if (File.Exists(oldConfigLocation)) {
                dataDirectory = oldLocation;

                return true;
            }

            return false;
        }

        public void Set(string dataDirectory) {
            if (string.IsNullOrWhiteSpace(dataDirectory)) {
                dataDirectory = _defaultDataDirectory;
            }

            var corrected = CorrectDataPath(dataDirectory);

            ValidateDataPath(corrected);

            WriteDataPathFile(_dataPathFile, dataDirectory);
        }

        private string ReadDataPathFile(string dataPathFile) {
            if (!File.Exists(dataPathFile)) return null;

            var path = File.ReadAllText(dataPathFile);

            if (string.IsNullOrWhiteSpace(path))
                throw new InvalidOperationException($"File '{dataPathFile}' is corrupt, it's empty");

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
