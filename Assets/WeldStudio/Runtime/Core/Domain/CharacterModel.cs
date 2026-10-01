using System;
using System.Collections.Generic;

namespace WeldStudio.Core
{
    /// <summary>
    /// Single source of truth for one character: what it wears (with variant and customisation of each item),
    /// the value of every modifier and the paint layers. Pure C#: it knows nothing about scenes, Addressables
    /// or UI. Presenters change it through commands (undo/redo); the character assembler reacts to its events.
    /// </summary>
    /// <remarks>Not thread-safe; use it from the main thread.</remarks>
    public sealed class CharacterModel
    {
        private readonly List<EquippedItem> equipped = new List<EquippedItem>();
        private readonly List<EquipmentEntry> unresolvedEquipment = new List<EquipmentEntry>();
        private readonly Dictionary<string, float> modifiers = new Dictionary<string, float>(StringComparer.Ordinal);
        private readonly List<PaintLayer> paintLayers = new List<PaintLayer>();

        public CharacterModel(string rigId)
        {
            if (string.IsNullOrWhiteSpace(rigId)) throw new ArgumentException("Rig id is required.", nameof(rigId));
            RigId = rigId;
        }

        /// <summary>Raised when items are added or removed (not when only a variant or appearance changes).</summary>
        public event Action<EquipmentChange> EquipmentChanged;

        /// <summary>Raised with the updated item when only its material variant changes.</summary>
        public event Action<EquippedItem> VariantChanged;

        /// <summary>
        /// Raised with the updated item and the colour zone or parameter id that changed; the id is null when
        /// several changed at once (preset applied).
        /// </summary>
        public event Action<EquippedItem, string> AppearanceChanged;

        /// <summary>Raised with the modifier id when its value is set or reset; read it with <see cref="TryGetModifier"/>.</summary>
        public event Action<string> ModifierChanged;

        /// <summary>Raised with the id of the layer added, removed or edited; null when the order or the whole stack changed.</summary>
        public event Action<string> PaintLayersChanged;

        /// <summary>Raised once after <see cref="ApplyPreset"/> finished raising its individual events.</summary>
        public event Action PresetApplied;

        public string RigId { get; }

        /// <summary>Worn items, in the order they were equipped.</summary>
        public IReadOnlyList<EquippedItem> Equipped => equipped;

        /// <summary>Preset entries that could not be resolved; preserved so that saving does not lose them.</summary>
        public IReadOnlyList<EquipmentEntry> UnresolvedEquipment => unresolvedEquipment;

        /// <summary>Explicitly set modifier values. Modifiers absent here use their default value.</summary>
        public IReadOnlyDictionary<string, float> Modifiers => modifiers;

        /// <summary>Paint layers, bottom to top.</summary>
        public IReadOnlyList<PaintLayer> PaintLayers => paintLayers;

        // ---------------------------------------------------------------- equipment

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
        /// already worn (use <see cref="SetVariant"/> or <see cref="SetItemColor"/> to change its look).
        /// </summary>
        /// <param name="variantId">Variant to use; null selects the item's default variant.</param>
        /// <param name="appearance">Initial customisation (e.g. restored by undo); null for none.</param>
        /// <exception cref="ArgumentException">Item is for another rig, or the variant does not exist.</exception>
        public EquipmentChange Equip(IEquipableDefinition definition, string variantId = null, ItemAppearance appearance = null)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (definition.RigId != RigId)
                throw new ArgumentException($"Item '{definition.Id}' is rigged for '{definition.RigId}', not '{RigId}'.", nameof(definition));
            if (IsEquipped(definition.Id)) return EquipmentChange.Empty;

            string variant = ResolveVariant(definition, variantId);
            IReadOnlyList<EquippedItem> removed = GetConflicts(definition);
            foreach (EquippedItem item in removed) equipped.Remove(item);

            var added = new EquippedItem(definition, variant, appearance);
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

        /// <param name="color">New colour, or null to return the zone to the definition's default.</param>
        /// <returns>False when the item is not worn or nothing changed.</returns>
        public bool SetItemColor(string itemId, string zoneId, ColorRgba? color)
        {
            int index = IndexOf(itemId);
            if (index < 0) return false;

            ItemAppearance current = equipped[index].Appearance;
            bool had = current.TryGetColor(zoneId, out ColorRgba existing);
            if (color.HasValue ? had && existing == color.Value : !had) return false;

            return ReplaceAppearance(index, current.WithColor(zoneId, color), zoneId);
        }

        /// <param name="value">New value, or null to return the parameter to the definition's default.</param>
        /// <returns>False when the item is not worn or nothing changed.</returns>
        public bool SetItemParameter(string itemId, string parameterId, float? value)
        {
            int index = IndexOf(itemId);
            if (index < 0) return false;

            ItemAppearance current = equipped[index].Appearance;
            bool had = current.TryGetParameter(parameterId, out float existing);
            if (value.HasValue ? had && existing == value.Value : !had) return false;

            return ReplaceAppearance(index, current.WithParameter(parameterId, value), parameterId);
        }

