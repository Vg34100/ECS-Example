using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Particles.Components
{
    /// <summary>
    /// Individual particle component
    /// </summary>
    public struct ParticleComponent
    {
        /// <summary>Particle velocity</summary>
        public Vector2 Velocity;

        /// <summary>Particle acceleration (gravity, wind, etc.)</summary>
        public Vector2 Acceleration;

        /// <summary>Particle color</summary>
        public Color Color;

        /// <summary>Initial color (for fade effects)</summary>
        public Color InitialColor;

        /// <summary>Particle scale</summary>
        public float Scale;

        /// <summary>Initial scale (for growth/shrink effects)</summary>
        public float InitialScale;

        /// <summary>Rotation in radians</summary>
        public float Rotation;

        /// <summary>Rotation speed in radians per second</summary>
        public float RotationSpeed;

        /// <summary>Total lifetime in seconds</summary>
        public float Lifetime;

        /// <summary>Time alive in seconds</summary>
        public float TimeAlive;

        /// <summary>Is this particle dead?</summary>
        public readonly bool IsDead => TimeAlive >= Lifetime;

        /// <summary>Lifetime progress (0.0 to 1.0)</summary>
        public readonly float LifetimeProgress => Lifetime > 0 ? MathHelper.Clamp(TimeAlive / Lifetime, 0f, 1f) : 1f;

        public ParticleComponent(Vector2 velocity, Color color, float scale, float lifetime)
        {
            Velocity = velocity;
            Acceleration = Vector2.Zero;
            Color = color;
            InitialColor = color;
            Scale = scale;
            InitialScale = scale;
            Rotation = 0f;
            RotationSpeed = 0f;
            Lifetime = lifetime;
            TimeAlive = 0f;
        }
    }
}
