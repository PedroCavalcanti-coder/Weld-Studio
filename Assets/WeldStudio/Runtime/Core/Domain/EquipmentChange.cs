using System;
using System.Collections.Generic;

namespace WeldStudio.Core
{
    /// <summary>Items added to and removed from a character by a single operation.</summary>
    public sealed class EquipmentChange
    {
        public static readonly EquipmentChange Empty =
            new EquipmentChange(Array.Empty<EquippedItem>(), Array.Empty<EquippedItem>());

        public EquipmentChange(IReadOnlyList<EquippedItem> added, IReadOnlyList<EquippedItem> removed)
        {
            Added = added ?? throw new ArgumentNullException(nameof(added));
            Removed = removed ?? throw new ArgumentNullException(nameof(removed));
        }

        public IReadOnlyList<EquippedItem> Added { get; }
        public IReadOnlyList<EquippedItem> Removed { get; }
        public bool IsEmpty => Added.Count == 0 && Removed.Count == 0;
    }
}
