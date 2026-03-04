using System;

namespace ECS_Base.Mechanics.Tweening.Data
{
    /// <summary>
    /// Easing function types for tweens
    /// </summary>
    public enum EasingType
    {
        Linear,
        EaseInQuad,
        EaseOutQuad,
        EaseInOutQuad,
        EaseInCubic,
        EaseOutCubic,
        EaseInOutCubic,
        EaseInElastic,
        EaseOutElastic,
        EaseOutBounce
    }

    /// <summary>
    /// Easing functions for smooth animations
    /// </summary>
    public static class Easing
    {
        public static float Apply(float t, EasingType type)
        {
            return type switch
            {
                EasingType.Linear => t,
                EasingType.EaseInQuad => t * t,
                EasingType.EaseOutQuad => t * (2 - t),
                EasingType.EaseInOutQuad => t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t,
                EasingType.EaseInCubic => t * t * t,
                EasingType.EaseOutCubic => (t - 1) * (t - 1) * (t - 1) + 1,
                EasingType.EaseInOutCubic => t < 0.5f ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1,
                EasingType.EaseInElastic => t == 0 || t == 1 ? t : -(float)Math.Pow(2, 10 * (t - 1)) * (float)Math.Sin((t - 1.1f) * 5 * Math.PI),
                EasingType.EaseOutElastic => t == 0 || t == 1 ? t : (float)Math.Pow(2, -10 * t) * (float)Math.Sin((t - 0.1f) * 5 * Math.PI) + 1,
                EasingType.EaseOutBounce => EaseOutBounce(t),
                _ => t
            };
        }

        private static float EaseOutBounce(float t)
        {
            if (t < 1 / 2.75f)
            {
                return 7.5625f * t * t;
            }
            else if (t < 2 / 2.75f)
            {
                t -= 1.5f / 2.75f;
                return 7.5625f * t * t + 0.75f;
            }
            else if (t < 2.5f / 2.75f)
            {
                t -= 2.25f / 2.75f;
                return 7.5625f * t * t + 0.9375f;
            }
            else
            {
                t -= 2.625f / 2.75f;
                return 7.5625f * t * t + 0.984375f;
            }
        }
    }
}
