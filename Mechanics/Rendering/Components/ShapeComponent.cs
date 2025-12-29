using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Rendering.Components
{
    public struct ShapeComponent
    {
        public enum ShapeType { Rectangle, Circle }

        public ShapeType Type;
        public Color Color;
        public Vector2 Size; // Width/Height for rectangle, Radius for circle (X component)

        public ShapeComponent(ShapeType type, Color color, Vector2 size)
        {
            Type = type;
            Color = color;
            Size = size;
        }
    }
}
