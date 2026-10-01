using System;
using System.Collections.Generic;
using System.Linq;

namespace WeldStudio.Core
{
    /// <summary>
    /// Sets (or, with a null value, resets) one modifier, or several at once with the same value (symmetric
    /// editing: left and right arm together). Consecutive commands on the same modifiers merge, so a whole
    /// slider drag is undone in one step; call <see cref="CommandHistory.Seal"/> when the drag ends.
    /// </summary>
    public sealed class SetModifierCommand : ICommand
    {
        private readonly CharacterModel model;
        private readonly string[] modifierIds;
        private float? value;
        private float?[] previousValues;

        /// <param name="value">New value, or null to reset the modifier to its default.</param>
        public SetModifierCommand(CharacterModel model, string modifierId, float? value, string label = null)
            : this(model, new[] { modifierId }, value, label)
        {
        }

        /// <param name="modifierIds">Modifiers that receive the same value, e.g. a part and its mirror.</param>
        public SetModifierCommand(CharacterModel model, IEnumerable<string> modifierIds, float? value, string label = null)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
            this.modifierIds = (modifierIds ?? throw new ArgumentNullException(nameof(modifierIds))).Distinct(StringComparer.Ordinal).ToArray();
            if (this.modifierIds.Length == 0 || this.modifierIds.Any(string.IsNullOrEmpty))
                throw new ArgumentException("At least one non-empty modifier id is required.", nameof(modifierIds));
            this.value = value;
            Label = label ?? $"Adjust {this.modifierIds[0]}";
        }

        public string Label { get; }

        public void Execute()
        {
            if (previousValues == null)
            {
                previousValues = new float?[modifierIds.Length];
                for (int i = 0; i < modifierIds.Length; i++)
                    previousValues[i] = model.TryGetModifier(modifierIds[i], out float current) ? current : (float?)null;
            }
            for (int i = 0; i < modifierIds.Length; i++) Apply(modifierIds[i], value);
        }

        public void Undo()
        {
            for (int i = 0; i < modifierIds.Length; i++) Apply(modifierIds[i], previousValues[i]);
        }

        public bool TryMergeWith(ICommand next)
        {
            var other = next as SetModifierCommand;
            if (other == null || other.model != model || !other.modifierIds.SequenceEqual(modifierIds)) return false;

            value = other.value;
            return true;
        }

        private void Apply(string modifierId, float? target)
        {
            if (target.HasValue) model.SetModifier(modifierId, target.Value);
            else model.ResetModifier(modifierId);
        }
    }
}
