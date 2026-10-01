using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace WeldStudio.Core.Data
{
    /// <summary>
    /// Base of every entry the user can browse in the catalog (clothing, hair, accessories...).
    /// </summary>
    /// <remarks>
    /// All catalog entries are loaded at startup through the Addressables label <see cref="CatalogLabel"/>,
    /// so they must stay lightweight: meshes, textures and icons are only referenced through
    /// <see cref="AssetReference"/>s and loaded on demand. Entries are immutable definitions; what a
    /// character currently wears lives in the character model, never here.
    /// </remarks>
    public abstract class CatalogItemData : ScriptableObject
    {
        /// <summary>Addressables label shared by all catalog entries, used to discover them at startup.</summary>
        public const string CatalogLabel = "weld.catalog";

        [Header("Identity")]
        [Tooltip("Stable unique ID saved in presets. Assigned automatically from the asset GUID; do not edit.")]
        [SerializeField] private string id;

        [Header("Presentation")]
        [SerializeField] private string displayName;

        [SerializeField, TextArea(2, 5)] private string description;

        [Tooltip("Thumbnail shown in the catalog. Loaded on demand, only while visible.")]
        [SerializeField] private AssetReferenceSprite icon;

        [Tooltip("Free-form search and filter keywords, e.g. \"casual\", \"sci-fi\".")]
        [SerializeField] private string[] tags = Array.Empty<string>();

        [Tooltip("Lower values are listed first.")]
        [SerializeField] private int sortOrder;

        [Header("Attribution")]
        [SerializeField] private string author;

        [Tooltip("SPDX license identifier of this asset, e.g. CC0-1.0 or CC-BY-4.0.")]
        [SerializeField] private string license = "CC0-1.0";

        [Tooltip("Where the original asset comes from (required by most CC-BY licenses).")]
        [SerializeField] private string sourceUrl;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;

        /// <summary>Catalog thumbnail. Load it through the asset provider, like any other reference.</summary>
        public AssetReferenceSprite Icon => icon;

        public IReadOnlyList<string> Tags => tags;
        public int SortOrder => sortOrder;
        public string Author => author;
        public string License => license;
        public string SourceUrl => sourceUrl;

        public bool HasTag(string tag)
        {
            foreach (string candidate in tags)
            {
                if (string.Equals(candidate, tag, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Appends a human-readable message for every authoring error. Used by the editor validator and
        /// by the catalog at load time, which skips broken entries (e.g. from a community pack) instead of
        /// failing.
        /// </summary>
        public virtual void CollectValidationErrors(ICollection<string> errors)
        {
            if (string.IsNullOrWhiteSpace(id)) errors.Add("Id is empty.");
            if (string.IsNullOrWhiteSpace(displayName)) errors.Add("Display name is empty.");
            if (string.IsNullOrWhiteSpace(license)) errors.Add("License is empty.");
        }

        /// <summary>True when the reference points to an asset. Does not check that it is loadable.</summary>
        protected static bool IsAssigned(AssetReference reference) =>
            reference != null && reference.RuntimeKeyIsValid();

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (!UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(this, out string guid, out long _))
                return; // Not saved as an asset yet.

            // The id mirrors the asset GUID, so it is unique by construction. Two cases need fixing: an
            // empty id (new asset) and a duplicated asset (Ctrl+D copies the serialized id of an asset that
            // still exists). An id that resolves to no asset is kept on purpose: it is a legacy id (e.g. a
            // regenerated .meta) and changing it would break presets that already reference it.
            bool isUnset = string.IsNullOrEmpty(id);
            bool isCopy = id != guid && !string.IsNullOrEmpty(UnityEditor.AssetDatabase.GUIDToAssetPath(id));
            if (isUnset || isCopy)
            {
                id = guid;
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}
