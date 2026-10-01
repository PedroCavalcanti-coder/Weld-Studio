using System;

namespace WeldStudio.Core
{
    /// <summary>Changes the material variant of a worn item.</summary>
    public sealed class SetVariantCommand : ICommand
    {
        private readonly CharacterModel model;
        private readonly string itemId;
        private string variantId;
        private string previousVariantId;
        private bool changed;

        public SetVariantCommand(CharacterModel model, string itemId, string variantId, string label = null)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
            this.itemId = itemId;
            this.variantId = variantId;
            Label = label ?? $"Change {itemId} variant";
        }

        public string Label { get; }

        public void Execute()
        {
            previousVariantId = model.TryGetEquipped(itemId, out EquippedItem item) ? item.VariantId : null;
            changed = model.SetVariant(itemId, variantId);
        }

        public void Undo()
        {
            if (changed) model.SetVariant(itemId, previousVariantId);
        }

        /// <summary>Clicking through swatches of the same item is a single undo step.</summary>
        public bool TryMergeWith(ICommand next)
        {
            var other = next as SetVariantCommand;
            if (other == null || other.model != model || other.itemId != itemId) return false;

            variantId = other.variantId;
            changed = changed || other.changed;
            return true;
        }
    }
}
