using System.Threading;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace WeldStudio.Core
{
    /// <summary>
    /// The only gateway to Addressables. Loads are reference-counted per key: two characters wearing the
    /// same item share one load, and the asset is unloaded when the last lease is disposed.
    /// </summary>
    public interface IAssetProvider
    {
        /// <remarks>
        /// If <paramref name="cancellationToken"/> is cancelled, the load is released as soon as it completes
        /// and the task is cancelled; callers never receive (or have to release) an obsolete asset.
        /// </remarks>
        Task<AssetLease<T>> LoadAsync<T>(AssetReference reference, CancellationToken cancellationToken = default)
            where T : UnityEngine.Object;

        /// <summary>Registers an external content catalog (content pack or mod) built with Addressables.</summary>
        Task LoadContentCatalogAsync(string catalogPath, CancellationToken cancellationToken = default);
    }
}
