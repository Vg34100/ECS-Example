using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Gravity.Components;
using ECS_Base.Mechanics.Movement.Components;

namespace ECS_Base.Mechanics.Gravity.Systems
{
    public class GravitySystem
    {
        private const float GRAVITY = 980f; // pixels per second squared
        private const float MAX_FALL_SPEED = 240f; // Limit to 15 pixels/frame at 60fps to prevent tunneling through 16px tiles

        public void Update(World world, float deltaTime)
        {
            foreach (var entity in world.Query<GravityComponent, VelocityComponent>())
            {
                var gravity = world.GetComponent<GravityComponent>(entity);
                var velocity = world.GetComponent<VelocityComponent>(entity);

                // Apply gravity to vertical velocity
                velocity.Value.Y += GRAVITY * gravity.GravityScale * deltaTime;

                // Clamp fall speed to prevent tunneling
                if (velocity.Value.Y > MAX_FALL_SPEED)
                {
                    velocity.Value.Y = MAX_FALL_SPEED;
                }

                world.AddComponent(entity, velocity);
            }
        }
    }
}
