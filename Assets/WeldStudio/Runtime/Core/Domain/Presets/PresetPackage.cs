using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WeldStudio.Core
{
    /// <summary>
    /// What a preset file contains: the <see cref="CharacterPreset"/> plus named binary attachments (the PNGs
    /// of the paint layers). Attachment names are relative paths such as <c>paint/&lt;layer id&gt;.png</c>.
    /// </summary>
    public sealed class PresetPackage
    {
        // Lower-case relative paths only: no "..", no absolute paths, no backslashes. Presets are shared
        // between users, so names coming from a file must never escape the package.
        private static readonly Regex ValidName = new Regex(@"^[a-z0-9_\-]+(/[a-z0-9_\-]+)*\.[a-z0-9]+$", RegexOptions.CultureInvariant);

        private readonly Dictionary<string, byte[]> attachments = new Dictionary<string, byte[]>(StringComparer.Ordinal);

        public PresetPackage(CharacterPreset preset)
        {
            Preset = preset ?? throw new ArgumentNullException(nameof(preset));
        }

        public CharacterPreset Preset { get; }
        public IReadOnlyDictionary<string, byte[]> Attachments => attachments;

        public static bool IsValidAttachmentName(string name) => name != null && name.Length <= 128 && ValidName.IsMatch(name);

        public void SetAttachment(string name, byte[] data)
        {
            if (!IsValidAttachmentName(name)) throw new ArgumentException($"Invalid attachment name '{name}'.", nameof(name));
            attachments[name] = data ?? throw new ArgumentNullException(nameof(data));
        }

        public bool TryGetAttachment(string name, out byte[] data) => attachments.TryGetValue(name, out data);
    }
}
