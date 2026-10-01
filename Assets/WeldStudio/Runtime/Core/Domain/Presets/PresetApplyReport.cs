using System.Collections.Generic;

namespace WeldStudio.Core
{
    /// <summary>
    /// What could not be applied from a preset. Nothing here is fatal: unresolved items are kept in the model
    /// and written back on the next save, so a missing content pack never destroys a preset.
    /// </summary>
    public sealed class PresetApplyReport
    {
        /// <summary>Preset was made for another rig; its items are kept but not equipped.</summary>
        public bool RigMismatch { get; internal set; }

        /// <summary>Item ids not found in the catalog (e.g. a content pack that is not installed).</summary>
        public List<string> MissingItemIds { get; } = new List<string>();

        /// <summary>Items found in the catalog but rigged for another skeleton.</summary>
        public List<string> IncompatibleItemIds { get; } = new List<string>();

        /// <summary>Unknown variants; the item's default variant was used instead.</summary>
        public List<EquipmentEntry> MissingVariants { get; } = new List<EquipmentEntry>();

        public bool HasIssues =>
            RigMismatch || MissingItemIds.Count > 0 || IncompatibleItemIds.Count > 0 || MissingVariants.Count > 0;
    }
}
