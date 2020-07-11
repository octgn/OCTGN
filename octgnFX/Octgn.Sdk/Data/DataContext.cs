using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Octgn.Sdk.Data
{
    public class DataContext : DbContext
    {
        public const long EmptyDbVersion = 0;
        public const long LatestDbVersion = 1;

        public static readonly IReadOnlyDictionary<long, string> MigrationScripts = new Dictionary<long, string> {
            { 1, "001_start.sql" }
        };

        public DbSet<PackageRecord> Packages { get; set; }
        public DbSet<PluginRecord> Plugins { get; set; }

        public Uri Path { get; }

        public DataContext() {
            Path = new Uri("temp.db", UriKind.Relative);
        }

        public DataContext(string path) {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

            Path = new Uri(path);
        }

        public IEnumerable<PluginRecord> Games() {
            var packageGroups = Packages
                .ToArray()
                .GroupBy(x => x.Id)
                .ToArray()
            ;

            foreach (var packageGroup in packageGroups) {
                var package = packageGroup.OrderByDescending(x => x.Version).First();

                foreach (var game in GamePlugins(package)) {
                    yield return game;
                }
            }
        }

        public IEnumerable<PluginRecord> GamePlugins(PackageRecord package) {
            var query = PackagePlugins(package)
                .Where(plugin =>
                    plugin.Type == "octgn.plugin.game"
                );
            ;

            foreach (var plugin in query) {
                yield return plugin;
            }
        }

        public IEnumerable<PluginRecord> PackagePlugins(PackageRecord package) {
            if (package == null) throw new ArgumentNullException(nameof(package));

            var query = Plugins
                .Where(plugin =>
                    plugin.PackageId == package.Id
                    && plugin.PackageVersion == package.Version
                );
            ;

            foreach (var plugin in query) {
                yield return plugin;
            }
        }

        #region Upgrading

        public void Upgrade(IProgress<Progress> progress, CancellationToken cancellationToken) {
            using (var transaction = Database.BeginTransaction(System.Data.IsolationLevel.Serializable)) {
                var currentDbVersion = GetDbVersion();

                if (currentDbVersion == DataContext.EmptyDbVersion) {
                    RunCreateScript(progress, cancellationToken);
                } else if (currentDbVersion < DataContext.LatestDbVersion) {
                    RunUpdateScripts(currentDbVersion, progress, cancellationToken);
                } else if (currentDbVersion > DataContext.LatestDbVersion) {
                    RunDowngradeScripts(currentDbVersion, progress, cancellationToken);
                } else {
                    // all is well go home
                    return;
                }

                transaction.Commit();
            }
        }

        private void RunCreateScript(IProgress<Progress> progress, CancellationToken cancellationToken) {
            var script = LoadScript("create.sql");

            ExecuteScript(script);
        }

        private void RunUpdateScripts(long currentDbVersion, IProgress<Progress> progress, CancellationToken cancellationToken) {
            //TODO: missing progress
            //TODO: missing cancellation
            for (; currentDbVersion <= LatestDbVersion; currentDbVersion++) {
                var script = LoadUpMigrationScript(currentDbVersion, out var scriptName);

                ExecuteScript(script);

                UpdateDbVersion(currentDbVersion);
            }
        }

        private void RunDowngradeScripts(long currentDbVersion, IProgress<Progress> progress, CancellationToken cancellationToken) {
            //TODO: missing progress
            //TODO: missing cancellation
            for (; currentDbVersion > 0; currentDbVersion--) {
                var script = LoadDownMigrationScript(currentDbVersion, out var scriptName);

                ExecuteScript(script);

                UpdateDbVersion(currentDbVersion);
            }
        }

        private long GetDbVersion() {
            using (var command = Database.GetDbConnection().CreateCommand()) {
                command.CommandText = "PRAGMA user_version";
                using (var reader = command.ExecuteReader()) {
                    if (!reader.Read()) throw new InvalidOperationException("Unable to read db version");

                    return reader.GetInt64(0);
                }
            }
        }

        private void UpdateDbVersion(long scriptVersion) {
            if (Database.ExecuteSqlRaw("PRAGMA user_version = {0}", scriptVersion) != 1)
                throw new InvalidOperationException($"user_version not updated.");
        }

        protected virtual string LoadScript(string name) {
            var assembly = typeof(DataContext).Assembly;

            var names = assembly.GetManifestResourceNames();

            //TODO: What exception is thrown if the resource doesn't exist?
            // Should possibly be caught and rethrow a more specific exception (ScriptNotFoundException or something)
            using (var stream = assembly.GetManifestResourceStream($"Octgn.Sdk.Data.Scripts.{name}"))
            using (var reader = new StreamReader(stream)) {
                return reader.ReadToEnd();
            }
        }

        private string LoadFullMigrationScript(long dbVersion, out string scriptName) {
            if (!MigrationScripts.TryGetValue(dbVersion, out var migrationScriptName))
                throw new ScriptNotFoundException($"Could not find a migration script for {dbVersion}");

            var script = LoadScript(migrationScriptName);

            // Set this after we load, so that we don't set the variable if LoadScript fails
            scriptName = migrationScriptName;

            return script;

        }

        private (string upScript, string downScript) SplitMigrationScript(string script, string scriptName) {
            // I check both only to make sure the file is in the proper format
            // Getting the script this way is a bit more complicated.
            var upIndex = script.IndexOf("-- up");

            var downIndex = script.IndexOf("-- down");

            if (downIndex < 0) {
                throw new InvalidMigrationScriptException($"Script {scriptName} is missing down migration");
            }

            if (upIndex < 0) {
                throw new InvalidMigrationScriptException($"Script {scriptName} is missing up migration");
            }

            var upScript = script.Substring(upIndex, downIndex);
            var downScript = script.Substring(downIndex, script.Length - downIndex);

            return (upScript, downScript);
        }

        private string LoadUpMigrationScript(long dbVersion, out string scriptName) {
            var fullScript = LoadFullMigrationScript(dbVersion, out scriptName);

            var split = SplitMigrationScript(fullScript, scriptName);

            var upScript = split.upScript;

            return upScript;
        }

        private string LoadDownMigrationScript(long dbVersion, out string scriptName) {
            var fullScript = LoadFullMigrationScript(dbVersion, out scriptName);

            var split = SplitMigrationScript(fullScript, scriptName);

            var downScript = split.downScript;

            return downScript;
        }

        protected virtual void ExecuteScript(string script) {
            this.Database.ExecuteSqlRaw(script);
        }

        #endregion Upgrading

        protected override void OnConfiguring(DbContextOptionsBuilder options) {
            options.UseSqlite($"Data Source={Path}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<PackageRecord>(config => {
                config.ToTable("octgn_packages");

                config.HasKey(record
                    => new {
                        record.Id,
                        record.Version
                    });

                config.HasIndex(record => record.Name);
                config.HasIndex(record => record.Description);
            });

            modelBuilder.Entity<PluginRecord>(config => {
                config.ToTable("octgn_plugins");

                config.HasKey(record
                    => new {
                        record.Id,
                        record.PackageId,
                        record.PackageVersion
                    });

                config.HasIndex(record => record.Name);
                config.HasIndex(record => record.Description);
            });
        }
    }
}
