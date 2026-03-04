using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Movement.Components
{
    /// <summary>
    /// Simple vertical bobbing.
    /// </summary>
    public struct FloatingComponent
    {
        public float Amplitude;
        public float Speed;
        public float Time;
        public Vector2 BasePosition;

        public FloatingComponent(Vector2 basePosition, float amplitude = 3f, float speed = 2f)
        {
            BasePosition = basePosition;
            Amplitude = amplitude;
            Speed = speed;
            Time = 0f;
        }
    }
}
