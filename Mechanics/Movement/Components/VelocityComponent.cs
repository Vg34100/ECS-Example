using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Movement.Components
{
    public struct VelocityComponent
    {
        public Vector2 Value;

        public VelocityComponent(float x, float y)
        {
            Value = new Vector2(x, y);
        }
    }
}
