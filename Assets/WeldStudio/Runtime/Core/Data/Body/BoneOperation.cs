namespace WeldStudio.Core.Data
{
    /// <summary>What a <see cref="BoneTransformModifierDefinition"/> does to its bones.</summary>
    public enum BoneOperation
    {
        /// <summary>Stretch or shrink along the bone axis: scale = 1 + value on that axis.</summary>
        Length = 0,

        /// <summary>Thicker or thinner: scale = 1 + value on the two axes perpendicular to the bone axis.</summary>
        Thickness = 1,

        /// <summary>Bigger or smaller in every direction: scale = 1 + value on all axes.</summary>
        UniformScale = 2,

        /// <summary>Move along the chosen axis: local offset = value metres.</summary>
        Offset = 3,
    }
}
