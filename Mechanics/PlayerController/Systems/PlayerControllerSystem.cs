using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Input.Components;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.PlayerController.Components;
using ECS_Base.Mechanics.Collision.Components;

namespace ECS_Base.Mechanics.PlayerController.Systems
{
    public class PlayerControllerSystem
    {
        public void Update(World world, float deltaTime)
        {
            foreach (var entity in world.Query<PlayerComponent, InputComponent, VelocityComponent>())
            {
                var player = world.GetComponent<PlayerComponent>(entity);
                var input = world.GetComponent<InputComponent>(entity);
                var velocity = world.GetComponent<VelocityComponent>(entity);

                // Get grounded state and wall touching (if it exists)
                bool isGrounded = false;
                bool touchingLeft = false;
                bool touchingRight = false;
                if (world.TryGetComponent<GroundedComponent>(entity, out var grounded))
                {
                    isGrounded = grounded.IsGrounded;
                    touchingLeft = grounded.TouchingLeft;
                    touchingRight = grounded.TouchingRight;
                }

                // Update coyote time if the component exists
                bool canJumpCoyote = isGrounded;
                if (world.TryGetComponent<CoyoteTimeComponent>(entity, out var coyoteTime))
                {
                    UpdateCoyoteTime(ref coyoteTime, isGrounded, deltaTime);
                    canJumpCoyote = isGrounded || coyoteTime.TimeLeftGround < coyoteTime.TimeWindow;
                    world.AddComponent(entity, coyoteTime);
                }

                // Update jump buffer if the component exists
                bool shouldJump = input.Jump;
                if (world.TryGetComponent<JumpBufferComponent>(entity, out var jumpBuffer))
                {
                    UpdateJumpBuffer(ref jumpBuffer, input.Jump, deltaTime);
                    shouldJump = jumpBuffer.JumpBuffered;
                    world.AddComponent(entity, jumpBuffer);
                }

                // Handle horizontal movement
                float moveSpeed = player.MoveSpeed;
                float targetVelocityX = input.Movement.X * moveSpeed;

                // Don't move into walls - prevents jitter when holding movement against wall
                if ((touchingLeft && targetVelocityX < 0) || (touchingRight && targetVelocityX > 0))
                {
                    targetVelocityX = 0;
                }

                // Apply air control if the component exists
                if (world.TryGetComponent<AirControlComponent>(entity, out var airControl) && !isGrounded)
                {
                    moveSpeed *= airControl.AirControlFactor;
                    targetVelocityX = input.Movement.X * moveSpeed;

                    // Don't move into walls in air either
                    if ((touchingLeft && targetVelocityX < 0) || (touchingRight && targetVelocityX > 0))
                    {
                        targetVelocityX = 0;
                    }

                    // Smooth air acceleration instead of instant direction change
                    velocity.Value.X = Lerp(velocity.Value.X, targetVelocityX, airControl.AirAcceleration);
                }
                else
                {
                    // Ground movement - instant direction change
                    velocity.Value.X = targetVelocityX;
                }

                // Handle jumping
                if (shouldJump && canJumpCoyote)
                {
                    velocity.Value.Y = -player.JumpForce;

                    // Reset coyote time so we can't jump again immediately
                    if (world.TryGetComponent<CoyoteTimeComponent>(entity, out coyoteTime))
                    {
                        coyoteTime.TimeLeftGround = 999f;
                        world.AddComponent(entity, coyoteTime);
                    }

                    // Reset jump buffer
                    if (world.TryGetComponent<JumpBufferComponent>(entity, out jumpBuffer))
                    {
                        jumpBuffer.JumpBuffered = false;
                        jumpBuffer.TimeSinceJumpPressed = 999f;
                        world.AddComponent(entity, jumpBuffer);
                    }

                    // Start tracking variable jump
                    if (world.TryGetComponent<VariableJumpComponent>(entity, out var variableJump))
                    {
                        variableJump.IsJumping = true;
                        variableJump.CurrentJumpTime = 0f;
                        world.AddComponent(entity, variableJump);
                    }
                }

                // Handle variable jump height
                if (world.TryGetComponent<VariableJumpComponent>(entity, out var varJump))
                {
                    UpdateVariableJump(ref varJump, ref velocity, input.JumpHeld, isGrounded, deltaTime);
                    world.AddComponent(entity, varJump);
                }

                world.AddComponent(entity, velocity);
            }
        }

        private void UpdateCoyoteTime(ref CoyoteTimeComponent coyoteTime, bool isGrounded, float deltaTime)
        {
            if (isGrounded)
            {
                coyoteTime.TimeLeftGround = 0f;
            }
            else if (coyoteTime.WasGroundedLastFrame)
            {
                // Just left the ground - start the coyote time window
                coyoteTime.TimeLeftGround = 0f;
            }
            else
            {
                coyoteTime.TimeLeftGround += deltaTime;
            }

            coyoteTime.WasGroundedLastFrame = isGrounded;
        }

        private void UpdateJumpBuffer(ref JumpBufferComponent jumpBuffer, bool jumpPressed, float deltaTime)
        {
            if (jumpPressed)
            {
                jumpBuffer.JumpBuffered = true;
                jumpBuffer.TimeSinceJumpPressed = 0f;
            }
            else
            {
                jumpBuffer.TimeSinceJumpPressed += deltaTime;

                // Clear buffer if too much time has passed
                if (jumpBuffer.TimeSinceJumpPressed > jumpBuffer.BufferWindow)
                {
                    jumpBuffer.JumpBuffered = false;
                }
            }
        }

        private void UpdateVariableJump(ref VariableJumpComponent varJump, ref VelocityComponent velocity,
                                       bool jumpHeld, bool isGrounded, float deltaTime)
        {
            if (varJump.IsJumping)
            {
                varJump.CurrentJumpTime += deltaTime;

                // If jump is released early and we're still moving upward, cut the jump short
                if (!jumpHeld && varJump.CurrentJumpTime > varJump.MinJumpTime && velocity.Value.Y < 0)
                {
                    velocity.Value.Y *= varJump.JumpCutMultiplier;
                    varJump.IsJumping = false;
                }

                // If we've held jump for max time or hit the ground, end the jump
                if (varJump.CurrentJumpTime > varJump.MaxJumpTime || isGrounded)
                {
                    varJump.IsJumping = false;
                }
            }
            else if (isGrounded)
            {
                // Reset jump state when grounded
                varJump.CurrentJumpTime = 0f;
            }
        }

        private float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
    }
}
