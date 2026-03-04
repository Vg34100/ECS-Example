using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Platformer.Components;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.Collision.Components;
using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Platformer.Systems
{
    /// <summary>
    /// System that handles platformer-specific physics
    /// </summary>
    public class PlatformerPhysicsSystem
    {
        /// <summary>
        /// Update platformer physics
        /// </summary>
        public void Update(World world, float deltaTime)
        {
            foreach (var entity in world.Query<PlatformerPhysicsComponent, VelocityComponent>())
            {
                if (!world.TryGetComponent<PlatformerPhysicsComponent>(entity, out var physics))
                    continue;

                if (!world.TryGetComponent<VelocityComponent>(entity, out var velocity))
                    continue;

                // Sync grounded state from CollisionSystem's GroundedComponent
                if (world.TryGetComponent<GroundedComponent>(entity, out var groundedComp))
                {
                    physics.IsGrounded = groundedComp.IsGrounded;
                }

                // Update grounded state tracking
                physics.WasGrounded = physics.IsGrounded;

                // Update timers
                if (!physics.IsGrounded)
                {
                    physics.TimeSinceGrounded += deltaTime;

                    // Reset IsJumping when falling (allows variable jump to work)
                    if (velocity.Value.Y >= 0)
                    {
                        physics.IsJumping = false;
                    }
                }
                else
                {
                    physics.TimeSinceGrounded = 0f;
                    physics.AirJumpsRemaining = physics.MaxAirJumps;

                    // Reset IsJumping when landing (transition from air to ground)
                    if (!physics.WasGrounded)
                    {
                        physics.IsJumping = false;
                    }
                }

                physics.TimeSinceJumpPressed += deltaTime;

                // Apply gravity
                if (!physics.IsGrounded)
                {
                    velocity.Value.Y += physics.Gravity * deltaTime;

                    // Clamp fall speed
                    if (velocity.Value.Y > physics.MaxFallSpeed)
                    {
                        velocity.Value.Y = physics.MaxFallSpeed;
                    }
                }

                // Auto-jump from buffer
                if (physics.HasJumpBuffer && physics.CanJump && physics.IsGrounded)
                {
                    PerformJump(ref physics, ref velocity);
                }

                world.AddComponent(entity, physics);
                world.AddComponent(entity, velocity);
            }
        }

        /// <summary>
        /// Request a jump
        /// </summary>
        public bool Jump(World world, Entity entity)
        {
            if (!world.TryGetComponent<PlatformerPhysicsComponent>(entity, out var physics))
                return false;

            if (!world.TryGetComponent<VelocityComponent>(entity, out var velocity))
                return false;

            // Buffer the jump input
            physics.TimeSinceJumpPressed = 0f;

            if (physics.CanJump)
            {
                PerformJump(ref physics, ref velocity);
                world.AddComponent(entity, physics);
                world.AddComponent(entity, velocity);
                return true;
            }

            // Save buffered input even if can't jump now
            world.AddComponent(entity, physics);
            return false;
        }

        /// <summary>
        /// Release jump (for variable jump height)
        /// </summary>
        public void ReleaseJump(World world, Entity entity)
        {
            if (!world.TryGetComponent<PlatformerPhysicsComponent>(entity, out var physics))
                return;

            if (!world.TryGetComponent<VelocityComponent>(entity, out var velocity))
                return;

            // If jumping upward and variable jump is enabled, reduce upward velocity
            if (physics.IsJumping && velocity.Value.Y < 0 && physics.VariableJumpHeight)
            {
                velocity.Value.Y *= physics.MinJumpMultiplier;
                world.AddComponent(entity, velocity);
            }
        }

        /// <summary>
        /// Set grounded state
        /// </summary>
        public void SetGrounded(World world, Entity entity, bool grounded)
        {
            if (!world.TryGetComponent<PlatformerPhysicsComponent>(entity, out var physics))
                return;

            physics.IsGrounded = grounded;
            world.AddComponent(entity, physics);
        }

        /// <summary>
        /// Check if entity can jump
        /// </summary>
        public bool CanJump(World world, Entity entity)
        {
            if (!world.TryGetComponent<PlatformerPhysicsComponent>(entity, out var physics))
                return false;

            return physics.CanJump;
        }

        private void PerformJump(ref PlatformerPhysicsComponent physics, ref VelocityComponent velocity)
        {
            // Use air jump if in air
            if (!physics.IsGrounded && physics.TimeSinceGrounded >= physics.CoyoteTime)
            {
                if (physics.AirJumpsRemaining > 0)
                {
                    physics.AirJumpsRemaining--;
                }
                else
                {
                    return; // Can't jump
                }
            }

            velocity.Value.Y = -physics.JumpForce;
            physics.IsJumping = true;
            physics.TimeSinceJumpPressed = 999f; // Clear buffer
        }
    }
}
