using UnityEngine;

namespace WeldStudio.Core
{
    /// <summary>
    /// A change to a bone's rest pose contributed by one modifier: a local scale multiplier and a local
    /// position offset (metres). Several modifiers may adjust the same bone; <see cref="BoneAdjustmentStack"/>
    /// combines them.
    /// </summary>
    public readonly struct BoneAdjustment
    {
        public BoneAdjustment(Vector3 scale, Vector3 offset)
        {
            Scale = scale;
            Offset = offset;
        }

        public static BoneAdjustment Identity => new BoneAdjustment(Vector3.one, Vector3.zero);

        public Vector3 Scale { get; }
        public Vector3 Offset { get; }

        /// <summary>Scales multiply, offsets add.</summary>
        public BoneAdjustment Combine(BoneAdjustment other) =>
            new BoneAdjustment(Vector3.Scale(Scale, other.Scale), Offset + other.Offset);
    }
}
