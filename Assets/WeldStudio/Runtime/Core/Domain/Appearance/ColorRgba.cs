using System;
using System.Globalization;

namespace WeldStudio.Core
{
    /// <summary>
    /// 8-bit sRGB colour used by the domain and presets. Independent from UnityEngine.Color so it round-trips
    /// exactly through JSON as <c>#RRGGBB</c> or <c>#RRGGBBAA</c>; the rendering layer converts it.
    /// </summary>
    public readonly struct ColorRgba : IEquatable<ColorRgba>
    {
        public ColorRgba(byte r, byte g, byte b, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public byte R { get; }
        public byte G { get; }
        public byte B { get; }
        public byte A { get; }

        /// <summary>Parses <c>#RRGGBB</c> or <c>#RRGGBBAA</c> (the leading # is optional, case-insensitive).</summary>
        public static bool TryParseHex(string hex, out ColorRgba color)
        {
            color = default;
            if (string.IsNullOrWhiteSpace(hex)) return false;

            string digits = hex.Trim();
            if (digits.StartsWith("#", StringComparison.Ordinal)) digits = digits.Substring(1);
            if (digits.Length != 6 && digits.Length != 8) return false;
            if (!uint.TryParse(digits, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out uint value)) return false;

            if (digits.Length == 6) value = (value << 8) | 0xFF;
            color = new ColorRgba((byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value);
            return true;
        }

        public static ColorRgba ParseHex(string hex) =>
            TryParseHex(hex, out ColorRgba color) ? color : throw new FormatException($"'{hex}' is not a #RRGGBB or #RRGGBBAA colour.");

        /// <summary><c>#RRGGBB</c> when opaque, <c>#RRGGBBAA</c> otherwise.</summary>
        public string ToHex() => A == 255 ? $"#{R:X2}{G:X2}{B:X2}" : $"#{R:X2}{G:X2}{B:X2}{A:X2}";

        public bool Equals(ColorRgba other) => R == other.R && G == other.G && B == other.B && A == other.A;
        public override bool Equals(object obj) => obj is ColorRgba other && Equals(other);
        public override int GetHashCode() => (R << 24) | (G << 16) | (B << 8) | A;
        public static bool operator ==(ColorRgba left, ColorRgba right) => left.Equals(right);
        public static bool operator !=(ColorRgba left, ColorRgba right) => !left.Equals(right);
        public override string ToString() => ToHex();
    }
}
