using System;

namespace WeldStudio.Core
{
    /// <summary>
    /// Anatomical slots an equipable item occupies. An item may occupy several slots
    /// (a dress is <c>Torso | Legs</c>). Two items conflict only when they share at least one slot
    /// <b>and</b> the same <see cref="EquipmentLayer"/>, so a shirt and a jacket can be worn together.
    /// </summary>
    /// <remarks>
    /// Serialized as an integer in assets: never renumber existing members, only append new bits.
    /// </remarks>
    [Flags]
    public enum EquipmentSlot
    {
        None = 0,
        Hair = 1 << 0,
        FacialHair = 1 << 1,
        Head = 1 << 2,   // hats, helmets
        Face = 1 << 3,   // glasses, masks
        Neck = 1 << 4,   // scarves, necklaces
        Torso = 1 << 5,  // shirts, jackets, upper part of dresses
        Back = 1 << 6,   // capes, backpacks
        Hands = 1 << 7,  // gloves
        Waist = 1 << 8,  // belts
        Legs = 1 << 9,   // trousers, skirts
        Feet = 1 << 10,  // shoes, socks
    }
}
