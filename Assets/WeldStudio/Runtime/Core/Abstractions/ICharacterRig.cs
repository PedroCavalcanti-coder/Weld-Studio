using UnityEngine;

namespace WeldStudio.Core
{
    /// <summary>
    /// The base body of a character: skeleton, animator and body mesh. Everything worn is bound to it.
    /// </summary>
    public interface ICharacterRig
    {
        string RigId { get; }
        Transform Root { get; }
        Animator Animator { get; }
        SkinnedMeshRenderer BodyRenderer { get; }

        bool TryGetBone(string boneName, out Transform bone);

        /// <summary>
        /// Remaps <paramref name="renderer"/>'s bones and root bone onto this skeleton by name. Bones that do
        /// not exist in the skeleton (skirt, hair strands) are kept and re-parented under their closest
        /// mapped ancestor.
        /// </summary>
        void BindSkinnedRenderer(SkinnedMeshRenderer renderer);

        /// <summary>Makes <paramref name="renderer"/> follow the body's blendshape weights by name.</summary>
        void RegisterConformingRenderer(SkinnedMeshRenderer renderer);

        void UnregisterConformingRenderer(SkinnedMeshRenderer renderer);

        /// <summary>Sets a body blendshape (0–100, Unity's scale) and propagates it to conforming renderers.</summary>
        void SetBodyBlendShapeWeight(string blendShapeName, float weight);

        bool TryGetBodyBlendShapeWeight(string blendShapeName, out float weight);

        /// <summary>Body regions currently hidden by worn items.</summary>
        void SetHiddenRegions(BodyRegion regions);
    }
}
