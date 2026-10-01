using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WeldStudio.Core;

namespace WeldStudio.Persistence
{
    /// <summary>
    /// Stores presets as <c>.weld</c> files: a zip archive with <c>preset.json</c> and the attachments
    /// (painted layers as PNG) under <c>attachments/</c>. One file is easy to share; the JSON inside stays
    /// readable and diffable.
    /// </summary>
    /// <remarks>
    /// Archive work runs on a worker thread and the file is replaced atomically. Preset files come from other
    /// users, so reading is defensive: bounded entry count and sizes, and attachment names that cannot escape
    /// the package.
    /// </remarks>
    public sealed class PackagePresetRepository : IPresetRepository
    {
        public const string Extension = ".weld";
        public const string PresetEntryName = "preset.json";
        public const string AttachmentFolder = "attachments/";

        public const int MaxEntries = 512;
        public const long MaxEntryBytes = 64L * 1024 * 1024;
        public const long MaxTotalBytes = 512L * 1024 * 1024;

        private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

        private readonly PresetMigrator migrator;

        /// <param name="defaultDirectory">Usually <c>Application.persistentDataPath/Presets</c>, provided by the composition root.</param>
        public PackagePresetRepository(string defaultDirectory, PresetMigrator migrator = null)
        {
            if (string.IsNullOrWhiteSpace(defaultDirectory))
                throw new ArgumentException("Default directory is required.", nameof(defaultDirectory));
            DefaultDirectory = defaultDirectory;
            this.migrator = migrator ?? PresetMigrator.Default;
        }

        public string FileExtension => Extension;
        public string DefaultDirectory { get; }

        public async Task SaveAsync(PresetPackage package, string path, CancellationToken cancellationToken = default)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path is required.", nameof(path));

            string directory = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

            // Write next to the destination, then swap: a crash mid-save leaves the previous file intact.
            string temporaryPath = path + ".tmp";
            try
            {
                await Task.Run(() => WriteArchive(package, temporaryPath, cancellationToken), cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();
                if (File.Exists(path)) File.Replace(temporaryPath, path, null);
                else File.Move(temporaryPath, path);
            }
            finally
            {
                if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
            }
        }

        public Task<PresetPackage> LoadAsync(string path, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path is required.", nameof(path));
            return Task.Run(() => ReadArchive(path, cancellationToken), cancellationToken);
        }

        /// <summary>Full path for a preset named <paramref name="presetName"/> in the default directory.</summary>
        public string GetDefaultPath(string presetName)
        {
            if (string.IsNullOrWhiteSpace(presetName)) throw new ArgumentException("Name is required.", nameof(presetName));

            var safeName = new StringBuilder(presetName.Trim());
            foreach (char invalid in Path.GetInvalidFileNameChars()) safeName.Replace(invalid, '_');
            safeName.Replace('/', '_').Replace('\\', '_');
            return Path.Combine(DefaultDirectory, safeName + Extension);
        }

        private static void WriteArchive(PresetPackage package, string archivePath, CancellationToken cancellationToken)
        {
            string json = PresetJson.Serialize(package.Preset);
            using (var stream = new FileStream(archivePath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                ZipArchiveEntry presetEntry = archive.CreateEntry(PresetEntryName, CompressionLevel.Optimal);
                using (var writer = new StreamWriter(presetEntry.Open(), Utf8NoBom)) writer.Write(json);

                foreach (var attachment in package.Attachments)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    // PNGs are already compressed; storing them avoids wasting time compressing them again.
                    ZipArchiveEntry entry = archive.CreateEntry(AttachmentFolder + attachment.Key, CompressionLevel.NoCompression);
                    using (Stream entryStream = entry.Open()) entryStream.Write(attachment.Value, 0, attachment.Value.Length);
                }
            }
        }

        private PresetPackage ReadArchive(string path, CancellationToken cancellationToken)
        {
            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    if (archive.Entries.Count > MaxEntries)
                        throw new PresetFormatException($"Preset file has more than {MaxEntries} entries.");

                    ZipArchiveEntry presetEntry = archive.GetEntry(PresetEntryName)
                        ?? throw new PresetFormatException($"Preset file has no {PresetEntryName}.");

                    long total = 0;
                    string json = Utf8NoBom.GetString(ReadBounded(presetEntry, ref total));
                    var package = new PresetPackage(PresetJson.Deserialize(json, migrator));

                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (!entry.FullName.StartsWith(AttachmentFolder, StringComparison.Ordinal)) continue;

                        string name = entry.FullName.Substring(AttachmentFolder.Length);
                        if (name.Length == 0) continue; // directory entry
                        if (!PresetPackage.IsValidAttachmentName(name))
                            throw new PresetFormatException($"Preset file contains an invalid attachment name '{entry.FullName}'.");

                        package.SetAttachment(name, ReadBounded(entry, ref total));
                    }
                    return package;
                }
            }
            catch (InvalidDataException exception)
            {
                throw new PresetFormatException("Preset file is not a valid .weld archive.", exception);
            }
        }

        /// <summary>
        /// Reads an entry counting real decompressed bytes: the sizes declared in the archive can lie
        /// (zip bombs), so they are not trusted.
        /// </summary>
        private static byte[] ReadBounded(ZipArchiveEntry entry, ref long total)
        {
            using (Stream input = entry.Open())
            using (var output = new MemoryStream())
            {
                var buffer = new byte[81920];
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    total += read;
                    if (output.Length + read > MaxEntryBytes || total > MaxTotalBytes)
                        throw new PresetFormatException("Preset file is too large.");
                    output.Write(buffer, 0, read);
                }
                return output.ToArray();
            }
        }
    }
}
