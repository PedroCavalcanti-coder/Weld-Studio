using System;
using System.Collections.Generic;
using UnityEngine;

namespace WeldStudio.Core.Data
{
    /// <summary>
    /// Data-defined slider that changes the body (and, through the shared skeleton and blendshape sync,
    /// everything worn). Subclasses decide what the value drives: blendshapes, bone length/thickness/offset...
    /// </summary>
    /// <remarks>
    /// Modifier ids are written by hand and are part of the preset format (e.g. <c>body.upperArm.l.length</c>):
    /// they must be unique across all modifiers and must never change after release.
    /// </remarks>
    public abstract class ModifierDefinition : ScriptableObject, ICharacterModifier
    {
        /// <summary>Addressables label of every modifier definition, used to discover them at startup.</summary>
        public const string ModifierLabel = "weld.modifiers";

        [Tooltip("Unique, human-readable id saved in presets, e.g. body.upperArm.l.length. Never rename after release.")]
        [SerializeField] private string id;

        [SerializeField] private string displayName;

        [Tooltip("UI grouping inside a body part, e.g. Shape, Size, Position.")]
        [SerializeField] private string group = "Shape";

        [SerializeField] private float minValue = -1f;
        [SerializeField] private float maxValue = 1f;
        [SerializeField] private float defaultValue;

        public string Id => id;
        public string DisplayName => displayName;
        public string Group => group;
        public float MinValue => minValue;
        public float MaxValue => maxValue;
        public float DefaultValue => defaultValue;

        public float Clamp(float value) => float.IsNaN(value) ? defaultValue : Math.Max(minValue, Math.Min(maxValue, value));

        public abstract void Apply(ICharacterRig rig, float value);

        public virtual void CollectValidationErrors(ICollection<string> errors)
        {
            if (string.IsNullOrWhiteSpace(id)) errors.Add("Id is empty.");
            if (string.IsNullOrWhiteSpace(displayName)) errors.Add("Display name is empty.");
            if (!(maxValue > minValue)) errors.Add("Max value must be greater than min value.");
            if (defaultValue < minValue || defaultValue > maxValue) errors.Add("Default value is outside [min, max].");
        }
    }
}
