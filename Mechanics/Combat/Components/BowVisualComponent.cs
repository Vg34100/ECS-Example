using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Combat.Components
{
    /// <summary>
    /// Visualizes bow when shooting.
    /// </summary>
    public struct BowVisualComponent
    {
        public int TargetEntityId;
        public Vector2 Size;
        public float Distance;
        public float TimeRemaining;
        public Color Color;

        public BowVisualComponent(int targetEntityId, Vector2 size, float distance, float timeRemaining, Color color)
        {
            TargetEntityId = targetEntityId;
            Size = size;
            Distance = distance;
            TimeRemaining = timeRemaining;
            Color = color;
        }
    }
}
