using UnityEngine;

namespace WeldStudio.Core.Data
{
    /// <summary>
    /// Definition of a hairstyle made of hair cards. Besides the common equipable data, new assets come with
    /// the standard hair options wired to the Weld hair shader: root, tip and streak colours plus length,
    /// volume, gradient and streak sliders.
    /// </summary>
    [CreateAssetMenu(fileName = "NewHair", menuName = "Weld Studio/Catalog/Hair", order = 1)]
    public class HairItemData : EquipableItemData
    {
        public const string RootColor = "root";
        public const string TipColor = "tip";
        public const string StreakColor = "streak";

        public const string Length = "length";
        public const string Volume = "volume";
        public const string GradientStart = "gradientStart";
        public const string GradientSoftness = "gradientSoftness";
        public const string StreakAmount = "streakAmount";

        /// <summary>Standard colour zones of the Weld hair shader.</summary>
        public static ColorZone[] StandardColorZones() => new[]
        {
            new ColorZone(RootColor, "Roots", new Color(0.18f, 0.11f, 0.07f), "_RootColor"),
            new ColorZone(TipColor, "Tips", new Color(0.30f, 0.19f, 0.11f), "_TipColor"),
            new ColorZone(StreakColor, "Streaks", new Color(0.75f, 0.58f, 0.36f), "_StreakColor"),
        };

        /// <summary>
        /// Standard sliders. Length is a shader trim along the strands (shortens only; longer styles are other
        /// items). Volume drives the optional <c>hair_volume</c> blendshape authored on the hair cards.
        /// </summary>
        public static ItemParameter[] StandardParameters() => new[]
        {
            new ItemParameter(Length, "Length", 0.2f, 1f, 1f, ParameterTarget.ShaderFloat, "_Length"),
            new ItemParameter(Volume, "Volume", 0f, 1f, 0f, ParameterTarget.BlendShape, "hair_volume"),
            new ItemParameter(GradientStart, "Root length", 0f, 1f, 0.15f, ParameterTarget.ShaderFloat, "_GradientStart"),
            new ItemParameter(GradientSoftness, "Root blend", 0f, 1f, 0.3f, ParameterTarget.ShaderFloat, "_GradientSoftness"),
            new ItemParameter(StreakAmount, "Streaks", 0f, 1f, 0f, ParameterTarget.ShaderFloat, "_StreakAmount"),
        };

        protected virtual void Reset() =>
            SetDefaults(EquipmentSlot.Hair, EquipmentLayer.Base, StandardColorZones(), StandardParameters());
    }
}
