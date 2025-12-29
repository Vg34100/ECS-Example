using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Input.Components;
using ECS_Base.Mechanics.Movement.Components;
using Microsoft.Xna.Framework;

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
                var movement = world.GetComponent<TopDownMovementComponent>(entity);
                var input = world.GetComponent<InputComponent>(entity);
                var velocity = world.GetComponent<VelocityComponent>(entity);

                // Normalize diagonal movement to prevent faster speed
                Vector2 movementDirection = input.Movement;
                if (movementDirection.LengthSquared() > 0)
                {
                    movementDirection.Normalize();
                }

                velocity.Value = movementDirection * movement.MoveSpeed;

                world.AddComponent(entity, velocity);
            }
        }
    }
}
