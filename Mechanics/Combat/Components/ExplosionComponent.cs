using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Combat.Components
{
    /// <summary>
    /// Visual explosion that expands and fades.
    /// </summary>
    public struct ExplosionComponent
    {
        public Microsoft.Xna.Framework.Vector2 Center;
        public float TimeRemaining;
        public float Duration;
        public float MaxRadius;
        public Color Color;

        public ExplosionComponent(Microsoft.Xna.Framework.Vector2 center, float duration, float maxRadius, Color color)
        {
            Center = center;
            Duration = duration;
            TimeRemaining = duration;
            MaxRadius = maxRadius;
            Color = color;
        }
    }
}
