using System.Threading;
using System.Threading.Tasks;

namespace WeldStudio.Core
{
    /// <summary>Saves and loads character presets. Implementations must not block the main thread.</summary>
    public interface IPresetRepository
    {
        /// <summary>File extension including the dot, e.g. <c>".weld.json"</c>.</summary>
        string FileExtension { get; }

        /// <summary>Folder used when the user does not pick one.</summary>
        string DefaultDirectory { get; }

        /// <summary>Writes atomically: an interrupted save never leaves a half-written preset behind.</summary>
        Task SaveAsync(CharacterPreset preset, string path, CancellationToken cancellationToken = default);

        /// <summary>Reads and migrates a preset to the current schema version.</summary>
        /// <exception cref="PresetFormatException">The file is not a valid preset or is from a newer version.</exception>
        Task<CharacterPreset> LoadAsync(string path, CancellationToken cancellationToken = default);
    }
}
