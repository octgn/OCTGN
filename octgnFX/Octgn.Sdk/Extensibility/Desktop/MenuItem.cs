using Csv;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Octgn.Sdk.Extensibility.Desktop
{
    [XmlRoot("MenuItem")]
    public class MenuItem
    {
        [XmlAttribute("Id")]
        public string Id { get; set; }

        [XmlAttribute("Text")]
        public string Text { get; set; }

        [XmlAttribute("Click")]
        public string Click { get; set; }

        [XmlIgnore]
        public MenuPlugin Menu { get; set; }

        public virtual Task OnClick(ClickContext context) {
            return Task.Run(() => OnClickSync(context));
        }

        private void OnClickSync(ClickContext context) {
            if (string.IsNullOrWhiteSpace(Click))
                return;


            if (!Click.StartsWith("launch ")) {
                return;
            }

            var launchString = Click.Substring(7).Trim();

            if (string.IsNullOrWhiteSpace(launchString)) {
                //TODO:Log.Warn
                return;
            }

            (var path, var args) = ParseLaunchString(launchString);

            path = Environment.ExpandEnvironmentVariables(path);

            if (!System.IO.Path.IsPathRooted(path)) {
                var root = Path.GetDirectoryName(Menu.Package.Record.Path);

                path = System.IO.Path.Combine(root, path);
            }

            RunProcess(path, args);
        }

        private (string Path, string Args) ParseLaunchString(string launchString) {
            launchString = launchString.Trim();

            var path = ReadLaunchStringPath(launchString, out var pathEnd);

            var argsString = launchString.Substring(pathEnd);

            return (path, argsString);
        }

        private string ReadLaunchStringPath(string launchString, out int pathEnd) {
            launchString = launchString.TrimStart();

            if (launchString.Length == 0) {
                pathEnd = 0;

                return string.Empty;
            }

            var endChar = ' ';
            var quote = (char)0;
            if (launchString[0] == '"') {
                quote = endChar = '"';
            } else if (launchString[0] == '\'') {
                quote = endChar = '\'';
            }

            pathEnd = 0;
            var pathBuilder = new StringBuilder();
            var prevChar = (char)0;
            var startIndex = 0;
            if (quote != 0) {
                startIndex = 1;
                prevChar = quote;
                pathEnd = 1;
            }

            for (var i = startIndex; i < launchString.Length; i++) {
                pathEnd = i + 1;

                var curChar = launchString[i];

                if (curChar == endChar && launchString[i - 1] != '\\') {
                    break;
                }

                pathBuilder.Append(curChar);
            }

            return pathBuilder.ToString();
        }

        private string TrimQuotes(string str) {
            if (string.IsNullOrWhiteSpace(str) || str.Length == 1) return str;

            var firstChar = str[0];
            var lastChar = str[^1];

            if (firstChar == '\'' || firstChar == '"') {
                if (firstChar == lastChar) {
                    return str[1..^1];
                }
            }

            return str;
        }

        private void RunProcess(string path, string args) {
            //TODO: Log this path and args
            var dir = Path.GetDirectoryName(path);

            var psi = new ProcessStartInfo(path, args);
            psi.UseShellExecute = true;
            psi.WorkingDirectory = dir;

            Process.Start(psi);
        }

        private static readonly CsvOptions _csvOptions = new CsvOptions() {
            AllowBackSlashToEscapeQuote = true,
            AllowNewLineInEnclosedFieldValues = false,
            AllowSingleQuoteToEncloseFieldValues = true,
            HeaderMode = Csv.HeaderMode.HeaderAbsent,
            ReturnEmptyForMissingColumn = true,
            Separator = ','
        };
    }
}
