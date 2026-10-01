namespace WeldStudio.Core
{
    /// <summary>
    /// Data-side view of anything that can be worn (clothing, hair, accessories). This is what the
    /// <see cref="CharacterModel"/> reasons about; it never sees prefabs or scene objects.
    /// </summary>
    public interface IEquipableDefinition
    {
        /// <summary>Stable id saved in presets.</summary>
        string Id { get; }

        /// <summary>Skeleton the item was rigged against; must match the character's rig.</summary>
        string RigId { get; }

        EquipmentSlot Slots { get; }
        EquipmentLayer Layer { get; }

        /// <summary>Id of the default material variant, or null when the item has no variants.</summary>
        string DefaultVariantId { get; }

        bool HasVariant(string variantId);
    }
}
