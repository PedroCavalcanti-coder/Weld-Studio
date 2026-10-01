using System;
using System.Collections.Generic;

namespace WeldStudio.Core
{
    /// <summary>One worn item in a preset.</summary>
    [Serializable]
    public sealed class EquipmentEntry
    {
        public EquipmentEntry()
        {
        }

        public EquipmentEntry(string itemId, string variantId)
        {
            ItemId = itemId;
            VariantId = variantId;
        }

        public string ItemId { get; set; }
        public string VariantId { get; set; }

        /// <summary>Colour overrides by zone id, as <c>#RRGGBB</c> / <c>#RRGGBBAA</c>.</summary>
        public Dictionary<string, string> Colors { get; set; } = new Dictionary<string, string>(StringComparer.Ordinal);

        /// <summary>Parameter overrides by parameter id (hair length, volume...).</summary>
        public Dictionary<string, float> Parameters { get; set; } = new Dictionary<string, float>(StringComparer.Ordinal);

        public EquipmentEntry Clone() => new EquipmentEntry(ItemId, VariantId)
        {
            Colors = new Dictionary<string, string>(Colors ?? new Dictionary<string, string>(), StringComparer.Ordinal),
            Parameters = new Dictionary<string, float>(Parameters ?? new Dictionary<string, float>(), StringComparer.Ordinal),
        };
    }
}
