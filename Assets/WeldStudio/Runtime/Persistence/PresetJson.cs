using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using WeldStudio.Core;

namespace WeldStudio.Persistence
{
    /// <summary>JSON format of presets: camelCase properties, modifier ids kept verbatim.</summary>
    public static class PresetJson
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            // Dictionary keys are modifier ids and must round-trip exactly, so they are not camel-cased.
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy { ProcessDictionaryKeys = false },
            },
            Formatting = Formatting.Indented,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            FloatParseHandling = FloatParseHandling.Double,
        };

        public static string Serialize(CharacterPreset preset)
        {
            if (preset == null) throw new ArgumentNullException(nameof(preset));
            return JsonConvert.SerializeObject(preset, Settings);
        }

        /// <exception cref="PresetFormatException">Invalid JSON, invalid structure or unsupported version.</exception>
        public static CharacterPreset Deserialize(string json, PresetMigrator migrator = null)
        {
            if (string.IsNullOrWhiteSpace(json)) throw new PresetFormatException("Preset file is empty.");

            try
            {
                JObject document = JObject.Parse(json);
                (migrator ?? PresetMigrator.Default).Migrate(document);
                CharacterPreset preset = document.ToObject<CharacterPreset>(JsonSerializer.Create(Settings));
                return Normalize(preset);
            }
            catch (JsonException exception)
            {
                throw new PresetFormatException("Preset file is not valid JSON or has an unexpected structure.", exception);
            }
        }

        private static CharacterPreset Normalize(CharacterPreset preset)
        {
            if (preset == null) throw new PresetFormatException("Preset file is empty.");

            preset.Metadata = preset.Metadata ?? new PresetMetadata();

            var equipment = new List<EquipmentEntry>();
            foreach (EquipmentEntry entry in preset.Equipment ?? new List<EquipmentEntry>())
            {
                if (!string.IsNullOrWhiteSpace(entry?.ItemId)) equipment.Add(entry);
            }
            preset.Equipment = equipment;

            var modifiers = new Dictionary<string, float>(StringComparer.Ordinal);
            if (preset.Modifiers != null)
            {
                foreach (KeyValuePair<string, float> pair in preset.Modifiers)
                {
                    if (!string.IsNullOrEmpty(pair.Key) && !float.IsNaN(pair.Value) && !float.IsInfinity(pair.Value))
                        modifiers[pair.Key] = pair.Value;
                }
            }
            preset.Modifiers = modifiers;
            return preset;
        }
    }
}
