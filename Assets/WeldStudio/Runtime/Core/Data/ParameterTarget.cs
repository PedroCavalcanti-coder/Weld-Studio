namespace WeldStudio.Core.Data
{
    /// <summary>What an <see cref="ItemParameter"/> drives on the item.</summary>
    public enum ParameterTarget
    {
        /// <summary>A float shader property, set through a MaterialPropertyBlock (e.g. hair <c>_Length</c>).</summary>
        ShaderFloat = 0,

        /// <summary>
        /// A blendshape of the item's renderers; the parameter value is mapped linearly from [min, max] to
        /// weight [0, 100] (e.g. hair volume, sleeve length).
        /// </summary>
        BlendShape = 1,
    }
}
