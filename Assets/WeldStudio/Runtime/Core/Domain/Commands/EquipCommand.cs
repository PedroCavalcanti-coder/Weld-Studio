using System;

namespace WeldStudio.Core
{
    /// <summary>Wears an item; undo removes it and puts back whatever it replaced.</summary>
    public sealed class EquipCommand : ICommand
    {
        private readonly CharacterModel model;
        private readonly IEquipableDefinition definition;
        private readonly string variantId;
        private EquipmentChange change = EquipmentChange.Empty;

        public EquipCommand(CharacterModel model, IEquipableDefinition definition, string variantId = null, string label = null)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
            this.definition = definition ?? throw new ArgumentNullException(nameof(definition));
            this.variantId = variantId;
            Label = label ?? $"Equip {definition.Id}";
        }

        public string Label { get; }

        public void Execute() => change = model.Equip(definition, variantId);

        public void Undo()
        {
            if (change.IsEmpty) return;

            model.Unequip(definition.Id);
            foreach (EquippedItem item in change.Removed) model.Equip(item.Definition, item.VariantId);
        }

        public bool TryMergeWith(ICommand next) => false;
    }
}
