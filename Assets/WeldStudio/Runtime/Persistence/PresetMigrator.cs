using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using WeldStudio.Core;

namespace WeldStudio.Persistence
{
    /// <summary>
    /// Upgrades preset documents written by older versions, one schema version at a time, before they are
    /// deserialized.
    /// </summary>
    /// <remarks>
    /// To change the format: bump <see cref="CharacterPreset.CurrentSchemaVersion"/>, add a step to
    /// <see cref="DefaultSteps"/> keyed by the version it upgrades from, and add a sample file of the old
    /// version to the tests.
    /// </remarks>
    public sealed class PresetMigrator
    {
        public const string VersionProperty = "schemaVersion";

        /// <summary>Step <c>n</c> upgrades a document from version <c>n</c> to <c>n + 1</c>.</summary>
        private static readonly Dictionary<int, Action<JObject>> DefaultSteps = new Dictionary<int, Action<JObject>>();

        private readonly int currentVersion;
        private readonly IReadOnlyDictionary<int, Action<JObject>> steps;

        public PresetMigrator(int currentVersion, IReadOnlyDictionary<int, Action<JObject>> steps)
        {
            if (currentVersion < 1) throw new ArgumentOutOfRangeException(nameof(currentVersion));
            this.currentVersion = currentVersion;
            this.steps = steps ?? throw new ArgumentNullException(nameof(steps));
        }

        public static PresetMigrator Default { get; } =
            new PresetMigrator(CharacterPreset.CurrentSchemaVersion, DefaultSteps);

        /// <summary>Migrates <paramref name="document"/> in place to the current version.</summary>
        /// <returns>The version the document had before migration.</returns>
        /// <exception cref="PresetFormatException">Missing or invalid version, or a version newer than supported.</exception>
        public int Migrate(JObject document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            JToken token = document[VersionProperty];
            if (token == null || token.Type != JTokenType.Integer)
                throw new PresetFormatException($"Preset has no integer '{VersionProperty}'.");

            int originalVersion = token.Value<int>();
            if (originalVersion < 1)
                throw new PresetFormatException($"Invalid preset schema version {originalVersion}.");
            if (originalVersion > currentVersion)
                throw new PresetFormatException(
                    $"Preset uses schema version {originalVersion}, but this version of Weld Studio supports up to {currentVersion}. Update the application.");

            for (int version = originalVersion; version < currentVersion; version++)
            {
                if (!steps.TryGetValue(version, out Action<JObject> step))
                    throw new InvalidOperationException($"No preset migration from schema version {version}.");

                step(document);
                document[VersionProperty] = version + 1;
            }
            return originalVersion;
        }
    }
}
