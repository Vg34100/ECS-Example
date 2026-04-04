using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Collision.Components
{
    public struct ColliderComponent
    {
        public enum ColliderType { Static, Dynamic }

        public Rectangle Bounds;
        public ColliderType Type;

        public ColliderComponent(Rectangle bounds, ColliderType type)
        {
            Bounds = bounds;
            Type = type;
        }
    }
}
