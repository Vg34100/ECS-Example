using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Particles.Components;
using ECS_Base.Mechanics.Movement.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ECS_Base.Mechanics.Particles.Systems
{
    /// <summary>
    /// System that manages particles and emitters
    /// </summary>
    public class ParticleSystem
    {
        private readonly Random _random = new Random();
        private Texture2D _pixelTexture;

        public ParticleSystem(GraphicsDevice graphicsDevice = null)
        {
            if (graphicsDevice != null)
            {
                _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
                _pixelTexture.SetData(new[] { Color.White });
            }
        }

        /// <summary>
        /// Update all particles and emitters
        /// </summary>
        public void Update(World world, float deltaTime)
        {
            UpdateParticles(world, deltaTime);
            UpdateEmitters(world, deltaTime);
        }

        private void UpdateParticles(World world, float deltaTime)
        {
            var particlesToRemove = new List<Entity>();

            foreach (var entity in world.Query<ParticleComponent, PositionComponent>())
            {
                if (!world.TryGetComponent<ParticleComponent>(entity, out var particle))
                    continue;

                if (!world.TryGetComponent<PositionComponent>(entity, out var position))
                    continue;

                // Update lifetime
                particle.TimeAlive += deltaTime;

                if (particle.IsDead)
                {
                    particlesToRemove.Add(entity);
                    continue;
                }

                // Update physics
                particle.Velocity += particle.Acceleration * deltaTime;
                position.Value += particle.Velocity * deltaTime;

                // Update rotation
                particle.Rotation += particle.RotationSpeed * deltaTime;

                // Fade out over lifetime
                float progress = particle.LifetimeProgress;
                particle.Color = Color.Lerp(particle.InitialColor, Color.Transparent, progress);

                // Scale down over lifetime
                particle.Scale = MathHelper.Lerp(particle.InitialScale, 0f, progress);

                // Write back
                world.AddComponent(entity, particle);
                world.AddComponent(entity, position);
            }

            // Remove dead particles
            foreach (var entity in particlesToRemove)
            {
                // Decrement emitter counter if particle was tracked
                DecrementEmitterParticleCount(world, entity);
                world.RemoveEntity(entity);
            }
        }

        private void UpdateEmitters(World world, float deltaTime)
        {
            var emittersToProcess = new List<(Entity, ParticleEmitterComponent, Vector2)>();

            // Collect emitters first
            foreach (var entity in world.Query<ParticleEmitterComponent, PositionComponent>())
            {
                if (!world.TryGetComponent<ParticleEmitterComponent>(entity, out var emitter))
                    continue;

                if (!emitter.IsActive)
                    continue;

                if (!world.TryGetComponent<PositionComponent>(entity, out var position))
                    continue;

                // Update duration
                emitter.TimeActive += deltaTime;

                if (emitter.IsFinished)
                {
                    emitter.IsActive = false;
                    world.AddComponent(entity, emitter);
                    continue;
                }

                // Emit particles
                emitter.EmissionAccumulator += emitter.EmissionRate * deltaTime;

                emittersToProcess.Add((entity, emitter, position.Value));
            }

            // Now emit particles (outside the query loop)
            foreach (var (entity, emitter, position) in emittersToProcess)
            {
                var updatedEmitter = emitter;

                while (updatedEmitter.EmissionAccumulator >= 1f && updatedEmitter.ActiveParticles < updatedEmitter.MaxParticles)
                {
                    EmitParticle(world, entity, updatedEmitter, position);
                    updatedEmitter.EmissionAccumulator -= 1f;
                    updatedEmitter.ActiveParticles++;
                }

                world.AddComponent(entity, updatedEmitter);
            }
        }

        private void EmitParticle(World world, Entity emitterEntity, ParticleEmitterComponent emitter, Vector2 emitterPosition)
        {
            var particleEntity = world.CreateEntity();

            // Random angle within spread
            float angle = emitter.EmissionAngle + ((float)_random.NextDouble() - 0.5f) * emitter.EmissionSpread;
            float speed = MathHelper.Lerp(emitter.SpeedRange.X, emitter.SpeedRange.Y, (float)_random.NextDouble());
            Vector2 velocity = new Vector2(
                (float)Math.Cos(angle) * speed,
                (float)Math.Sin(angle) * speed
            );

            float lifetime = MathHelper.Lerp(emitter.LifetimeRange.X, emitter.LifetimeRange.Y, (float)_random.NextDouble());
            float scale = MathHelper.Lerp(emitter.ScaleRange.X, emitter.ScaleRange.Y, (float)_random.NextDouble());

            var particle = new ParticleComponent(velocity, emitter.ParticleColor, scale, lifetime);
            particle.Acceleration = emitter.Gravity;

            world.AddComponent(particleEntity, particle);
            world.AddComponent(particleEntity, new PositionComponent { Value = emitterPosition });

            // Store emitter reference for cleanup (we'll use a marker component for this)
            // For now, we just track the count
        }

        private void DecrementEmitterParticleCount(World world, Entity particleEntity)
        {
            // In a full implementation, we'd track which emitter spawned each particle
            // For now, we just decrement the first emitter we find
            foreach (var entity in world.Query<ParticleEmitterComponent>())
            {
                if (world.TryGetComponent<ParticleEmitterComponent>(entity, out var emitter))
                {
                    if (emitter.ActiveParticles > 0)
                    {
                        emitter.ActiveParticles--;
                        world.AddComponent(entity, emitter);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Draw all particles
        /// </summary>
        public void Draw(World world, SpriteBatch spriteBatch)
        {
            if (_pixelTexture == null)
                return;

            foreach (var entity in world.Query<ParticleComponent, PositionComponent>())
            {
                if (!world.TryGetComponent<ParticleComponent>(entity, out var particle))
                    continue;

                if (!world.TryGetComponent<PositionComponent>(entity, out var position))
                    continue;

                spriteBatch.Draw(
                    _pixelTexture,
                    position.Value,
                    null,
                    particle.Color,
                    particle.Rotation,
                    new Vector2(0.5f, 0.5f),
                    particle.Scale * 4f, // Make particles visible
                    SpriteEffects.None,
                    0f
                );
            }
        }

        /// <summary>
        /// Create a burst of particles
        /// </summary>
        public void CreateBurst(World world, Vector2 position, int count, Color color, float speed = 100f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = ((float)i / count) * MathHelper.TwoPi;
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;

                var entity = world.CreateEntity();
                var particle = new ParticleComponent(velocity, color, 1f, 1f);
                particle.Acceleration = new Vector2(0, 100f); // Gravity

                world.AddComponent(entity, particle);
                world.AddComponent(entity, new PositionComponent { Value = position });
            }
        }
    }
}
