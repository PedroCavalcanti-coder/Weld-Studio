using System.Collections.Generic;
using UnityEngine;

namespace WeldStudio.Core.Data
{
    /// <summary>
    /// Slider driving one or two body blendshapes: positive values drive <see cref="PositiveShape"/> (nose
    /// wider), negative values drive <see cref="NegativeShape"/> (nose narrower). Worn items with
    /// "conform to body shape" follow automatically through blendshapes of the same name.
    /// </summary>
    [CreateAssetMenu(fileName = "NewBlendShapeModifier", menuName = "Weld Studio/Body/Blend Shape Modifier", order = 10)]
    public class BlendShapeModifierDefinition : ModifierDefinition
    {
        [Tooltip("Blendshape driven by values above zero (weight 100 at Max Value).")]
        [SerializeField] private string positiveShape;

        [Tooltip("Optional blendshape driven by values below zero (weight 100 at Min Value).")]
        [SerializeField] private string negativeShape;

        public string PositiveShape => positiveShape;
        public string NegativeShape => negativeShape;

        public override void Apply(ICharacterRig rig, float value)
        {
            Weights(Clamp(value), out float positive, out float negative);
            if (!string.IsNullOrEmpty(positiveShape)) rig.SetBodyBlendShapeWeight(positiveShape, positive);
            if (!string.IsNullOrEmpty(negativeShape)) rig.SetBodyBlendShapeWeight(negativeShape, negative);
        }

        /// <summary>Blendshape weights in [0, 100] for an already clamped value.</summary>
        public void Weights(float value, out float positive, out float negative)
        {
            positive = value > 0f && MaxValue > 0f ? value / MaxValue * 100f : 0f;
            negative = value < 0f && MinValue < 0f ? value / MinValue * 100f : 0f;
        }

        public override void CollectValidationErrors(ICollection<string> errors)
        {
            base.CollectValidationErrors(errors);
            if (string.IsNullOrWhiteSpace(positiveShape) && string.IsNullOrWhiteSpace(negativeShape))
                errors.Add("At least one blendshape is required.");
            if (!string.IsNullOrWhiteSpace(negativeShape) && MinValue >= 0f)
                errors.Add("A negative shape needs a negative Min Value.");
        }
    }
}
