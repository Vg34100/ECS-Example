using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Movement.Components
{
    public struct PositionComponent
    {
        public Vector2 Value;

        public PositionComponent(float x, float y)
        {
            Value = new Vector2(x, y);
        }
    }
}
