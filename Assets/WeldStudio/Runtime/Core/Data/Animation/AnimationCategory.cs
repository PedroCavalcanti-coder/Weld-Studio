namespace WeldStudio.Core.Data
{
    /// <summary>How sample animations are grouped in the preview panel.</summary>
    public enum AnimationCategory
    {
        Idle = 0,
        Locomotion = 1,
        Gesture = 2,
        Pose = 3,

        /// <summary>Extreme poses (deep squat, arms overhead, twist) to check deformation of body and clothes.</summary>
        RangeOfMotion = 4,
    }
}
