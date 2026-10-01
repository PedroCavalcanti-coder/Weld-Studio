using System;

namespace WeldStudio.Core
{
    /// <summary>
    /// Sets (or, with null, resets) a parameter of a worn item, such as hair length or volume. Slider drags
    /// merge into one undo step until <see cref="CommandHistory.Seal"/>.
    /// </summary>
    public sealed class SetItemParameterCommand : ICommand
    {
        private readonly CharacterModel model;
        private readonly string itemId;
        private readonly string parameterId;
        private float? value;
        private float? previousValue;
        private bool captured;

        public SetItemParameterCommand(CharacterModel model, string itemId, string parameterId, float? value, string label = null)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
            if (string.IsNullOrEmpty(parameterId)) throw new ArgumentException("Parameter id is required.", nameof(parameterId));
            this.itemId = itemId;
            this.parameterId = parameterId;
            this.value = value;
            Label = label ?? $"Adjust {parameterId}";
        }

        public string Label { get; }

        public void Execute()
        {
            if (!captured)
            {
                previousValue = model.TryGetEquipped(itemId, out EquippedItem item) && item.Appearance.TryGetParameter(parameterId, out float current)
                    ? current
                    : (float?)null;
                captured = true;
            }
            model.SetItemParameter(itemId, parameterId, value);
        }

        public void Undo() => model.SetItemParameter(itemId, parameterId, previousValue);

        public bool TryMergeWith(ICommand next)
        {
            var other = next as SetItemParameterCommand;
            if (other == null || other.model != model || other.itemId != itemId || other.parameterId != parameterId) return false;

            value = other.value;
            return true;
        }
    }
}
