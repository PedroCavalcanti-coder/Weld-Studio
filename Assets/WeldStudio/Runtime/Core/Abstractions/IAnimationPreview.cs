using System;
using System.Threading;
using System.Threading.Tasks;
using WeldStudio.Core.Data;

namespace WeldStudio.Core
{
    /// <summary>
    /// Plays sample animations on the character so the user can check how body edits, clothes and hair deform
    /// in motion. Preview only: nothing here is saved in presets.
    /// </summary>
    public interface IAnimationPreview
    {
        AnimationClipData CurrentClip { get; }
        bool IsPlaying { get; }

        /// <summary>Playback position in [0, 1] of the current clip.</summary>
        float NormalizedTime { get; }

        /// <summary>Playback speed multiplier; 1 is real time.</summary>
        float Speed { get; set; }

        /// <summary>Raised when the clip, play state or position changes through this API.</summary>
        event Action StateChanged;

        /// <summary>Loads <paramref name="clip"/> and plays it, blending from the current pose.</summary>
        Task PlayAsync(AnimationClipData clip, CancellationToken cancellationToken = default);

        void Pause();
        void Resume();

        /// <summary>Jumps to <paramref name="normalizedTime"/> (scrubbing); works while paused.</summary>
        void Seek(float normalizedTime);

        /// <summary>Stops and returns to the default idle.</summary>
        void Stop();
    }
}
