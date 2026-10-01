using System;
using UnityEngine;

namespace WeldStudio.Core.Data
{
    /// <summary>
    /// A slider exposed by an item: hair length and volume, root-to-tip gradient, streak amount, sleeve
    /// length... Defined in data so new items expose new sliders without code.
    /// </summary>
    [Serializable]
    public sealed class ItemParameter
    {
        [Tooltip("Stable identifier, unique within the item. Saved in presets: do not rename after release.")]
        [SerializeField] private string id;

        [SerializeField] private string displayName;
        [SerializeField] private float minValue;
        [SerializeField] private float maxValue = 1f;
        [SerializeField] private float defaultValue;

        [SerializeField] private ParameterTarget target = ParameterTarget.ShaderFloat;

        [Tooltip("Shader float property or blendshape name, depending on Target.")]
        [SerializeField] private string propertyName;

        [Tooltip("Flattened material slot for shader properties; -1 applies to all slots. Ignored for blendshapes.")]
        [SerializeField] private int materialSlot = -1;

        public ItemParameter()
        {
        }

        public ItemParameter(string id, string displayName, float minValue, float maxValue, float defaultValue,
            ParameterTarget target, string propertyName, int materialSlot = -1)
        {
            this.id = id;
            this.displayName = displayName;
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.defaultValue = defaultValue;
            this.target = target;
            this.propertyName = propertyName;
            this.materialSlot = materialSlot;
        }

        public string Id => id;
        public string DisplayName => displayName;
        public float MinValue => minValue;
        public float MaxValue => maxValue;
        public float DefaultValue => defaultValue;
        public ParameterTarget Target => target;
        public string PropertyName => propertyName;
        public int MaterialSlot => materialSlot;

        public float Clamp(float value) => float.IsNaN(value) ? defaultValue : Math.Max(minValue, Math.Min(maxValue, value));

        /// <summary>Blendshape weight in [0, 100] for <paramref name="value"/>, mapping [min, max] linearly.</summary>
        public float ToBlendShapeWeight(float value) =>
            maxValue > minValue ? (Clamp(value) - minValue) / (maxValue - minValue) * 100f : 0f;

        internal string Validate()
        {
            if (string.IsNullOrWhiteSpace(id)) return "has an empty id";
            if (string.IsNullOrWhiteSpace(propertyName)) return $"'{id}' has no property name";
            if (!(maxValue > minValue)) return $"'{id}' needs max greater than min";
            if (defaultValue < minValue || defaultValue > maxValue) return $"'{id}' default is outside [min, max]";
            return null;
        }
    }
}
