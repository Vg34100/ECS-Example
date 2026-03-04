using ECS_Base.Mechanics.Tweening.Data;
using System;

namespace ECS_Base.Mechanics.Tweening.Components
{
    /// <summary>
    /// Component for tweening a value over time
    /// </summary>
    public struct TweenComponent
    {
        /// <summary>Type of value being tweened</summary>
        public TweenTarget Target;

        /// <summary>Start value</summary>
        public float StartValue;

        /// <summary>End value</summary>
        public float EndValue;

        /// <summary>Duration in seconds</summary>
        public float Duration;

        /// <summary>Time elapsed</summary>
        public float Elapsed;

        /// <summary>Easing function</summary>
        public EasingType Easing;

        /// <summary>Callback when tween completes</summary>
        public Action OnComplete;

        /// <summary>Should this tween loop?</summary>
        public bool Loop;

        /// <summary>Should this tween ping-pong (reverse direction)?</summary>
        public bool PingPong;

        /// <summary>Is tween currently going in reverse?</summary>
        public bool IsReversing;

        /// <summary>Remove component when complete?</summary>
        public bool RemoveOnComplete;

        /// <summary>Is tween complete?</summary>
        public readonly bool IsComplete => !Loop && !PingPong && Elapsed >= Duration;

        /// <summary>Progress (0.0 to 1.0)</summary>
        public readonly float Progress => Duration > 0 ? Math.Clamp(Elapsed / Duration, 0f, 1f) : 1f;

        /// <summary>Current interpolated value</summary>
        public readonly float CurrentValue
        {
            get
            {
                float t = Progress;
                float easedT = Data.Easing.Apply(t, Easing);

                if (IsReversing)
                    return StartValue + (EndValue - StartValue) * (1f - easedT);
                else
                    return StartValue + (EndValue - StartValue) * easedT;
            }
        }

        public TweenComponent(TweenTarget target, float start, float end, float duration, EasingType easing = EasingType.Linear)
        {
            Target = target;
            StartValue = start;
            EndValue = end;
            Duration = duration;
            Elapsed = 0f;
            Easing = easing;
            OnComplete = null;
            Loop = false;
            PingPong = false;
            IsReversing = false;
            RemoveOnComplete = true;
        }
    }

    /// <summary>
    /// What property is being tweened
    /// </summary>
    public enum TweenTarget
    {
        PositionX,
        PositionY,
        ScaleX,
        ScaleY,
        Rotation,
        Alpha,
        Custom
    }
}
