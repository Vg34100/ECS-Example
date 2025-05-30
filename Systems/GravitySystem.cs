// Systems/GravitySystem.cs
using ECS_Example.Components;

namespace ECS_Example.Systems
{
    public class GravitySystem
    {
        private const float MAX_FALL_SPEED = 600f;

        public void Update(World world, float deltaTime)
        {
            foreach (var entity in world.Query<Gravity, Velocity>())
            {
                var gravity = world.GetComponent<Gravity>(entity);
                if (gravity.IsGrounded) continue; // Skip grounded entities

                var velocity = world.GetComponent<Velocity>(entity);
                velocity.Value.Y += gravity.Value * deltaTime;

                if (velocity.Value.Y > MAX_FALL_SPEED)
                    velocity.Value.Y = MAX_FALL_SPEED;

                world.AddComponent(entity, velocity);
            }
        }
    }
}