        // ---------------------------------------------------------------- modifiers

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

        // ---------------------------------------------------------------- paint layers

        public int IndexOfPaintLayer(string layerId)
        {
            for (int i = 0; i < paintLayers.Count; i++)
            {
                if (paintLayers[i].Id == layerId) return i;
            }
            return -1;
        }

        public bool TryGetPaintLayer(string layerId, out PaintLayer layer)
        {
            int index = IndexOfPaintLayer(layerId);
            layer = index >= 0 ? paintLayers[index] : null;
            return layer != null;
        }

        /// <param name="index">Position from the bottom; -1 (default) puts it on top.</param>
        /// <exception cref="ArgumentException">A layer with the same id already exists.</exception>
        public void AddPaintLayer(PaintLayer layer, int index = -1)
        {
            if (layer == null) throw new ArgumentNullException(nameof(layer));
            if (IndexOfPaintLayer(layer.Id) >= 0) throw new ArgumentException($"Paint layer '{layer.Id}' already exists.", nameof(layer));

            paintLayers.Insert(index < 0 || index > paintLayers.Count ? paintLayers.Count : index, layer);
            PaintLayersChanged?.Invoke(layer.Id);
        }

        /// <returns>The index the layer had, or -1 when it did not exist.</returns>
        public int RemovePaintLayer(string layerId)
        {
            int index = IndexOfPaintLayer(layerId);
            if (index < 0) return -1;

            paintLayers.RemoveAt(index);
            PaintLayersChanged?.Invoke(layerId);
            return index;
        }

        /// <summary>Replaces the settings (name, opacity, blend mode, visibility) of the layer with the same id.</summary>
        /// <exception cref="ArgumentException">The layer's target cannot change; paint a new layer instead.</exception>
        public bool UpdatePaintLayer(PaintLayer layer)
        {
            if (layer == null) throw new ArgumentNullException(nameof(layer));
            int index = IndexOfPaintLayer(layer.Id);
            if (index < 0 || paintLayers[index].HasSameSettings(layer)) return false;
            if (paintLayers[index].TargetId != layer.TargetId)
                throw new ArgumentException("A paint layer cannot change target.", nameof(layer));

            paintLayers[index] = layer;
            PaintLayersChanged?.Invoke(layer.Id);
            return true;
        }

        public bool MovePaintLayer(string layerId, int newIndex)
        {
            int index = IndexOfPaintLayer(layerId);
            if (index < 0) return false;

            newIndex = Math.Max(0, Math.Min(paintLayers.Count - 1, newIndex));
            if (newIndex == index) return false;

            PaintLayer layer = paintLayers[index];
            paintLayers.RemoveAt(index);
            paintLayers.Insert(newIndex, layer);
            PaintLayersChanged?.Invoke(null);
            return true;
        }

        // ---------------------------------------------------------------- presets

        public CharacterPreset ToPreset(PresetMetadata metadata = null, string appVersion = null)
        {
            var preset = new CharacterPreset
            {
                CreatedWith = appVersion,
                RigId = RigId,
                Metadata = metadata ?? new PresetMetadata(),
                Modifiers = new Dictionary<string, float>(modifiers, StringComparer.Ordinal),
            };
            foreach (EquippedItem item in equipped) preset.Equipment.Add(ToEntry(item));
            foreach (EquipmentEntry entry in unresolvedEquipment) preset.Equipment.Add(entry.Clone());
            foreach (PaintLayer layer in paintLayers)
            {
                preset.PaintLayers.Add(new PaintLayerEntry
                {
                    Id = layer.Id,
                    TargetId = layer.TargetId,
                    Name = layer.Name,
                    Opacity = layer.Opacity,
                    BlendMode = layer.BlendMode,
                    Visible = layer.Visible,
                    Image = PaintLayerEntry.ImageNameFor(layer.Id),
                });
            }
            return preset;
        }

        /// <summary>
        /// Replaces the whole state with <paramref name="preset"/>. Raises one <see cref="EquipmentChanged"/>
        /// with the difference, <see cref="VariantChanged"/> / <see cref="AppearanceChanged"/> for items that
        /// stayed but look different, <see cref="ModifierChanged"/> per changed modifier,
        /// <see cref="PaintLayersChanged"/> if the layers differ, and finally <see cref="PresetApplied"/>.
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
                    unresolved.Add(entry.Clone());
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
                target.Add(new EquippedItem(definition, variant, ReadAppearance(entry, report)));
            }

            var removed = new List<EquippedItem>();
            var added = new List<EquippedItem>();
            var variantChanges = new List<EquippedItem>();
            var appearanceChanges = new List<EquippedItem>();
            foreach (EquippedItem current in equipped)
            {
                EquippedItem next = target.Find(item => item.ItemId == current.ItemId);
                if (next == null)
                {
                    removed.Add(current);
                    continue;
                }
                if (next.VariantId != current.VariantId) variantChanges.Add(next);
                if (!next.Appearance.Equals(current.Appearance)) appearanceChanges.Add(next);
            }
            foreach (EquippedItem next in target)
            {
                if (!IsEquipped(next.ItemId)) added.Add(next);
            }

