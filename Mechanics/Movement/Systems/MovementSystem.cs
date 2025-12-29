using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Movement.Components;

namespace ECS_Base.Mechanics.Movement.Systems
{
    public class MovementSystem
    {
        private const float MAX_VELOCITY_X = 300f; // Prevent horizontal tunneling

        public void Update(World world, float deltaTime)
        {
            foreach (var entity in world.GetEntities())
            {
                if (world.TryGetComponent<PositionComponent>(entity, out var position) &&
                    world.TryGetComponent<VelocityComponent>(entity, out var velocity))
                {
                    // Clamp horizontal velocity to prevent tunneling
                    if (velocity.Value.X > MAX_VELOCITY_X)
                        velocity.Value.X = MAX_VELOCITY_X;
                    else if (velocity.Value.X < -MAX_VELOCITY_X)
                        velocity.Value.X = -MAX_VELOCITY_X;

                    position.Value += velocity.Value * deltaTime;
                    world.AddComponent(entity, position);
                }
            }
        }
    }
}
