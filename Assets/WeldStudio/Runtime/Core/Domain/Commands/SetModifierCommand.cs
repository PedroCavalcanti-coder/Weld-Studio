using System;

namespace WeldStudio.Core
{
    /// <summary>
    /// Sets (or, with a null value, resets) a modifier. Consecutive commands on the same modifier merge, so a
    /// whole slider drag is undone in one step; call <see cref="CommandHistory.Seal"/> when the drag ends.
    /// </summary>
    public sealed class SetModifierCommand : ICommand
    {
        private readonly CharacterModel model;
        private readonly string modifierId;
        private float? value;
        private float? previousValue;
        private bool captured;

        /// <param name="value">New value, or null to reset the modifier to its default.</param>
        public SetModifierCommand(CharacterModel model, string modifierId, float? value, string label = null)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
            if (string.IsNullOrEmpty(modifierId)) throw new ArgumentException("Modifier id is required.", nameof(modifierId));
            this.modifierId = modifierId;
            this.value = value;
            Label = label ?? $"Adjust {modifierId}";
        }

        public string Label { get; }

        public void Execute()
        {
            if (!captured)
            {
                previousValue = model.TryGetModifier(modifierId, out float current) ? current : (float?)null;
                captured = true;
            }
            Apply(value);
        }

        public void Undo() => Apply(previousValue);

        public bool TryMergeWith(ICommand next)
        {
            var other = next as SetModifierCommand;
            if (other == null || other.model != model || other.modifierId != modifierId) return false;

            value = other.value;
            return true;
        }

        private void Apply(float? target)
        {
            if (target.HasValue) model.SetModifier(modifierId, target.Value);
            else model.ResetModifier(modifierId);
        }
    }
}
