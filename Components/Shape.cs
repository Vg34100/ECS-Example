// Components/Shape.cs
using Microsoft.Xna.Framework;

namespace ECS_Example.Components
{
    public struct Shape
    {
        public enum ShapeType { Rectangle, Circle }

        public ShapeType Type;
        public Color Color;
        public Vector2 Size; // Width/Height for rectangle, Radius for circle (X component)

        public Shape(ShapeType type, Color color, Vector2 size)
        {
            Type = type;
            Color = color;
            Size = size;
        }
    }
}