using System;
using System.Collections.Generic;
using UnityEngine;

namespace WeldStudio.Core.Data
{
    /// <summary>
    /// A part of the body the user can select (click it on the model or pick it in a list) to edit it: head,
    /// nose, left upper arm, torso... Groups the sliders of that part and tells the UI how to highlight and
    /// frame it.
    /// </summary>
    [CreateAssetMenu(fileName = "NewBodyPart", menuName = "Weld Studio/Body/Body Part", order = 0)]
    public class BodyPartData : ScriptableObject
    {
        /// <summary>Addressables label of every body part, used to build the selection list.</summary>
        public const string BodyPartLabel = "weld.bodyparts";

        [SerializeField] private string id;
        [SerializeField] private string displayName;

        [Tooltip("Regions of the body mesh that belong to this part: clicking them selects it, and they are highlighted while selected.")]
        [SerializeField] private BodyRegion regions;

        [Tooltip("Bone the camera frames when the part is selected.")]
        [SerializeField] private string focusBone;

        [Tooltip("Sliders of this part, shown grouped by ModifierDefinition.Group.")]
        [SerializeField] private ModifierDefinition[] modifiers = Array.Empty<ModifierDefinition>();

        [Tooltip("The opposite-side part (left ↔ right). With symmetry on, edits apply to both.")]
        [SerializeField] private BodyPartData mirror;

        [Tooltip("Lower values are listed first.")]
        [SerializeField] private int sortOrder;

        public string Id => id;
        public string DisplayName => displayName;
        public BodyRegion Regions => regions;
        public string FocusBone => focusBone;
        public IReadOnlyList<ModifierDefinition> Modifiers => modifiers;
        public BodyPartData Mirror => mirror;
        public int SortOrder => sortOrder;

        /// <summary>
        /// Modifier of the mirror part matching <paramref name="modifier"/> by position in the list, or null.
        /// Mirrored parts must list their modifiers in the same order.
        /// </summary>
        public ModifierDefinition FindMirrorOf(ModifierDefinition modifier)
        {
            if (mirror == null || modifier == null) return null;
            int index = Array.IndexOf(modifiers, modifier);
            return index >= 0 && index < mirror.modifiers.Length ? mirror.modifiers[index] : null;
        }

        public void CollectValidationErrors(ICollection<string> errors)
        {
            if (string.IsNullOrWhiteSpace(id)) errors.Add("Id is empty.");
            if (string.IsNullOrWhiteSpace(displayName)) errors.Add("Display name is empty.");
            if (regions == BodyRegion.None) errors.Add("At least one body region is required.");
            if (mirror != null && mirror.modifiers.Length != modifiers.Length)
                errors.Add($"Mirror part '{mirror.id}' must list the same number of modifiers, in the same order.");
            for (int i = 0; i < modifiers.Length; i++)
            {
                if (modifiers[i] == null) errors.Add($"Modifier #{i} is missing.");
            }
        }
    }
}
