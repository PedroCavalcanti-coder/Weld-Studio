using System;

namespace WeldStudio.Core
{
    /// <summary>A file is not a valid preset, or was written by a newer, unsupported version.</summary>
    public sealed class PresetFormatException : Exception
    {
        public PresetFormatException(string message) : base(message)
        {
        }

        public PresetFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
