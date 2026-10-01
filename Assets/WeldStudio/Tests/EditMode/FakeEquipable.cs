using System;
using System.Collections.Generic;
using WeldStudio.Core;

namespace WeldStudio.Tests
{
    /// <summary>In-memory equipable definition, so domain tests need no ScriptableObjects.</summary>
    internal sealed class FakeEquipable : IEquipableDefinition
    {
        public const string Rig = "test.rig";

        private readonly HashSet<string> variants;

        public FakeEquipable(string id, EquipmentSlot slots, EquipmentLayer layer = EquipmentLayer.Base,
            string rigId = Rig, params string[] variants)
        {
            Id = id;
            Slots = slots;
            Layer = layer;
            RigId = rigId;
            this.variants = new HashSet<string>(variants, StringComparer.Ordinal);
            DefaultVariantId = variants.Length > 0 ? variants[0] : null;
        }

        public string Id { get; }
        public string RigId { get; }
        public EquipmentSlot Slots { get; }
        public EquipmentLayer Layer { get; }
        public string DefaultVariantId { get; }

        public bool HasVariant(string variantId) => variantId != null && variants.Contains(variantId);

        /// <summary>Resolver over a fixed set of items, as the catalog service would provide.</summary>
        public static Func<string, IEquipableDefinition> Resolver(params FakeEquipable[] items)
        {
            var byId = new Dictionary<string, IEquipableDefinition>();
            foreach (FakeEquipable item in items) byId[item.Id] = item;
            return id => byId.TryGetValue(id, out IEquipableDefinition item) ? item : null;
        }
    }
}
