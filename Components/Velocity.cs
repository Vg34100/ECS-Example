// Components/Velocity.cs
using Microsoft.Xna.Framework;

namespace ECS_Example.Components
{
    public struct Velocity
    {
        public Vector2 Value;

        public Velocity(float x, float y)
        {
            Value = new Vector2(x, y);
        }
    }
}