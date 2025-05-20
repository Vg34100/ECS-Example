// Components/Position.cs
using Microsoft.Xna.Framework;

namespace ECS_Example.Components
{
    public struct Position
    {
        public Vector2 Value;

        public Position(float x, float y)
        {
            Value = new Vector2(x, y);
        }
    }
}