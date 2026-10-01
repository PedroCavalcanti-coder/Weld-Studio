using System;

namespace WeldStudio.Core
{
    /// <summary>
    /// Regions of the base body mesh that a worn item can hide, so skin never pokes through clothing
    /// and covered skin costs no overdraw.
    /// </summary>
    /// <remarks>
    /// The bit index of each member is the region id baked into the body mesh by the content pipeline
    /// (see docs/ARCHITECTURE.md). Changing an existing value breaks every body mesh: only append.
    /// </remarks>
    [Flags]
    public enum BodyRegion
    {
        None = 0,
        Scalp = 1 << 0,
        Face = 1 << 1,
        Neck = 1 << 2,
        Chest = 1 << 3,
        Abdomen = 1 << 4,
        Pelvis = 1 << 5,
        UpperArmLeft = 1 << 6,
        UpperArmRight = 1 << 7,
        ForearmLeft = 1 << 8,
        ForearmRight = 1 << 9,
        HandLeft = 1 << 10,
        HandRight = 1 << 11,
        ThighLeft = 1 << 12,
        ThighRight = 1 << 13,
        LowerLegLeft = 1 << 14,
        LowerLegRight = 1 << 15,
        FootLeft = 1 << 16,
        FootRight = 1 << 17,
    }
}
