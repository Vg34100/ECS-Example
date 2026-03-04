using System;

namespace ECS_Base.Mechanics.Core
{
    /// <summary>
    /// Timer component for cooldowns, delays, and repeating timers.
    /// Note: Only one timer per entity. For multiple timers, create multiple entities.
    /// </summary>
    public struct TimerComponent
    {
        /// <summary>Optional name to identify this timer</summary>
        public string Name;

        /// <summary>Total duration of the timer in seconds</summary>
        public float Duration;

        /// <summary>Time elapsed since timer started</summary>
        public float Elapsed;

        /// <summary>If true, timer repeats when it reaches duration</summary>
        public bool IsRepeating;

        /// <summary>If true, timer is paused and won't advance</summary>
        public bool IsPaused;

        /// <summary>If true, timer has finished (only relevant for non-repeating timers)</summary>
        public bool IsFinished;

        /// <summary>Callback to invoke when timer completes (reference type is OK in structs)</summary>
        public Action OnComplete;

        /// <summary>If true, component is removed after timer completes (one-shot timer)</summary>
        public bool RemoveOnComplete;

        /// <summary>Gets the remaining time on this timer</summary>
        public readonly float TimeRemaining => Math.Max(0, Duration - Elapsed);

        /// <summary>Gets normalized progress (0.0 to 1.0)</summary>
        public readonly float Progress => Duration > 0 ? Math.Clamp(Elapsed / Duration, 0f, 1f) : 1f;

        public TimerComponent(float duration, bool isRepeating = false, Action onComplete = null)
        {
            Duration = duration;
            Elapsed = 0f;
            IsRepeating = isRepeating;
            OnComplete = onComplete;
            IsPaused = false;
            IsFinished = false;
            RemoveOnComplete = false;
            Name = string.Empty;
        }
    }
}
