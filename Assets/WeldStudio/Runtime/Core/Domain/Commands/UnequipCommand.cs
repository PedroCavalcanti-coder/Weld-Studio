using System;

namespace WeldStudio.Core
{
    /// <summary>Removes a worn item; undo wears it again with the same variant.</summary>
    public sealed class UnequipCommand : ICommand
    {
        private readonly CharacterModel model;
        private readonly string itemId;
        private EquippedItem removed;

        public UnequipCommand(CharacterModel model, string itemId, string label = null)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
            this.itemId = itemId;
            Label = label ?? $"Remove {itemId}";
        }

        public string Label { get; }

        public void Execute()
        {
            removed = model.TryGetEquipped(itemId, out EquippedItem item) ? item : null;
            if (removed != null) model.Unequip(itemId);
        }

        public void Undo()
        {
            if (removed != null) model.Equip(removed.Definition, removed.VariantId);
        }

        public bool TryMergeWith(ICommand next) => false;
    }
}
