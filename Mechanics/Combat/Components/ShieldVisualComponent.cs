using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Combat.Components
{
    /// <summary>
    /// Visualizes shield block area.
    /// </summary>
    public struct ShieldVisualComponent
    {
        public int TargetEntityId;
        public Vector2 Size;
        public float Distance;
        public Color Color;

        public ShieldVisualComponent(int targetEntityId, Vector2 size, float distance, Color color)
        {
            TargetEntityId = targetEntityId;
            Size = size;
            Distance = distance;
            Color = color;
        }
    }
}
