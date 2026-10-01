using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace WeldStudio.Core.Data
{
    /// <summary>
    /// Base of every catalog entry that is worn on the shared skeleton (clothing, hair, accessories): how to
    /// assemble it and how the user can customise it (variants, colour zones, parameters, physics).
    /// </summary>
    public abstract class EquipableItemData : CatalogItemData, IEquipableDefinition
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

        [Header("Appearance")]
        [Tooltip("Selectable looks (texture sets). The first one is the default; empty uses the prefab's own materials.")]
        [SerializeField] private MaterialVariant[] materialVariants = Array.Empty<MaterialVariant>();

        [Tooltip("Parts the user can recolour freely (shown as colour pickers).")]
        [SerializeField] private ColorZone[] colorZones = Array.Empty<ColorZone>();

        [Tooltip("Sliders the item exposes (length, volume...).")]
        [SerializeField] private ItemParameter[] parameters = Array.Empty<ItemParameter>();

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
        public IReadOnlyList<MaterialVariant> MaterialVariants => materialVariants;
        public IReadOnlyList<ColorZone> ColorZones => colorZones;
        public IReadOnlyList<ItemParameter> Parameters => parameters;

        /// <summary>Default look, or null when the prefab's own materials are used.</summary>
        public MaterialVariant DefaultVariant => materialVariants.Length > 0 ? materialVariants[0] : null;

        public string DefaultVariantId => DefaultVariant?.Id;

        /// <summary>
        /// Profiles in order of preference. Entries can be null when the module that defines their type is
        /// not installed (e.g. Magica Cloth 2); consumers skip them and fall back to the next one.
        /// </summary>
        public IReadOnlyList<PhysicsProfileData> PhysicsProfiles => physicsProfiles;

        /// <summary>True when both items cannot be worn together: they share a slot on the same layer.</summary>
        public bool ConflictsWith(EquipableItemData other) => EquipmentRules.Conflicts(this, other);

        public bool HasVariant(string variantId) => TryGetVariant(variantId, out MaterialVariant _);

        public bool TryGetVariant(string variantId, out MaterialVariant variant) => TryFind(materialVariants, v => v.Id == variantId, out variant);
        public bool TryGetColorZone(string zoneId, out ColorZone zone) => TryFind(colorZones, z => z.Id == zoneId, out zone);
        public bool TryGetParameter(string parameterId, out ItemParameter parameter) => TryFind(parameters, p => p.Id == parameterId, out parameter);

        public override void CollectValidationErrors(ICollection<string> errors)
        {
            base.CollectValidationErrors(errors);

            if (!IsAssigned(prefab)) errors.Add("Prefab is not assigned.");
            if (string.IsNullOrWhiteSpace(rigId)) errors.Add("Rig id is empty.");
            if (slots == EquipmentSlot.None) errors.Add("Item must occupy at least one equipment slot.");

            CheckUniqueIds(materialVariants, v => v?.Id, "Material variant", errors);
            CheckUniqueIds(colorZones, z => z?.Id, "Colour zone", errors);
            CheckUniqueIds(parameters, p => p?.Id, "Parameter", errors);

            foreach (ColorZone zone in colorZones)
            {
                if (zone != null && string.IsNullOrWhiteSpace(zone.ShaderProperty))
                    errors.Add($"Colour zone '{zone.Id}' has no shader property.");
            }
            foreach (ItemParameter parameter in parameters)
            {
                string problem = parameter?.Validate();
                if (problem != null) errors.Add($"Parameter {problem}.");
            }

#if UNITY_EDITOR
            GameObject prefabAsset = IsAssigned(prefab) ? prefab.editorAsset : null;
            if (prefabAsset != null && prefabAsset.GetComponentInChildren<SkinnedMeshRenderer>(true) == null)
                errors.Add($"Prefab '{prefabAsset.name}' has no SkinnedMeshRenderer.");
#endif
        }

        /// <summary>Lets subclasses set authoring defaults (e.g. in <c>Reset</c>).</summary>
        protected void SetDefaults(EquipmentSlot defaultSlots, EquipmentLayer defaultLayer, ColorZone[] defaultZones, ItemParameter[] defaultParameters)
        {
            slots = defaultSlots;
            layer = defaultLayer;
            colorZones = defaultZones ?? Array.Empty<ColorZone>();
            parameters = defaultParameters ?? Array.Empty<ItemParameter>();
        }

        private static bool TryFind<T>(T[] items, Func<T, bool> match, out T found) where T : class
        {
            foreach (T item in items)
            {
                if (item != null && match(item))
                {
                    found = item;
                    return true;
                }
            }
            found = null;
            return false;
        }

        private static void CheckUniqueIds<T>(T[] items, Func<T, string> getId, string kind, ICollection<string> errors)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < items.Length; i++)
            {
                string id = getId(items[i]);
                if (string.IsNullOrWhiteSpace(id)) errors.Add($"{kind} #{i} has an empty id.");
                else if (!ids.Add(id)) errors.Add($"{kind} id '{id}' is used more than once.");
            }
        }
    }
}
