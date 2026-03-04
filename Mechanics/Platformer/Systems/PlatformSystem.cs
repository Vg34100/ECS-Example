using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Platformer.Components;
using ECS_Base.Mechanics.Movement.Components;
using Microsoft.Xna.Framework;
using System;

namespace ECS_Base.Mechanics.Platformer.Systems
{
    /// <summary>
    /// System that handles platform movement and rider interactions
    /// </summary>
    public class PlatformSystem
    {
        /// <summary>
        /// Update all platforms
        /// </summary>
        public void Update(World world, float deltaTime)
        {
            UpdateMovingPlatforms(world, deltaTime);
            UpdateRiders(world);
        }

        /// <summary>
        /// Update moving platform positions
        /// </summary>
        private void UpdateMovingPlatforms(World world, float deltaTime)
        {
            foreach (var entity in world.Query<MovingPlatformComponent, PositionComponent>())
            {
                if (!world.TryGetComponent<MovingPlatformComponent>(entity, out var moving))
                    continue;

                if (!world.TryGetComponent<PositionComponent>(entity, out var position))
                    continue;

                moving.PreviousPosition = position.Value;

                switch (moving.MovementType)
                {
                    case PlatformMovementType.Linear:
                        UpdateLinearPlatform(ref moving, ref position, deltaTime);
                        break;

                    case PlatformMovementType.Circular:
                        UpdateCircularPlatform(ref moving, ref position, deltaTime);
                        break;

                    case PlatformMovementType.Waypoint:
                        UpdateWaypointPlatform(ref moving, ref position, deltaTime);
                        break;
                }

                // Calculate velocity based on position change
                moving.Velocity = (position.Value - moving.PreviousPosition) / deltaTime;

                world.AddComponent(entity, moving);
                world.AddComponent(entity, position);
            }
        }

        /// <summary>
        /// Update riders on platforms
        /// </summary>
        private void UpdateRiders(World world)
        {
            // Clear all rider lists
            foreach (var platformEntity in world.Query<PlatformComponent>())
            {
                if (!world.TryGetComponent<PlatformComponent>(platformEntity, out var platform))
                    continue;

                platform.Riders.Clear();
                world.AddComponent(platformEntity, platform);
            }

            // Check all entities with physics against all platforms
            foreach (var entity in world.Query<PositionComponent, PlatformerPhysicsComponent>())
            {
                if (!world.TryGetComponent<PositionComponent>(entity, out var entityPos))
                    continue;

                if (!world.TryGetComponent<PlatformerPhysicsComponent>(entity, out var physics))
                    continue;

                // Find platform entity is standing on
                foreach (var platformEntity in world.Query<PlatformComponent, PositionComponent>())
                {
                    if (!world.TryGetComponent<PlatformComponent>(platformEntity, out var platform))
                        continue;

                    if (!world.TryGetComponent<PositionComponent>(platformEntity, out var platformPos))
                        continue;

                    if (IsOnPlatform(entityPos.Value, physics, platformPos.Value, platform))
                    {
                        // Add to riders
                        platform.Riders.Add(entity);
                        world.AddComponent(platformEntity, platform);

                        // Transfer platform velocity to rider
                        if (world.TryGetComponent<MovingPlatformComponent>(platformEntity, out var moving))
                        {
                            if (world.TryGetComponent<VelocityComponent>(entity, out var velocity))
                            {
                                // Transfer both X and Y velocity so riders stick to platforms
                                velocity.Value.X += moving.Velocity.X;
                                velocity.Value.Y += moving.Velocity.Y;
                                world.AddComponent(entity, velocity);
                            }
                        }

                        break; // Only one platform per entity
                    }
                }
            }
        }

        /// <summary>
        /// Check if entity is on platform
        /// </summary>
        private bool IsOnPlatform(Vector2 entityPos, PlatformerPhysicsComponent physics, Vector2 platformPos, PlatformComponent platform)
        {
            // Must be grounded to be on platform
            if (!physics.IsGrounded)
                return false;

            // Check if entity is within platform bounds horizontally
            float halfWidth = platform.Width * 0.5f;
            if (entityPos.X < platformPos.X - halfWidth || entityPos.X > platformPos.X + halfWidth)
                return false;

            // Check if entity is on top of platform (within threshold)
            float platformTop = platformPos.Y - platform.Height * 0.5f;
            float distanceFromTop = Math.Abs(entityPos.Y - platformTop);

            return distanceFromTop <= platform.SurfaceThreshold;
        }

