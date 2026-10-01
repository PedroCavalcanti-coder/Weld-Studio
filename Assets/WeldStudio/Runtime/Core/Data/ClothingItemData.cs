using UnityEngine;
using UnityEngine.AddressableAssets;

namespace WeldStudio.Core.Data
{
    /// <summary>
    /// Definition of a wearable clothing item: everything the UI needs to list and customise it and
    /// everything the character assembler needs to put it on the shared skeleton.
    /// </summary>
    [CreateAssetMenu(fileName = "NewClothingItem", menuName = "Weld Studio/Catalog/Clothing Item", order = 0)]
    public class ClothingItemData : EquipableItemData
    {
        [Header("Body Occlusion")]
        [Tooltip("Body regions hidden while this item is worn.")]
        [SerializeField] private BodyRegion hiddenBodyRegions = BodyRegion.None;

        [Tooltip("Optional precise mask in the body's UV space (white = hidden), combined with the regions above.")]
        [SerializeField] private AssetReferenceTexture2D bodyMask;

        public BodyRegion HiddenBodyRegions => hiddenBodyRegions;

        /// <summary>Optional precise body mask; check <see cref="HasBodyMask"/> before loading it.</summary>
        public AssetReferenceTexture2D BodyMask => bodyMask;

        public bool HasBodyMask => IsAssigned(bodyMask);

        /// <summary>New assets start with one recolourable zone wired to the clothing shader's first mask channel.</summary>
        protected virtual void Reset() => SetDefaults(EquipmentSlot.Torso, EquipmentLayer.Base,
            new[] { new ColorZone("primary", "Primary", Color.white, "_ZoneColor0") }, null);
    }
}
