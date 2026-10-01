using System;

namespace WeldStudio.Core
{
    /// <summary>Adds a paint layer; undo removes it.</summary>
    /// <remarks>
    /// Layer commands only change the layer list. Brush strokes are recorded by the painting engine as their
    /// own commands (pixel snapshots), and the engine keeps the pixels of removed layers while they can still
    /// be restored by undo.
    /// </remarks>
    public sealed class AddPaintLayerCommand : ICommand
    {
        private readonly CharacterModel model;
        private readonly PaintLayer layer;
        private readonly int index;

        public AddPaintLayerCommand(CharacterModel model, PaintLayer layer, int index = -1, string label = null)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
            this.layer = layer ?? throw new ArgumentNullException(nameof(layer));
            this.index = index;
            Label = label ?? $"Add layer {layer.Name}";
        }

        public string Label { get; }
        public void Execute() => model.AddPaintLayer(layer, index);
        public void Undo() => model.RemovePaintLayer(layer.Id);
        public bool TryMergeWith(ICommand next) => false;
    }
}
