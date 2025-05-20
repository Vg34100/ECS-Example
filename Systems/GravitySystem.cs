// Systems/GravitySystem.cs
using ECS_Example.Components;

namespace ECS_Example.Systems
{
    public class GravitySystem
    {
        private const float MAX_FALL_SPEED = 600f;

        public void Update(World world, float deltaTime)
        {
            foreach (var entity in world.GetEntities())
            {
                if (world.TryGetComponent<Gravity>(entity, out var gravity) &&
                    world.TryGetComponent<Velocity>(entity, out var velocity))
                {
                    if (!gravity.IsGrounded)
                    {
                        // Apply gravity
                        velocity.Value.Y += gravity.Value * deltaTime;

                        // Cap fall speed
                        if (velocity.Value.Y > MAX_FALL_SPEED)
                            velocity.Value.Y = MAX_FALL_SPEED;

                        world.AddComponent(entity, velocity);
                    }
                }
            }
        }
    }
}