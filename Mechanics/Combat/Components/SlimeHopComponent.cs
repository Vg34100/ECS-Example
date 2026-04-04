using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Combat.Components
{
    /// <summary>
    /// Makes an enemy move in short hops instead of continuous chasing.
    /// </summary>
    public struct SlimeHopComponent
    {
        public float HopInterval;
        public float HopDuration;
        public float TimeUntilNextHop;
        public float HopTimeRemaining;
        public float HopSpeed;
        public Vector2 HopDirection;

        public SlimeHopComponent(float hopInterval = 0.55f, float hopDuration = 0.16f, float hopSpeed = 72f)
        {
            HopInterval = hopInterval;
            HopDuration = hopDuration;
            TimeUntilNextHop = 0f;
            HopTimeRemaining = 0f;
            HopSpeed = hopSpeed;
            HopDirection = Vector2.Zero;
        }
    }
}
