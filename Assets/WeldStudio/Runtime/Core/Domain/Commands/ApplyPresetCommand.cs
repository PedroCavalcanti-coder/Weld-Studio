using System;

namespace WeldStudio.Core
{
    /// <summary>Replaces the whole character with a preset; undo restores the previous state.</summary>
    public sealed class ApplyPresetCommand : ICommand
    {
        private readonly CharacterModel model;
        private readonly CharacterPreset preset;
        private readonly Func<string, IEquipableDefinition> resolve;
        private CharacterPreset previous;

        public ApplyPresetCommand(CharacterModel model, CharacterPreset preset, Func<string, IEquipableDefinition> resolve, string label = null)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
            this.preset = preset ?? throw new ArgumentNullException(nameof(preset));
            this.resolve = resolve ?? throw new ArgumentNullException(nameof(resolve));
            Label = label ?? "Load preset";
        }

        public string Label { get; }

        /// <summary>Result of the last <see cref="Execute"/>, for showing missing content to the user.</summary>
        public PresetApplyReport Report { get; private set; }

        public void Execute()
        {
            previous = model.ToPreset();
            Report = model.ApplyPreset(preset, resolve);
        }

        public void Undo()
        {
            if (previous != null) model.ApplyPreset(previous, resolve);
        }

        public bool TryMergeWith(ICommand next) => false;
    }
}