            var changedModifiers = new List<string>();
            var nextModifiers = new Dictionary<string, float>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, float> pair in preset.Modifiers ?? new Dictionary<string, float>())
            {
                if (!string.IsNullOrEmpty(pair.Key) && !float.IsNaN(pair.Value) && !float.IsInfinity(pair.Value))
                    nextModifiers[pair.Key] = pair.Value;
            }
            foreach (KeyValuePair<string, float> pair in modifiers)
            {
                if (!nextModifiers.TryGetValue(pair.Key, out float value) || value != pair.Value) changedModifiers.Add(pair.Key);
            }
            foreach (string key in nextModifiers.Keys)
            {
                if (!modifiers.ContainsKey(key)) changedModifiers.Add(key);
            }

            List<PaintLayer> nextLayers = ReadPaintLayers(preset, report);
            bool layersChanged = nextLayers.Count != paintLayers.Count;
            for (int i = 0; !layersChanged && i < nextLayers.Count; i++) layersChanged = !nextLayers[i].HasSameSettings(paintLayers[i]);

            equipped.Clear();
            equipped.AddRange(target);
            unresolvedEquipment.Clear();
            unresolvedEquipment.AddRange(unresolved);
            modifiers.Clear();
            foreach (KeyValuePair<string, float> pair in nextModifiers) modifiers[pair.Key] = pair.Value;
            paintLayers.Clear();
            paintLayers.AddRange(nextLayers);

            if (added.Count > 0 || removed.Count > 0) EquipmentChanged?.Invoke(new EquipmentChange(added, removed));
            foreach (EquippedItem item in variantChanges) VariantChanged?.Invoke(item);
            foreach (EquippedItem item in appearanceChanges) AppearanceChanged?.Invoke(item, null);
            foreach (string modifierId in changedModifiers) ModifierChanged?.Invoke(modifierId);
            if (layersChanged) PaintLayersChanged?.Invoke(null);
            PresetApplied?.Invoke();
            return report;
        }

        // ---------------------------------------------------------------- helpers

        private int IndexOf(string itemId)
        {
            for (int i = 0; i < equipped.Count; i++)
            {
                if (equipped[i].ItemId == itemId) return i;
            }
            return -1;
        }

        private bool ReplaceAppearance(int index, ItemAppearance appearance, string changedId)
        {
            equipped[index] = equipped[index].WithAppearance(appearance);
            AppearanceChanged?.Invoke(equipped[index], changedId);
            return true;
        }

        private static string ResolveVariant(IEquipableDefinition definition, string variantId)
        {
            if (variantId == null) return definition.DefaultVariantId;
            if (!definition.HasVariant(variantId))
                throw new ArgumentException($"Item '{definition.Id}' has no variant '{variantId}'.", nameof(variantId));
            return variantId;
        }

        private static EquipmentEntry ToEntry(EquippedItem item)
        {
            var entry = new EquipmentEntry(item.ItemId, item.VariantId);
            foreach (KeyValuePair<string, ColorRgba> pair in item.Appearance.Colors) entry.Colors[pair.Key] = pair.Value.ToHex();
            foreach (KeyValuePair<string, float> pair in item.Appearance.Parameters) entry.Parameters[pair.Key] = pair.Value;
            return entry;
        }

        private static ItemAppearance ReadAppearance(EquipmentEntry entry, PresetApplyReport report)
        {
            var colors = new Dictionary<string, ColorRgba>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, string> pair in entry.Colors ?? new Dictionary<string, string>())
            {
                if (ColorRgba.TryParseHex(pair.Value, out ColorRgba color)) colors[pair.Key] = color;
                else report.InvalidColors.Add($"{entry.ItemId}/{pair.Key}");
            }
            return ItemAppearance.Create(colors, entry.Parameters);
        }

        private static List<PaintLayer> ReadPaintLayers(CharacterPreset preset, PresetApplyReport report)
        {
            var layers = new List<PaintLayer>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (PaintLayerEntry entry in preset.PaintLayers ?? new List<PaintLayerEntry>())
            {
                if (entry == null) continue;
                if (string.IsNullOrWhiteSpace(entry.Id) || string.IsNullOrWhiteSpace(entry.TargetId) ||
                    float.IsNaN(entry.Opacity) || !ids.Add(entry.Id))
                {
                    report.InvalidPaintLayers.Add(entry.Id ?? "(no id)");
                    continue;
                }
                layers.Add(new PaintLayer(entry.Id, entry.TargetId, entry.Name, entry.Opacity, entry.BlendMode, entry.Visible));
            }
            return layers;
        }
    }
}
