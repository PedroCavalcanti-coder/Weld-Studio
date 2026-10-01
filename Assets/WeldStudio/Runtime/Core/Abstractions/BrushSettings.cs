using System;

namespace WeldStudio.Core
{
    /// <summary>Brush used by an <see cref="IPaintStroke"/>. Immutable.</summary>
    public readonly struct BrushSettings
    {
        public BrushSettings(BrushTool tool, ColorRgba color, float radius, float hardness = 0.8f, float opacity = 1f)
        {
            if (!(radius > 0f)) throw new ArgumentOutOfRangeException(nameof(radius), radius, "Radius must be positive.");
            Tool = tool;
            Color = color;
            Radius = radius;
            Hardness = Clamp01(hardness);
            Opacity = Clamp01(opacity);
        }

        public BrushTool Tool { get; }
        public ColorRgba Color { get; }

        /// <summary>Radius in metres on the surface of the model, so the brush feels the same at any zoom.</summary>
        public float Radius { get; }

        /// <summary>0 = soft falloff, 1 = hard edge.</summary>
        public float Hardness { get; }

        public float Opacity { get; }

        private static float Clamp01(float value) => float.IsNaN(value) ? 1f : Math.Max(0f, Math.Min(1f, value));
    }
}
