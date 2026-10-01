using System;
using System.Collections.Generic;

namespace WeldStudio.Core
{
    /// <summary>
    /// Serializable snapshot of a character. Holds ids and values only (never Unity objects), so it can be
    /// serialized off the main thread and shared between machines.
    /// </summary>
    [Serializable]
    public sealed class CharacterPreset
    {
        /// <summary>Bump on every incompatible format change and add a migration step.</summary>
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion { get; set; } = CurrentSchemaVersion;

        /// <summary>Application version that wrote the preset, for diagnostics.</summary>
        public string CreatedWith { get; set; }

        public string RigId { get; set; }
        public PresetMetadata Metadata { get; set; } = new PresetMetadata();
        public List<EquipmentEntry> Equipment { get; set; } = new List<EquipmentEntry>();

        /// <summary>Explicitly set modifier values by modifier id. Absent ids use their default value.</summary>
        public Dictionary<string, float> Modifiers { get; set; } = new Dictionary<string, float>(StringComparer.Ordinal);
    }
}
