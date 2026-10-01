using System;
using System.Collections.Generic;

namespace WeldStudio.Core
{
    /// <summary>
    /// User customisation of one worn item: a colour per colour zone (shirt collar, hair roots, hair tips,
    /// streaks...) and a value per parameter (hair length, volume...). Ids come from the item's definition;
    /// unknown ids are kept so presets survive content updates. Immutable.
    /// </summary>
    public sealed class ItemAppearance : IEquatable<ItemAppearance>
    {
        public static readonly ItemAppearance Empty =
            new ItemAppearance(new Dictionary<string, ColorRgba>(), new Dictionary<string, float>());

        private readonly Dictionary<string, ColorRgba> colors;
        private readonly Dictionary<string, float> parameters;

        private ItemAppearance(Dictionary<string, ColorRgba> colors, Dictionary<string, float> parameters)
        {
            this.colors = colors;
            this.parameters = parameters;
        }

        public static ItemAppearance Create(IEnumerable<KeyValuePair<string, ColorRgba>> colors,
            IEnumerable<KeyValuePair<string, float>> parameters)
        {
            var colorCopy = new Dictionary<string, ColorRgba>(StringComparer.Ordinal);
            if (colors != null)
            {
                foreach (KeyValuePair<string, ColorRgba> pair in colors)
                {
                    if (!string.IsNullOrEmpty(pair.Key)) colorCopy[pair.Key] = pair.Value;
                }
            }

            var parameterCopy = new Dictionary<string, float>(StringComparer.Ordinal);
            if (parameters != null)
            {
                foreach (KeyValuePair<string, float> pair in parameters)
                {
                    if (!string.IsNullOrEmpty(pair.Key) && IsFinite(pair.Value)) parameterCopy[pair.Key] = pair.Value;
                }
            }
            return colorCopy.Count == 0 && parameterCopy.Count == 0 ? Empty : new ItemAppearance(colorCopy, parameterCopy);
        }

        /// <summary>Explicit colours by zone id. Zones absent here use the definition's default colour.</summary>
        public IReadOnlyDictionary<string, ColorRgba> Colors => colors;

        /// <summary>Explicit values by parameter id. Parameters absent here use the definition's default.</summary>
        public IReadOnlyDictionary<string, float> Parameters => parameters;

        public bool IsEmpty => colors.Count == 0 && parameters.Count == 0;

        public bool TryGetColor(string zoneId, out ColorRgba color) => colors.TryGetValue(zoneId, out color);
        public bool TryGetParameter(string parameterId, out float value) => parameters.TryGetValue(parameterId, out value);

        /// <param name="color">New colour, or null to return the zone to its default.</param>
        public ItemAppearance WithColor(string zoneId, ColorRgba? color)
        {
            if (string.IsNullOrEmpty(zoneId)) throw new ArgumentException("Zone id is required.", nameof(zoneId));
            var copy = new Dictionary<string, ColorRgba>(colors, StringComparer.Ordinal);
            if (color.HasValue) copy[zoneId] = color.Value;
            else copy.Remove(zoneId);
            return Create(copy, parameters);
        }

        /// <param name="value">New value, or null to return the parameter to its default.</param>
        public ItemAppearance WithParameter(string parameterId, float? value)
        {
            if (string.IsNullOrEmpty(parameterId)) throw new ArgumentException("Parameter id is required.", nameof(parameterId));
            if (value.HasValue && !IsFinite(value.Value))
                throw new ArgumentOutOfRangeException(nameof(value), value, "Parameter values must be finite.");

            var copy = new Dictionary<string, float>(parameters, StringComparer.Ordinal);
            if (value.HasValue) copy[parameterId] = value.Value;
            else copy.Remove(parameterId);
            return Create(colors, copy);
        }

        public bool Equals(ItemAppearance other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null || other.colors.Count != colors.Count || other.parameters.Count != parameters.Count) return false;

            foreach (KeyValuePair<string, ColorRgba> pair in colors)
            {
                if (!other.colors.TryGetValue(pair.Key, out ColorRgba value) || value != pair.Value) return false;
            }
            foreach (KeyValuePair<string, float> pair in parameters)
            {
                if (!other.parameters.TryGetValue(pair.Key, out float value) || value != pair.Value) return false;
            }
            return true;
        }

        public override bool Equals(object obj) => Equals(obj as ItemAppearance);
        public override int GetHashCode() => colors.Count * 397 ^ parameters.Count;

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
