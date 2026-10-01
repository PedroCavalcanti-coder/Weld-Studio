using System;

namespace WeldStudio.Core
{
    /// <summary>
    /// A layer the user paints on (tattoo, make-up, dirt, logo...), drawn in the UV space of its target mesh:
    /// the body or one worn item. This is the layer's metadata only; its pixels live in the painting engine
    /// while editing and in a preset attachment once saved. Immutable.
    /// </summary>
    public sealed class PaintLayer
    {
        /// <summary><see cref="TargetId"/> of layers painted on the base body.</summary>
        public const string BodyTarget = "body";

        public PaintLayer(string id, string targetId, string name, float opacity = 1f,
            PaintBlendMode blendMode = PaintBlendMode.Normal, bool visible = true)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Layer id is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(targetId)) throw new ArgumentException("Target id is required.", nameof(targetId));
            if (float.IsNaN(opacity)) throw new ArgumentOutOfRangeException(nameof(opacity));

            Id = id;
            TargetId = targetId;
            Name = name ?? string.Empty;
            Opacity = Math.Max(0f, Math.Min(1f, opacity));
            BlendMode = blendMode;
            Visible = visible;
        }

        public string Id { get; }

        /// <summary><see cref="BodyTarget"/> or the id of the worn item the layer is painted on.</summary>
        public string TargetId { get; }

        public string Name { get; }
        public float Opacity { get; }
        public PaintBlendMode BlendMode { get; }
        public bool Visible { get; }

        public static string NewId() => Guid.NewGuid().ToString("N");

        public PaintLayer WithName(string name) => new PaintLayer(Id, TargetId, name, Opacity, BlendMode, Visible);
        public PaintLayer WithOpacity(float opacity) => new PaintLayer(Id, TargetId, Name, opacity, BlendMode, Visible);
        public PaintLayer WithBlendMode(PaintBlendMode mode) => new PaintLayer(Id, TargetId, Name, Opacity, mode, Visible);
        public PaintLayer WithVisible(bool visible) => new PaintLayer(Id, TargetId, Name, Opacity, BlendMode, visible);

        public bool HasSameSettings(PaintLayer other) =>
            other != null && other.Id == Id && other.TargetId == TargetId && other.Name == Name &&
            other.Opacity == Opacity && other.BlendMode == BlendMode && other.Visible == Visible;
    }
}
