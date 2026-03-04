using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.Stats.Components;
using ECS_Base.Mechanics.Collision.Components;

namespace ECS_Base.Mechanics.Movement.Systems
{
    /// <summary>
    /// Moves entities back and forth between MinX and MaxX.
    /// </summary>
    public class PatrolSystem
    {
        public void Update(World world, float deltaTime)
        {
            foreach (var entity in world.Query<PatrolComponent, PositionComponent, VelocityComponent>())
            {
                if (world.TryGetComponent<StatsComponent>(entity, out var stats) && stats.IsDead)
                    continue;

                var patrol = world.GetComponent<PatrolComponent>(entity);
                var position = world.GetComponent<PositionComponent>(entity);
                var velocity = world.GetComponent<VelocityComponent>(entity);

                if (position.Value.X <= patrol.MinX)
                    patrol.Direction = 1;
                else if (position.Value.X >= patrol.MaxX)
                    patrol.Direction = -1;

                if (world.TryGetComponent<GroundedComponent>(entity, out var grounded))
                {
                    if (grounded.TouchingLeft)
                        patrol.Direction = 1;
                    if (grounded.TouchingRight)
                        patrol.Direction = -1;
                }

                velocity.Value.X = patrol.Speed * patrol.Direction;

                world.AddComponent(entity, patrol);
                world.AddComponent(entity, velocity);
            }
        }
    }
}
