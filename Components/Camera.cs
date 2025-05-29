// Components/Camera.cs - Updated Camera component with dampening
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
        public float Zoom;      // Zoom level (1.0f = normal, 2.0f = 2x zoomed in, 0.5f = zoomed out)
        public float DampeningThreshold; // Minimum distance before camera starts moving
        public Vector2 Velocity; // Camera velocity for smooth movement

        public Camera(Vector2 initialPosition, float lagFactor = 0.1f, Vector2 offset = default,
                     float zoom = 2.0f, float dampeningThreshold = 5.0f)
        {
            Position = initialPosition;
            TargetPosition = initialPosition;
            LagFactor = lagFactor;
            Offset = offset;
            IsActive = true;
            Zoom = zoom;
            DampeningThreshold = dampeningThreshold;
            Velocity = Vector2.Zero;
        }
    }
}