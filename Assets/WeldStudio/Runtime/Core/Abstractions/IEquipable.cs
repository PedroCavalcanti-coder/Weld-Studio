using System;
using System.Collections.Generic;
using UnityEngine;

namespace WeldStudio.Core
{
    /// <summary>
    /// A worn piece instantiated in the scene (the runtime counterpart of an
    /// <see cref="IEquipableDefinition"/>). Implementations decide how the piece is attached: skinned to the
    /// shared skeleton, parented to a single bone, generated procedurally...
    /// </summary>
    /// <remarks>
    /// Disposing an equipable destroys its scene objects. It does not release the loaded asset: the asset
    /// lease is owned by whoever loaded it.
    /// </remarks>
    public interface IEquipable : IDisposable
    {
        IEquipableDefinition Definition { get; }

        /// <summary>Root of the instantiated piece.</summary>
        GameObject Root { get; }

        /// <summary>Renderers in hierarchy order; material variants index into their flattened slots.</summary>
        IReadOnlyList<Renderer> Renderers { get; }

        /// <summary>Body regions to hide while this piece is attached.</summary>
        BodyRegion HiddenBodyRegions { get; }

        bool IsAttached { get; }

        void Attach(ICharacterRig rig);
        void Detach();
    }
}
