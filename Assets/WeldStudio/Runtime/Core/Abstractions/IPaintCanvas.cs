namespace WeldStudio.Core
{
    /// <summary>
    /// The painting engine: holds the pixels of every paint layer (in the UV space of the body or of a worn
    /// item) and composites them over the base textures. Paint layer metadata (order, opacity, blend mode)
    /// comes from the <see cref="CharacterModel"/>.
    /// </summary>
    public interface IPaintCanvas
    {
        /// <summary>
        /// Starts a brush stroke on <paramref name="layerId"/>. Feed it pointer rays while the user drags and
        /// call <see cref="IPaintStroke.Complete"/> on release to obtain the undoable command.
        /// </summary>
        IPaintStroke BeginStroke(string layerId, BrushSettings brush);

        /// <summary>Encodes a layer's pixels as PNG for saving (main thread).</summary>
        byte[] EncodeLayer(string layerId);

        /// <summary>Restores a layer's pixels from a PNG attachment (main thread).</summary>
        void LoadLayer(string layerId, byte[] png);

        /// <summary>Frees a layer's pixels once no command can bring the layer back.</summary>
        void ReleaseLayer(string layerId);
    }
}
