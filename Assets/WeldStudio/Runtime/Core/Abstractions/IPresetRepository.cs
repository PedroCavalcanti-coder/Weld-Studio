using System.Threading;
using System.Threading.Tasks;

namespace WeldStudio.Core
{
    /// <summary>
    /// Saves and loads character presets together with their attachments (painted layers) as a single file.
    /// Implementations must not block the main thread.
    /// </summary>
    public interface IPresetRepository
    {
        /// <summary>File extension including the dot, e.g. <c>".weld"</c>.</summary>
        string FileExtension { get; }

        /// <summary>Folder used when the user does not pick one.</summary>
        string DefaultDirectory { get; }

        /// <summary>Writes atomically: an interrupted save never leaves a half-written preset behind.</summary>
        Task SaveAsync(PresetPackage package, string path, CancellationToken cancellationToken = default);

        /// <summary>Reads a preset file, migrating the preset to the current schema version.</summary>
        /// <exception cref="PresetFormatException">The file is not a valid preset or is from a newer version.</exception>
        Task<PresetPackage> LoadAsync(string path, CancellationToken cancellationToken = default);
    }
}
