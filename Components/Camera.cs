// Components/Camera.cs
using Microsoft.Xna.Framework;

namespace ECS_Example.Components
{
    public struct Camera
    {
        public Vector2 Position;
        public Vector2 TargetPosition;
        public float LagFactor; // Higher values = more lag (0.1f = very responsive, 0.9f = very laggy)
        public Vector2 Offset;  // Offset from target (to center player on screen)
        public bool IsActive;

        public Camera(Vector2 initialPosition, float lagFactor = 0.1f, Vector2 offset = default)
        {
            Position = initialPosition;
            TargetPosition = initialPosition;
            LagFactor = lagFactor;
            Offset = offset;
            IsActive = true;
        }
    }
}