using System;

namespace WeldStudio.Core
{
    /// <summary>A paint layer in a preset. Its pixels are the PNG attachment named <see cref="Image"/>.</summary>
    [Serializable]
    public sealed class PaintLayerEntry
    {
        public string Id { get; set; }
        public string TargetId { get; set; }
        public string Name { get; set; }
        public float Opacity { get; set; } = 1f;
        public PaintBlendMode BlendMode { get; set; } = PaintBlendMode.Normal;
        public bool Visible { get; set; } = true;

        /// <summary>Attachment name of the layer's PNG, e.g. <c>paint/3f2a....png</c>.</summary>
        public string Image { get; set; }

        /// <summary>Conventional attachment name for a layer's pixels.</summary>
        public static string ImageNameFor(string layerId) => $"paint/{layerId}.png";
    }
}
