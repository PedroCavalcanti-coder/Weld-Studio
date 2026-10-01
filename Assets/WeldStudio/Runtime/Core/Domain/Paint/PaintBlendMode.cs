namespace WeldStudio.Core
{
    /// <summary>How a paint layer is composited over the layers below it.</summary>
    /// <remarks>Serialized by name in presets; renaming a member breaks saved files.</remarks>
    public enum PaintBlendMode
    {
        Normal = 0,
        Multiply = 1,
        Screen = 2,
        Overlay = 3,
    }
}
