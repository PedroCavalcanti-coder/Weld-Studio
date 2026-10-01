namespace WeldStudio.Core
{
    /// <summary>
    /// A parametric change to the character driven by a single value: a blendshape slider, a macro that
    /// drives many blendshapes (age, weight), skin tone... The UI builds its sliders from these without
    /// knowing any concrete type.
    /// </summary>
    public interface ICharacterModifier
    {
        /// <summary>Stable id saved in presets, e.g. <c>"body.weight"</c> or <c>"face.jawOpen"</c>.</summary>
        string Id { get; }

        string DisplayName { get; }
        float MinValue { get; }
        float MaxValue { get; }
        float DefaultValue { get; }

        /// <summary>Applies <paramref name="value"/>, already clamped to [MinValue, MaxValue].</summary>
        void Apply(ICharacterRig rig, float value);
    }
}
