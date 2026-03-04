using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Movement.Components
{
    /// <summary>
    /// Stores last non-zero facing direction.
    /// </summary>
    public struct FacingComponent
    {
        public Vector2 Direction;

        public FacingComponent(Vector2 direction)
        {
            Direction = direction;
        }
    }
}
