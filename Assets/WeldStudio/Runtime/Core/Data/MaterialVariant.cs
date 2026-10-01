using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace WeldStudio.Core.Data
{
    /// <summary>
    /// A selectable look (colourway) of a catalog item, shown in the UI as a swatch.
    /// </summary>
    /// <remarks>
    /// Entry <c>i</c> of <see cref="Materials"/> replaces material slot <c>i</c> of the item's prefab,
    /// counting slots across all renderers in hierarchy order
    /// (<c>GetComponentsInChildren&lt;Renderer&gt;(true)</c>, then each renderer's <c>sharedMaterials</c>).
    /// An unassigned entry keeps the prefab's own material.
    /// </remarks>
    [Serializable]
    public sealed class MaterialVariant
    {
        [Tooltip("Stable identifier, unique within the item. Saved in presets: do not rename after release.")]
        [SerializeField] private string id = "default";

        [SerializeField] private string displayName = "Default";

        [Tooltip("Colour of the swatch shown in the UI.")]
        [SerializeField] private Color swatchColor = Color.white;

        [Tooltip("Material overrides by flattened slot index. Leave an entry empty to keep the prefab's material.")]
        [SerializeField] private AssetReferenceT<Material>[] materials = Array.Empty<AssetReferenceT<Material>>();

        public string Id => id;
        public string DisplayName => displayName;
        public Color SwatchColor => swatchColor;
        public IReadOnlyList<AssetReferenceT<Material>> Materials => materials;
    }
}
