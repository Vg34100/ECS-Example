using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Input.Components;
using ECS_Base.Mechanics.Movement.Components;
using Microsoft.Xna.Framework;
using ECS_Base.Mechanics.Stats.Components;

namespace ECS_Base.Mechanics.Movement.Systems
{
    /// <summary>
    /// Handles 8-directional top-down movement
    /// Uses input to directly set velocity (no gravity, no jumping)
    /// </summary>
    public class TopDownMovementSystem
    {
        public void Update(World world, float deltaTime)
        {
            foreach (var entity in world.Query<TopDownMovementComponent, InputComponent, VelocityComponent>())
            {
                if (world.TryGetComponent<StatsComponent>(entity, out var stats) && stats.IsDead)
                {
                    if (world.TryGetComponent<VelocityComponent>(entity, out var deadVel))
                    {
                        deadVel.Value = Vector2.Zero;
                        world.AddComponent(entity, deadVel);
                    }
                    continue;
                }

                var movement = world.GetComponent<TopDownMovementComponent>(entity);
                var input = world.GetComponent<InputComponent>(entity);
                var velocity = world.GetComponent<VelocityComponent>(entity);

                // Normalize diagonal movement to prevent faster speed
                Vector2 movementDirection = input.Movement;
                if (movementDirection.LengthSquared() > 0)
                {
                    movementDirection.Normalize();
                    if (world.TryGetComponent<FacingComponent>(entity, out var facing))
                    {
                        facing.Direction = movementDirection;
                        world.AddComponent(entity, facing);
                    }
                    else
                    {
                        world.AddComponent(entity, new FacingComponent(movementDirection));
                    }
                }

                velocity.Value = movementDirection * movement.MoveSpeed;

                world.AddComponent(entity, velocity);
            }
        }
    }
}
