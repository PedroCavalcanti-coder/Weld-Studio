using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WeldStudio.Core;

namespace WeldStudio.Persistence
{
    /// <summary>
    /// Stores presets as JSON files. Serialization runs on a worker thread and file access is asynchronous,
    /// so saving and loading never stall the main thread.
    /// </summary>
    public sealed class JsonPresetRepository : IPresetRepository
    {
        public const string Extension = ".weld.json";

        private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

        private readonly PresetMigrator migrator;

        /// <param name="defaultDirectory">Usually <c>Application.persistentDataPath/Presets</c>, provided by the composition root.</param>
        public JsonPresetRepository(string defaultDirectory, PresetMigrator migrator = null)
        {
            if (string.IsNullOrWhiteSpace(defaultDirectory))
                throw new ArgumentException("Default directory is required.", nameof(defaultDirectory));
            DefaultDirectory = defaultDirectory;
            this.migrator = migrator ?? PresetMigrator.Default;
        }

        public string FileExtension => Extension;
        public string DefaultDirectory { get; }

        public async Task SaveAsync(CharacterPreset preset, string path, CancellationToken cancellationToken = default)
        {
            if (preset == null) throw new ArgumentNullException(nameof(preset));
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path is required.", nameof(path));

            string json = await Task.Run(() => PresetJson.Serialize(preset), cancellationToken);

            string directory = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

            // Write next to the destination, then swap: a crash mid-save leaves the previous file intact.
            string temporaryPath = path + ".tmp";
            try
            {
                using (var stream = new FileStream(temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                using (var writer = new StreamWriter(stream, Utf8NoBom))
                {
                    await writer.WriteAsync(json);
                    await writer.FlushAsync();
                }

                cancellationToken.ThrowIfCancellationRequested();
                if (File.Exists(path)) File.Replace(temporaryPath, path, null);
                else File.Move(temporaryPath, path);
            }
            finally
            {
                if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
            }
        }

        public async Task<CharacterPreset> LoadAsync(string path, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path is required.", nameof(path));

            string json;
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
            using (var reader = new StreamReader(stream, Utf8NoBom))
            {
                json = await reader.ReadToEndAsync();
            }

            cancellationToken.ThrowIfCancellationRequested();
            return await Task.Run(() => PresetJson.Deserialize(json, migrator), cancellationToken);
        }

        /// <summary>Full path for a preset named <paramref name="presetName"/> in the default directory.</summary>
        public string GetDefaultPath(string presetName)
        {
            if (string.IsNullOrWhiteSpace(presetName)) throw new ArgumentException("Name is required.", nameof(presetName));

            var safeName = new StringBuilder(presetName.Trim());
            foreach (char invalid in Path.GetInvalidFileNameChars()) safeName.Replace(invalid, '_');
            return Path.Combine(DefaultDirectory, safeName + Extension);
        }
    }
}
