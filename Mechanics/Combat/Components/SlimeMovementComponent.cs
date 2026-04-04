using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Combat.Components
{
    /// <summary>
    /// Makes an enemy move in short bursts with pauses between hops.
    /// </summary>
    public struct SlimeMovementComponent
    {
        public float HopDuration;
        public float PauseDuration;
        public float HopTimeRemaining;
        public float PauseTimeRemaining;
        public Vector2 HopDirection;
        public float LungeSpeedMultiplier;

        public SlimeMovementComponent(float hopDuration = 0.20f, float pauseDuration = 0.35f, float lungeSpeedMultiplier = 3.0f)
        {
            HopDuration = hopDuration;
            PauseDuration = pauseDuration;
            HopTimeRemaining = 0f;
            PauseTimeRemaining = 0f;
            HopDirection = Vector2.Zero;
            LungeSpeedMultiplier = lungeSpeedMultiplier;
        }
    }
}
