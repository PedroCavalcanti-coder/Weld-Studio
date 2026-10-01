using UnityEngine;

namespace WeldStudio.Core.Data
{
    /// <summary>
    /// Base class for physics settings attached to catalog items (cloth, hair strands, jiggle).
    /// Each physics backend ships its own subclass in its own assembly (Unity Cloth in
    /// WeldStudio.Physics, Magica Cloth 2 in an optional module), so the core never depends on a
    /// physics package.
    /// </summary>
    /// <remarks>
    /// Item prefabs stay physics-agnostic: the backend adds and configures its components after the
    /// item has been bound to the character's skeleton.
    /// </remarks>
    public abstract class PhysicsProfileData : ScriptableObject
    {
        /// <summary>Identifier of the backend able to consume this profile, e.g. <c>"unity.cloth"</c>.</summary>
        public abstract string BackendId { get; }
    }
}
