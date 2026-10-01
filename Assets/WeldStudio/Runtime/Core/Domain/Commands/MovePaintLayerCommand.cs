using System;

namespace WeldStudio.Core
{
    /// <summary>Moves a layer up or down the stack.</summary>
    public sealed class MovePaintLayerCommand : ICommand
    {
        private readonly CharacterModel model;
        private readonly string layerId;
        private readonly int newIndex;
        private int previousIndex = -1;

        public MovePaintLayerCommand(CharacterModel model, string layerId, int newIndex, string label = null)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
            this.layerId = layerId;
            this.newIndex = newIndex;
            Label = label ?? "Reorder layer";
        }

        public string Label { get; }

        public void Execute()
        {
            previousIndex = model.IndexOfPaintLayer(layerId);
            model.MovePaintLayer(layerId, newIndex);
        }

        public void Undo()
        {
            if (previousIndex >= 0) model.MovePaintLayer(layerId, previousIndex);
        }

        public bool TryMergeWith(ICommand next) => false;
    }
}
