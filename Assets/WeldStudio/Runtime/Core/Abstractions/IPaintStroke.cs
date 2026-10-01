using UnityEngine;

namespace WeldStudio.Core
{
    /// <summary>A brush stroke in progress.</summary>
    public interface IPaintStroke
    {
        /// <summary>Paints where <paramref name="ray"/> hits the layer's target mesh; misses are ignored.</summary>
        /// <param name="pressure">Pen pressure in [0, 1]; 1 for mouse input.</param>
        void AddSample(Ray ray, float pressure = 1f);

        /// <summary>Ends the stroke. The returned command restores the touched pixels on undo.</summary>
        ICommand Complete();

        /// <summary>Discards the stroke and restores the touched pixels (e.g. Esc while dragging).</summary>
        void Cancel();
    }
}
