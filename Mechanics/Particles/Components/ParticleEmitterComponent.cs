using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Particles.Components
{
    /// <summary>
    /// Particle emitter configuration
    /// </summary>
    public struct ParticleEmitterComponent
    {
        /// <summary>Particles spawned per second</summary>
        public float EmissionRate;

        /// <summary>Time accumulator for emission</summary>
        public float EmissionAccumulator;

        /// <summary>Particle lifetime range (min, max)</summary>
        public Vector2 LifetimeRange;

        /// <summary>Particle speed range (min, max)</summary>
        public Vector2 SpeedRange;

        /// <summary>Emission angle in radians (0 = right, PI/2 = up)</summary>
        public float EmissionAngle;

        /// <summary>Emission spread in radians</summary>
        public float EmissionSpread;

        /// <summary>Particle color</summary>
        public Color ParticleColor;

        /// <summary>Particle scale range (min, max)</summary>
        public Vector2 ScaleRange;

        /// <summary>Gravity acceleration applied to particles</summary>
        public Vector2 Gravity;

        /// <summary>Maximum particles this emitter can have alive</summary>
        public int MaxParticles;

        /// <summary>Current particle count</summary>
        public int ActiveParticles;

        /// <summary>Is this emitter active?</summary>
        public bool IsActive;

        /// <summary>Duration of emission (0 = infinite)</summary>
        public float Duration;

        /// <summary>Time emitter has been active</summary>
        public float TimeActive;

        /// <summary>Has emitter finished?</summary>
        public readonly bool IsFinished => Duration > 0 && TimeActive >= Duration;

        public ParticleEmitterComponent(float emissionRate, Color color, int maxParticles = 100)
        {
            EmissionRate = emissionRate;
            EmissionAccumulator = 0f;
            LifetimeRange = new Vector2(0.5f, 2.0f);
            SpeedRange = new Vector2(50f, 150f);
            EmissionAngle = 0f;
            EmissionSpread = MathHelper.TwoPi;
            ParticleColor = color;
            ScaleRange = new Vector2(0.5f, 1.5f);
            Gravity = new Vector2(0, 100f);
            MaxParticles = maxParticles;
            ActiveParticles = 0;
            IsActive = true;
            Duration = 0f;
            TimeActive = 0f;
        }
    }
}
