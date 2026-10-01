using System;

namespace WeldStudio.Core
{
    /// <summary>Removes a paint layer; undo puts it back at the same position.</summary>
    public sealed class RemovePaintLayerCommand : ICommand
    {
        private readonly CharacterModel model;
        private readonly string layerId;
        private PaintLayer removed;
        private int removedIndex = -1;

        public RemovePaintLayerCommand(CharacterModel model, string layerId, string label = null)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
            this.layerId = layerId;
            Label = label ?? "Delete layer";
        }

        public string Label { get; }

        public void Execute()
        {
            removed = model.TryGetPaintLayer(layerId, out PaintLayer layer) ? layer : null;
            removedIndex = model.RemovePaintLayer(layerId);
        }

        public void Undo()
        {
            if (removed != null) model.AddPaintLayer(removed, removedIndex);
        }

        public bool TryMergeWith(ICommand next) => false;
    }
}
