namespace WeldStudio.Core
{
    /// <summary>
    /// Stacking order of worn items, from the skin outwards. Items on different layers never replace
    /// each other. The layer is also the tie-breaker when nearly coincident surfaces fight for depth:
    /// outer layers win.
    /// </summary>
    /// <remarks>
    /// Values are spaced so new layers can be inserted without renumbering serialized data.
    /// </remarks>
    public enum EquipmentLayer
    {
        Underwear = 0,   // underwear, socks, skin-tight suits
        Base = 100,      // shirts, trousers, dresses, hair
        Mid = 200,       // sweaters, vests
        Outer = 300,     // jackets, coats, capes
        Accessory = 400, // belts, bags, jewellery, glasses
    }
}
