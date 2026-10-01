using System;

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
    }
}
