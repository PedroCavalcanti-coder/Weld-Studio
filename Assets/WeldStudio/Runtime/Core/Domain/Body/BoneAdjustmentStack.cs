using System;
using System.Collections.Generic;

namespace WeldStudio.Core
{
    /// <summary>
    /// Bookkeeping of every bone adjustment by bone and by source (modifier id), so that, for example, "arm
    /// length" and "arm thickness" can both scale the same bone without overwriting each other. The character
    /// rig applies <see cref="GetCombined"/> on top of each bone's rest pose.
    /// </summary>
    public sealed class BoneAdjustmentStack
    {
        private readonly Dictionary<string, Dictionary<string, BoneAdjustment>> byBone =
            new Dictionary<string, Dictionary<string, BoneAdjustment>>(StringComparer.Ordinal);

        /// <summary>Bones that currently have at least one adjustment.</summary>
        public IEnumerable<string> AdjustedBones => byBone.Keys;

        /// <summary>Sets the contribution of <paramref name="sourceId"/> to a bone.</summary>
        public void Set(string boneName, string sourceId, BoneAdjustment adjustment)
        {
            if (string.IsNullOrEmpty(boneName)) throw new ArgumentException("Bone name is required.", nameof(boneName));
            if (string.IsNullOrEmpty(sourceId)) throw new ArgumentException("Source id is required.", nameof(sourceId));

            if (!byBone.TryGetValue(boneName, out Dictionary<string, BoneAdjustment> sources))
            {
                sources = new Dictionary<string, BoneAdjustment>(StringComparer.Ordinal);
                byBone[boneName] = sources;
            }
            sources[sourceId] = adjustment;
        }

        /// <summary>Removes every contribution of <paramref name="sourceId"/>.</summary>
        /// <returns>The bones whose combined adjustment changed.</returns>
        public IReadOnlyList<string> RemoveSource(string sourceId)
        {
            var changed = new List<string>();
            var emptied = new List<string>();
            foreach (KeyValuePair<string, Dictionary<string, BoneAdjustment>> pair in byBone)
            {
                if (!pair.Value.Remove(sourceId)) continue;
                changed.Add(pair.Key);
                if (pair.Value.Count == 0) emptied.Add(pair.Key);
            }
            foreach (string bone in emptied) byBone.Remove(bone);
            return changed;
        }

        /// <summary>Product of all scales and sum of all offsets applied to the bone; identity when none.</summary>
        public BoneAdjustment GetCombined(string boneName)
        {
            BoneAdjustment result = BoneAdjustment.Identity;
            if (byBone.TryGetValue(boneName, out Dictionary<string, BoneAdjustment> sources))
            {
                foreach (BoneAdjustment adjustment in sources.Values) result = result.Combine(adjustment);
            }
            return result;
        }

        public void Clear() => byBone.Clear();
    }
}
