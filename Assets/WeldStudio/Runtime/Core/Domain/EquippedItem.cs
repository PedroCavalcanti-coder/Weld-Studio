using System;

namespace WeldStudio.Core
{
    /// <summary>An item currently worn by a character, with its selected variant and customisation. Immutable.</summary>
    public sealed class EquippedItem
    {
        public EquippedItem(IEquipableDefinition definition, string variantId, ItemAppearance appearance = null)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            VariantId = variantId;
            Appearance = appearance ?? ItemAppearance.Empty;
        }

        public IEquipableDefinition Definition { get; }

        /// <summary>Selected variant, or null when the item has no variants.</summary>
        public string VariantId { get; }

        /// <summary>Colour zone and parameter overrides chosen by the user.</summary>
        public ItemAppearance Appearance { get; }

        public string ItemId => Definition.Id;

        public EquippedItem WithVariant(string variantId) => new EquippedItem(Definition, variantId, Appearance);

        public EquippedItem WithAppearance(ItemAppearance appearance) => new EquippedItem(Definition, VariantId, appearance);

        public override string ToString() => VariantId == null ? ItemId : $"{ItemId} ({VariantId})";
    }
}
