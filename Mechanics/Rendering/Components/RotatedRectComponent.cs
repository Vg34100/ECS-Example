using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Rendering.Components
{
    /// <summary>
    /// Draws a rotated rectangle at a center position.
    /// </summary>
    public struct RotatedRectComponent
    {
        public Vector2 Center;
        public Vector2 Size;
        public float Rotation;
        public Color Color;

        public RotatedRectComponent(Vector2 center, Vector2 size, float rotation, Color color)
        {
            Center = center;
            Size = size;
            Rotation = rotation;
            Color = color;
        }
    }
}