        /// <summary>
        /// Update linear moving platform
        /// </summary>
        private void UpdateLinearPlatform(ref MovingPlatformComponent moving, ref PositionComponent position, float deltaTime)
        {
            Vector2 target = moving.Direction > 0 ? moving.EndPosition : moving.StartPosition;
            Vector2 direction = target - position.Value;
            float distance = direction.Length();

            if (distance < 0.1f)
            {
                // Reached target, reverse direction
                position.Value = target;
                moving.Direction *= -1;
            }
            else
            {
                // Move towards target
                direction.Normalize();
                float moveDistance = moving.Speed * deltaTime;

                if (moveDistance >= distance)
                {
                    position.Value = target;
                    moving.Direction *= -1;
                }
                else
                {
                    position.Value += direction * moveDistance;
                }
            }
        }

        /// <summary>
        /// Update circular moving platform
        /// </summary>
        private void UpdateCircularPlatform(ref MovingPlatformComponent moving, ref PositionComponent position, float deltaTime)
        {
            // Update angle based on speed (speed is angular velocity in radians/second)
            moving.Angle += moving.Speed * moving.Direction * deltaTime;

            // Keep angle in 0-2π range
            if (moving.Angle > MathHelper.TwoPi)
                moving.Angle -= MathHelper.TwoPi;
            else if (moving.Angle < 0)
                moving.Angle += MathHelper.TwoPi;

            // Calculate new position
            position.Value = moving.CenterPoint + new Vector2(
                (float)Math.Cos(moving.Angle) * moving.Radius,
                (float)Math.Sin(moving.Angle) * moving.Radius
            );
        }

        /// <summary>
        /// Update waypoint moving platform
        /// </summary>
        private void UpdateWaypointPlatform(ref MovingPlatformComponent moving, ref PositionComponent position, float deltaTime)
        {
            if (moving.Waypoints.Count < 2)
                return;

            // Handle waiting at waypoint
            if (moving.CurrentWaitTime > 0)
            {
                moving.CurrentWaitTime -= deltaTime;
                if (moving.CurrentWaitTime <= 0)
                {
                    moving.CurrentWaitTime = 0;
                }
                return;
            }

            // Calculate next waypoint index
            int nextIndex = moving.CurrentWaypointIndex + moving.Direction;

            // Check if we need to reverse direction
            if (nextIndex >= moving.Waypoints.Count || nextIndex < 0)
            {
                moving.Direction *= -1;
                nextIndex = moving.CurrentWaypointIndex + moving.Direction;
            }

            // Get target waypoint
            Vector2 target = moving.Waypoints[nextIndex];
            Vector2 direction = target - position.Value;
            float distance = direction.Length();

            if (distance < 0.1f)
            {
                // Already at target, move to next
                position.Value = target;
                moving.CurrentWaypointIndex = nextIndex;
                moving.CurrentWaitTime = moving.WaypointWaitTime;
            }
            else
            {
                // Move towards waypoint
                direction.Normalize();
                float moveDistance = moving.Speed * deltaTime;

                if (moveDistance >= distance)
                {
                    // Reached waypoint
                    position.Value = target;
                    moving.CurrentWaypointIndex = nextIndex;
                    moving.CurrentWaitTime = moving.WaypointWaitTime;
                }
                else
                {
                    position.Value += direction * moveDistance;
                }
            }
        }

        /// <summary>
        /// Check if entity can pass through one-way platform
        /// </summary>
        public bool CanPassThrough(Vector2 entityPos, Vector2 entityVelocity, Vector2 platformPos, PlatformComponent platform)
        {
            if (!platform.OneWay)
                return false;

            // Can pass through if moving upward or if entity is below platform
            float platformTop = platformPos.Y - platform.Height * 0.5f;
            return entityVelocity.Y < 0 || entityPos.Y > platformTop;
        }

        /// <summary>
        /// Get all riders on a platform
        /// </summary>
        public Entity[] GetRiders(World world, Entity platformEntity)
        {
            if (!world.TryGetComponent<PlatformComponent>(platformEntity, out var platform))
                return Array.Empty<Entity>();

            return platform.Riders.ToArray();
        }
    }
}
