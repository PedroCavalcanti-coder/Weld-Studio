using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace WeldStudio.Core.Data
{
    /// <summary>
    /// A sample animation offered in the preview panel to check the character in motion. Clips are Humanoid,
    /// so any clip retargets to the shared skeleton.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAnimation", menuName = "Weld Studio/Catalog/Animation", order = 2)]
    public class AnimationClipData : CatalogItemData
    {
        [Header("Animation")]
        [SerializeField] private AssetReferenceT<AnimationClip> clip;
        [SerializeField] private AnimationCategory category = AnimationCategory.Idle;
        [SerializeField] private bool loop = true;

        public AssetReferenceT<AnimationClip> Clip => clip;
        public AnimationCategory Category => category;
        public bool Loop => loop;

        public override void CollectValidationErrors(ICollection<string> errors)
        {
            base.CollectValidationErrors(errors);
            if (!IsAssigned(clip)) errors.Add("Animation clip is not assigned.");
        }
    }
}
