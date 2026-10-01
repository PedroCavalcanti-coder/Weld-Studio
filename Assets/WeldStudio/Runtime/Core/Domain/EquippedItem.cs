using System;

namespace WeldStudio.Core
{
    /// <summary>An item currently worn by a character, with its selected material variant. Immutable.</summary>
    public sealed class EquippedItem
    {
        public EquippedItem(IEquipableDefinition definition, string variantId)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            VariantId = variantId;
        }

        public IEquipableDefinition Definition { get; }

        /// <summary>Selected variant, or null when the item has no variants.</summary>
        public string VariantId { get; }

        public string ItemId => Definition.Id;

        public EquippedItem WithVariant(string variantId) => new EquippedItem(Definition, variantId);

        public override string ToString() => VariantId == null ? ItemId : $"{ItemId} ({VariantId})";
    }
}
