// Components/Collider.cs
using Microsoft.Xna.Framework;

namespace ECS_Example.Components
{
    public struct Collider
    {
        public enum ColliderType { Static, Dynamic }

        public Rectangle Bounds;
        public ColliderType Type;

        public Collider(Rectangle bounds, ColliderType type)
        {
            Bounds = bounds;
            Type = type;
        }
    }
}