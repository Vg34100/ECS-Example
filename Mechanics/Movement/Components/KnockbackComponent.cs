using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Movement.Components
{
    /// <summary>
    /// Temporary knockback velocity that decays over time.
    /// </summary>
    public struct KnockbackComponent
    {
        public Vector2 Velocity;
        public float Decay;

        public KnockbackComponent(Vector2 velocity, float decay = 8f)
        {
            Velocity = velocity;
            Decay = decay;
        }
    }
}
