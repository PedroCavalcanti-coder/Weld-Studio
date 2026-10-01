using System;
using UnityEngine;

namespace WeldStudio.Core.Data
{
    /// <summary>
    /// A part of an item the user can recolour: the collar of a shirt, the soles of a shoe, the roots, tips
    /// or streaks of a hairstyle. The colour is written to a shader colour property through a
    /// MaterialPropertyBlock, so recolouring never duplicates materials.
    /// </summary>
    /// <remarks>
    /// Clothing shaders follow the convention of a zone mask texture whose R, G, B and A channels select up
    /// to four zones per material, tinted by <c>_ZoneColor0</c>..<c>_ZoneColor3</c>. Hair shaders expose
    /// <c>_RootColor</c>, <c>_TipColor</c> and <c>_StreakColor</c>. See docs/07-personalizacao.md.
    /// </remarks>
    [Serializable]
    public sealed class ColorZone
    {
        [Tooltip("Stable identifier, unique within the item. Saved in presets: do not rename after release.")]
        [SerializeField] private string id = "primary";

        [SerializeField] private string displayName = "Primary";

        [SerializeField] private Color defaultColor = Color.white;

        [Tooltip("Shader colour property that receives this zone's colour, e.g. _ZoneColor0 or _RootColor.")]
        [SerializeField] private string shaderProperty = "_ZoneColor0";

        [Tooltip("Flattened material slot (see MaterialVariant) the colour applies to; -1 applies to all slots.")]
        [SerializeField] private int materialSlot = -1;

        public ColorZone()
        {
        }

        public ColorZone(string id, string displayName, Color defaultColor, string shaderProperty, int materialSlot = -1)
        {
            this.id = id;
            this.displayName = displayName;
            this.defaultColor = defaultColor;
            this.shaderProperty = shaderProperty;
            this.materialSlot = materialSlot;
        }

        public string Id => id;
        public string DisplayName => displayName;
        public Color DefaultColor => defaultColor;
        public string ShaderProperty => shaderProperty;
        public int MaterialSlot => materialSlot;
    }
}
