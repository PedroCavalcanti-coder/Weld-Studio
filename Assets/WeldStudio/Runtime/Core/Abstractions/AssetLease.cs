using System;

namespace WeldStudio.Core
{
    /// <summary>
    /// A loaded asset plus the obligation to release it. Disposing the lease releases the asset exactly once,
    /// so load/release pairs cannot get out of balance.
    /// </summary>
    public sealed class AssetLease<T> : IDisposable where T : UnityEngine.Object
    {
        private Action release;

        public AssetLease(T asset, Action release)
        {
            Asset = asset;
            this.release = release;
        }

        public T Asset { get; }
        public bool IsReleased => release == null;

        public void Dispose()
        {
            Action pending = release;
            release = null;
            pending?.Invoke();
        }
    }
}
