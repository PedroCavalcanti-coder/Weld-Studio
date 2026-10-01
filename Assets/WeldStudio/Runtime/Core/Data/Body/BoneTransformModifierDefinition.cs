using System;
using System.Collections.Generic;
using UnityEngine;

namespace WeldStudio.Core.Data
{
    /// <summary>
    /// Slider that moves, stretches or scales bones of the skeleton: arm length, leg thickness, head size,
    /// shoulder position... Clothes and hair follow because they are skinned to the same bones.
    /// </summary>
    [CreateAssetMenu(fileName = "NewBoneModifier", menuName = "Weld Studio/Body/Bone Transform Modifier", order = 11)]
    public class BoneTransformModifierDefinition : ModifierDefinition
    {
        [Tooltip("Bones affected, by name (e.g. upperarm_l). Usually one; several for chains such as the spine.")]
        [SerializeField] private string[] bones = Array.Empty<string>();

        [SerializeField] private BoneOperation operation = BoneOperation.Length;

        [Tooltip("Bone axis for Length/Thickness, or direction for Offset.")]
        [SerializeField] private BoneAxis axis = BoneAxis.Y;

        public IReadOnlyList<string> Bones => bones;
        public BoneOperation Operation => operation;
        public BoneAxis Axis => axis;

        public override void Apply(ICharacterRig rig, float value)
        {
            BoneAdjustment adjustment = ToAdjustment(Clamp(value));
            foreach (string bone in bones)
            {
                if (!string.IsNullOrEmpty(bone)) rig.SetBoneAdjustment(bone, Id, adjustment);
            }
        }

        /// <summary>The adjustment applied to every bone for an already clamped value.</summary>
        public BoneAdjustment ToAdjustment(float value)
        {
            float factor = 1f + value;
            Vector3 along = AxisVector(axis);
            switch (operation)
            {
                case BoneOperation.Length:
                    return new BoneAdjustment(Vector3.one + along * value, Vector3.zero);
                case BoneOperation.Thickness:
                    return new BoneAdjustment(Vector3.one * factor - along * value, Vector3.zero);
                case BoneOperation.UniformScale:
                    return new BoneAdjustment(Vector3.one * factor, Vector3.zero);
                case BoneOperation.Offset:
                    return new BoneAdjustment(Vector3.one, along * value);
                default:
                    throw new InvalidOperationException($"Unknown bone operation {operation}.");
            }
        }

        public override void CollectValidationErrors(ICollection<string> errors)
        {
            base.CollectValidationErrors(errors);
            if (bones.Length == 0) errors.Add("At least one bone is required.");
            for (int i = 0; i < bones.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(bones[i])) errors.Add($"Bone #{i} has no name.");
            }
            if (operation != BoneOperation.Offset && MinValue <= -1f)
                errors.Add("Scale operations need Min Value greater than -1 (a scale of zero or less collapses the mesh).");
        }

        private static Vector3 AxisVector(BoneAxis boneAxis)
        {
            switch (boneAxis)
            {
                case BoneAxis.X: return new Vector3(1f, 0f, 0f);
                case BoneAxis.Z: return new Vector3(0f, 0f, 1f);
                default: return new Vector3(0f, 1f, 0f);
            }
        }
    }
}
