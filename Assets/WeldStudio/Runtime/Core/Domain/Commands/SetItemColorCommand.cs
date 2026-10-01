using System;

namespace WeldStudio.Core
{
    /// <summary>
    /// Sets (or, with null, resets) the colour of one colour zone of a worn item: a part of a garment, hair
    /// roots, tips or streaks. Dragging in the colour picker merges into one undo step until
    /// <see cref="CommandHistory.Seal"/>.
    /// </summary>
    public sealed class SetItemColorCommand : ICommand
    {
        private readonly CharacterModel model;
        private readonly string itemId;
        private readonly string zoneId;
        private ColorRgba? color;
        private ColorRgba? previousColor;
        private bool captured;

        public SetItemColorCommand(CharacterModel model, string itemId, string zoneId, ColorRgba? color, string label = null)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
            if (string.IsNullOrEmpty(zoneId)) throw new ArgumentException("Zone id is required.", nameof(zoneId));
            this.itemId = itemId;
            this.zoneId = zoneId;
            this.color = color;
            Label = label ?? $"Colour {zoneId}";
        }

        public string Label { get; }

        public void Execute()
        {
            if (!captured)
            {
                previousColor = model.TryGetEquipped(itemId, out EquippedItem item) && item.Appearance.TryGetColor(zoneId, out ColorRgba current)
                    ? current
                    : (ColorRgba?)null;
                captured = true;
            }
            model.SetItemColor(itemId, zoneId, color);
        }

        public void Undo() => model.SetItemColor(itemId, zoneId, previousColor);

        public bool TryMergeWith(ICommand next)
        {
            var other = next as SetItemColorCommand;
            if (other == null || other.model != model || other.itemId != itemId || other.zoneId != zoneId) return false;

            color = other.color;
            return true;
        }
    }
}
