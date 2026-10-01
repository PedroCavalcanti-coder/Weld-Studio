namespace WeldStudio.Core
{
    /// <summary>Rules that decide which equipables can be worn together.</summary>
    public static class EquipmentRules
    {
        /// <summary>
        /// Two equipables conflict when they share at least one slot on the same layer. Equipping one removes
        /// every equipped item it conflicts with.
        /// </summary>
        public static bool Conflicts(IEquipableDefinition a, IEquipableDefinition b) =>
            a != null && b != null && a.Layer == b.Layer && (a.Slots & b.Slots) != 0;
    }
}
