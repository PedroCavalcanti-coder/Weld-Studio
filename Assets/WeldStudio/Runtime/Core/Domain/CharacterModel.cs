using System;
using System.Collections.Generic;

namespace WeldStudio.Core
{
    /// <summary>
    /// Single source of truth for one character: what it wears, which variant of each item and the value of
    /// every modifier. Pure C#: it knows nothing about scenes, Addressables or UI. Presenters change it
    /// through commands (undo/redo); the character assembler reacts to its events.
    /// </summary>
    /// <remarks>Not thread-safe; use it from the main thread.</remarks>
    public sealed class CharacterModel
    {
        private readonly List<EquippedItem> equipped = new List<EquippedItem>();
        private readonly List<EquipmentEntry> unresolvedEquipment = new List<EquipmentEntry>();
        private readonly Dictionary<string, float> modifiers = new Dictionary<string, float>(StringComparer.Ordinal);

        public CharacterModel(string rigId)
        {
            if (string.IsNullOrWhiteSpace(rigId)) throw new ArgumentException("Rig id is required.", nameof(rigId));
            RigId = rigId;
        }

        /// <summary>Raised when items are added or removed (not when only a variant changes).</summary>
        public event Action<EquipmentChange> EquipmentChanged;

        /// <summary>Raised with the updated item when only its material variant changes.</summary>
        public event Action<EquippedItem> VariantChanged;

        /// <summary>Raised with the modifier id when its value is set or reset; read it with <see cref="TryGetModifier"/>.</summary>
        public event Action<string> ModifierChanged;

        /// <summary>Raised once after <see cref="ApplyPreset"/> finished raising its individual events.</summary>
        public event Action PresetApplied;

        public string RigId { get; }

        /// <summary>Worn items, in the order they were equipped.</summary>
        public IReadOnlyList<EquippedItem> Equipped => equipped;

        /// <summary>Preset entries that could not be resolved; preserved so that saving does not lose them.</summary>
        public IReadOnlyList<EquipmentEntry> UnresolvedEquipment => unresolvedEquipment;

        /// <summary>Explicitly set modifier values. Modifiers absent here use their default value.</summary>
        public IReadOnlyDictionary<string, float> Modifiers => modifiers;

        public bool IsEquipped(string itemId) => IndexOf(itemId) >= 0;

        public bool TryGetEquipped(string itemId, out EquippedItem item)
        {
            int index = IndexOf(itemId);
            item = index >= 0 ? equipped[index] : null;
            return item != null;
        }

        /// <summary>Items that equipping <paramref name="definition"/> would remove.</summary>
        public IReadOnlyList<EquippedItem> GetConflicts(IEquipableDefinition definition)
        {
            var conflicts = new List<EquippedItem>();
            foreach (EquippedItem item in equipped)
            {
                if (item.ItemId != definition.Id && EquipmentRules.Conflicts(item.Definition, definition))
                    conflicts.Add(item);
            }
            return conflicts;
        }

        /// <summary>
        /// Wears <paramref name="definition"/>, removing every item it conflicts with. Does nothing if it is
        /// already worn (use <see cref="SetVariant"/> to change its look).
        /// </summary>
        /// <param name="variantId">Variant to use; null selects the item's default variant.</param>
        /// <exception cref="ArgumentException">Item is for another rig, or the variant does not exist.</exception>
        public EquipmentChange Equip(IEquipableDefinition definition, string variantId = null)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (definition.RigId != RigId)
                throw new ArgumentException($"Item '{definition.Id}' is rigged for '{definition.RigId}', not '{RigId}'.", nameof(definition));
            if (IsEquipped(definition.Id)) return EquipmentChange.Empty;

            string variant = ResolveVariant(definition, variantId);
            IReadOnlyList<EquippedItem> removed = GetConflicts(definition);
            foreach (EquippedItem item in removed) equipped.Remove(item);

            var added = new EquippedItem(definition, variant);
            equipped.Add(added);

            var change = new EquipmentChange(new[] { added }, removed);
            EquipmentChanged?.Invoke(change);
            return change;
        }

        public bool Unequip(string itemId)
        {
            int index = IndexOf(itemId);
            if (index < 0) return false;

            EquippedItem removed = equipped[index];
            equipped.RemoveAt(index);
            EquipmentChanged?.Invoke(new EquipmentChange(Array.Empty<EquippedItem>(), new[] { removed }));
            return true;
        }

        /// <returns>False when the item is not worn or already uses that variant.</returns>
        /// <exception cref="ArgumentException">The variant does not exist on the item.</exception>
        public bool SetVariant(string itemId, string variantId)
        {
            int index = IndexOf(itemId);
            if (index < 0) return false;

            EquippedItem current = equipped[index];
            string variant = ResolveVariant(current.Definition, variantId);
            if (variant == current.VariantId) return false;

            equipped[index] = current.WithVariant(variant);
            VariantChanged?.Invoke(equipped[index]);
            return true;
        }

        public bool TryGetModifier(string modifierId, out float value) => modifiers.TryGetValue(modifierId, out value);

        /// <summary>Sets a modifier value. Clamping to the modifier's range is done when it is applied.</summary>
        public void SetModifier(string modifierId, float value)
        {
            if (string.IsNullOrEmpty(modifierId)) throw new ArgumentException("Modifier id is required.", nameof(modifierId));
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(nameof(value), value, "Modifier values must be finite.");
            if (modifiers.TryGetValue(modifierId, out float current) && current == value) return;

            modifiers[modifierId] = value;
            ModifierChanged?.Invoke(modifierId);
        }

