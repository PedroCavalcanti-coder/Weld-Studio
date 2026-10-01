using System;
using WeldStudio.Core.Data;

namespace WeldStudio.Core
{
    /// <summary>
    /// A physics engine able to simulate worn pieces (Unity Cloth, spring bones, Magica Cloth 2...).
    /// Backends live in their own assemblies; the core only knows this contract.
    /// </summary>
    public interface IPhysicsBackend
    {
        /// <summary>Matches <see cref="PhysicsProfileData.BackendId"/> of the profiles it consumes.</summary>
        string BackendId { get; }

        /// <summary>
        /// Adds and configures simulation on an attached piece. Disposing the result removes it again.
        /// </summary>
        IDisposable Apply(IEquipable target, PhysicsProfileData profile, ICharacterRig rig);

        /// <summary>The body's shape changed: colliders derived from it must be rebuilt.</summary>
        void OnBodyShapeChanged(ICharacterRig rig);
    }
}
