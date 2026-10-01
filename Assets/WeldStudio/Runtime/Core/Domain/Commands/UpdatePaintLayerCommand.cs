using System;

namespace WeldStudio.Core
{
    /// <summary>
    /// Changes a layer's name, opacity, blend mode or visibility. Consecutive edits of the same layer (an
    /// opacity drag) merge until <see cref="CommandHistory.Seal"/>.
    /// </summary>
    public sealed class UpdatePaintLayerCommand : ICommand
    {
        private readonly CharacterModel model;
        private PaintLayer updated;
        private PaintLayer previous;

        public UpdatePaintLayerCommand(CharacterModel model, PaintLayer updated, string label = null)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
            this.updated = updated ?? throw new ArgumentNullException(nameof(updated));
            Label = label ?? "Edit layer";
        }

        public string Label { get; }

        public void Execute()
        {
            if (previous == null && model.TryGetPaintLayer(updated.Id, out PaintLayer current)) previous = current;
            model.UpdatePaintLayer(updated);
        }

        public void Undo()
        {
            if (previous != null) model.UpdatePaintLayer(previous);
        }

        public bool TryMergeWith(ICommand next)
        {
            var other = next as UpdatePaintLayerCommand;
            if (other == null || other.model != model || other.updated.Id != updated.Id) return false;

            updated = other.updated;
            return true;
        }
    }
}
