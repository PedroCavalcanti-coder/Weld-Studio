using System;

namespace WeldStudio.Core
{
    /// <summary>Descriptive data about a preset; not used to rebuild the character.</summary>
    [Serializable]
    public sealed class PresetMetadata
    {
        public string Name { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
