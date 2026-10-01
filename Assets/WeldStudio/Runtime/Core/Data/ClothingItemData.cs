using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace WeldStudio.Core.Data
{
    /// <summary>
    /// Definition of a wearable clothing item: everything the UI needs to list it and everything the
    /// character assembler needs to put it on the shared skeleton.
    /// </summary>
    [CreateAssetMenu(fileName = "NewClothingItem", menuName = "Weld Studio/Catalog/Clothing Item", order = 0)]
    public class ClothingItemData : CatalogItemData
    {
        /// <summary>Rig the official content is authored against (MakeHuman "Game engine" skeleton).</summary>
        public const string DefaultRigId = "makehuman.game_engine";

        [Header("Assembly")]
        [Tooltip("Prefab with one or more SkinnedMeshRenderers skinned to a copy of the base skeleton. " +
                 "At runtime its bones are remapped by name onto the character's skeleton.")]
        [SerializeField] private AssetReferenceGameObject prefab;

        [Tooltip("Skeleton this item was rigged against. Items made for another rig are rejected.")]
        [SerializeField] private string rigId = DefaultRigId;

        [Tooltip("Slots this item occupies. Equipping it removes items that share a slot on the same layer.")]
        [SerializeField] private EquipmentSlot slots = EquipmentSlot.Torso;

        [SerializeField] private EquipmentLayer layer = EquipmentLayer.Base;

        [Tooltip("Copy the body's blendshape weights to this item's blendshapes with the same name, so it " +
                 "follows body shape changes (weight, muscle, proportions).")]
        [SerializeField] private bool conformToBodyShape = true;

        [Header("Body Occlusion")]
        [Tooltip("Body regions hidden while this item is worn.")]
        [SerializeField] private BodyRegion hiddenBodyRegions = BodyRegion.None;

        [Tooltip("Optional precise mask in the body's UV space (white = hidden), combined with the regions above.")]
        [SerializeField] private AssetReferenceTexture2D bodyMask;

        [Header("Appearance")]
        [Tooltip("Selectable colourways. The first one is the default; empty uses the prefab's own materials.")]
        [SerializeField] private MaterialVariant[] materialVariants = Array.Empty<MaterialVariant>();

        [Header("Physics")]
        [Tooltip("Ordered by preference: the first profile whose backend is installed is used " +
                 "(e.g. Magica Cloth 2, then Unity Cloth). Empty means the item is not simulated.")]
        [SerializeField] private PhysicsProfileData[] physicsProfiles = Array.Empty<PhysicsProfileData>();

        /// <summary>The item's prefab.</summary>
        /// <remarks>
        /// Load it through the application's asset provider (which passes this reference as a key to
        /// <c>Addressables.LoadAssetAsync</c>), never with <c>Prefab.LoadAssetAsync()</c>: this reference
        /// lives in a shared asset and can track a single handle, so two characters wearing the same item
        /// would collide.
        /// </remarks>
        public AssetReferenceGameObject Prefab => prefab;

        public string RigId => rigId;
        public EquipmentSlot Slots => slots;
        public EquipmentLayer Layer => layer;
        public bool ConformToBodyShape => conformToBodyShape;
        public BodyRegion HiddenBodyRegions => hiddenBodyRegions;

        /// <summary>Optional precise body mask; check <see cref="HasBodyMask"/> before loading it.</summary>
        public AssetReferenceTexture2D BodyMask => bodyMask;

        public bool HasBodyMask => IsAssigned(bodyMask);
        public IReadOnlyList<MaterialVariant> MaterialVariants => materialVariants;

        /// <summary>Default colourway, or null when the prefab's own materials are used.</summary>
        public MaterialVariant DefaultVariant => materialVariants.Length > 0 ? materialVariants[0] : null;

        /// <summary>
        /// Profiles in order of preference. Entries can be null when the module that defines their type is
        /// not installed (e.g. Magica Cloth 2); consumers skip them and fall back to the next one.
        /// </summary>
        public IReadOnlyList<PhysicsProfileData> PhysicsProfiles => physicsProfiles;

        /// <summary>True when both items cannot be worn together: they share a slot on the same layer.</summary>
        public bool ConflictsWith(ClothingItemData other) =>
            other != null && layer == other.layer && (slots & other.slots) != 0;

        public bool TryGetVariant(string variantId, out MaterialVariant variant)
        {
            foreach (MaterialVariant candidate in materialVariants)
            {
                if (candidate != null && candidate.Id == variantId)
                {
                    variant = candidate;
                    return true;
                }
            }
            variant = null;
            return false;
        }

        public override void CollectValidationErrors(ICollection<string> errors)
        {
            base.CollectValidationErrors(errors);

            if (!IsAssigned(prefab)) errors.Add("Prefab is not assigned.");
            if (string.IsNullOrWhiteSpace(rigId)) errors.Add("Rig id is empty.");
            if (slots == EquipmentSlot.None) errors.Add("Item must occupy at least one equipment slot.");

            var variantIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < materialVariants.Length; i++)
            {
                string variantId = materialVariants[i]?.Id;
                if (string.IsNullOrWhiteSpace(variantId))
                    errors.Add($"Material variant #{i} has an empty id.");
                else if (!variantIds.Add(variantId))
                    errors.Add($"Material variant id '{variantId}' is used more than once.");
            }

#if UNITY_EDITOR
            GameObject prefabAsset = IsAssigned(prefab) ? prefab.editorAsset : null;
            if (prefabAsset != null && prefabAsset.GetComponentInChildren<SkinnedMeshRenderer>(true) == null)
                errors.Add($"Prefab '{prefabAsset.name}' has no SkinnedMeshRenderer.");
#endif
        }
    }
}