        /// <summary>Returns the modifier to its default value.</summary>
        public bool ResetModifier(string modifierId)
        {
            if (!modifiers.Remove(modifierId)) return false;
            ModifierChanged?.Invoke(modifierId);
            return true;
        }

        public CharacterPreset ToPreset(PresetMetadata metadata = null, string appVersion = null)
        {
            var preset = new CharacterPreset
            {
                CreatedWith = appVersion,
                RigId = RigId,
                Metadata = metadata ?? new PresetMetadata(),
                Modifiers = new Dictionary<string, float>(modifiers, StringComparer.Ordinal),
            };
            foreach (EquippedItem item in equipped)
                preset.Equipment.Add(new EquipmentEntry(item.ItemId, item.VariantId));
            foreach (EquipmentEntry entry in unresolvedEquipment)
                preset.Equipment.Add(new EquipmentEntry(entry.ItemId, entry.VariantId));
            return preset;
        }

        /// <summary>
        /// Replaces the whole state with <paramref name="preset"/>. Raises one <see cref="EquipmentChanged"/>
        /// with the difference, <see cref="VariantChanged"/> for items that only changed variant,
        /// <see cref="ModifierChanged"/> per changed modifier and finally <see cref="PresetApplied"/>.
        /// </summary>
        /// <param name="resolve">Finds an equipable by id, or returns null (usually the catalog service).</param>
        public PresetApplyReport ApplyPreset(CharacterPreset preset, Func<string, IEquipableDefinition> resolve)
        {
            if (preset == null) throw new ArgumentNullException(nameof(preset));
            if (resolve == null) throw new ArgumentNullException(nameof(resolve));

            var report = new PresetApplyReport { RigMismatch = !string.IsNullOrEmpty(preset.RigId) && preset.RigId != RigId };
            var target = new List<EquippedItem>();
            var unresolved = new List<EquipmentEntry>();

            foreach (EquipmentEntry entry in preset.Equipment ?? new List<EquipmentEntry>())
            {
                if (string.IsNullOrEmpty(entry?.ItemId)) continue;

                IEquipableDefinition definition = report.RigMismatch ? null : resolve(entry.ItemId);
                if (definition == null || definition.RigId != RigId)
                {
                    if (definition == null && !report.RigMismatch) report.MissingItemIds.Add(entry.ItemId);
                    else if (definition != null) report.IncompatibleItemIds.Add(entry.ItemId);
                    unresolved.Add(new EquipmentEntry(entry.ItemId, entry.VariantId));
                    continue;
                }

                string variant = entry.VariantId;
                if (variant != null && !definition.HasVariant(variant))
                {
                    report.MissingVariants.Add(new EquipmentEntry(entry.ItemId, entry.VariantId));
                    variant = null;
                }
                if (variant == null) variant = definition.DefaultVariantId;

                // Later entries win, exactly as if the user had equipped them in order.
                target.RemoveAll(item => item.ItemId == definition.Id || EquipmentRules.Conflicts(item.Definition, definition));
                target.Add(new EquippedItem(definition, variant));
            }

            var removed = new List<EquippedItem>();
            var added = new List<EquippedItem>();
            var variantChanges = new List<EquippedItem>();
            foreach (EquippedItem current in equipped)
            {
                EquippedItem next = target.Find(item => item.ItemId == current.ItemId);
                if (next == null) removed.Add(current);
                else if (next.VariantId != current.VariantId) variantChanges.Add(next);
            }
            foreach (EquippedItem next in target)
            {
                if (!IsEquipped(next.ItemId)) added.Add(next);
            }

            var changedModifiers = new List<string>();
            Dictionary<string, float> nextModifiers = preset.Modifiers ?? new Dictionary<string, float>();
            foreach (KeyValuePair<string, float> pair in modifiers)
            {
                if (!nextModifiers.TryGetValue(pair.Key, out float value) || value != pair.Value) changedModifiers.Add(pair.Key);
            }
            foreach (KeyValuePair<string, float> pair in nextModifiers)
            {
                if (!modifiers.ContainsKey(pair.Key)) changedModifiers.Add(pair.Key);
            }

            equipped.Clear();
            equipped.AddRange(target);
            unresolvedEquipment.Clear();
            unresolvedEquipment.AddRange(unresolved);
            modifiers.Clear();
            foreach (KeyValuePair<string, float> pair in nextModifiers)
            {
                if (!string.IsNullOrEmpty(pair.Key) && !float.IsNaN(pair.Value) && !float.IsInfinity(pair.Value))
                    modifiers[pair.Key] = pair.Value;
            }

            if (added.Count > 0 || removed.Count > 0) EquipmentChanged?.Invoke(new EquipmentChange(added, removed));
            foreach (EquippedItem item in variantChanges) VariantChanged?.Invoke(item);
            foreach (string modifierId in changedModifiers) ModifierChanged?.Invoke(modifierId);
            PresetApplied?.Invoke();
            return report;
        }

        private int IndexOf(string itemId)
        {
            for (int i = 0; i < equipped.Count; i++)
            {
                if (equipped[i].ItemId == itemId) return i;
            }
            return -1;
        }

        private static string ResolveVariant(IEquipableDefinition definition, string variantId)
        {
            if (variantId == null) return definition.DefaultVariantId;
            if (!definition.HasVariant(variantId))
                throw new ArgumentException($"Item '{definition.Id}' has no variant '{variantId}'.", nameof(variantId));
            return variantId;
        }
    }
}
