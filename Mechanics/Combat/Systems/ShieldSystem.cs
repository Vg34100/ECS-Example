using ECS_Base.Mechanics.Combat.Components;
using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Input.Components;
using ECS_Base.Mechanics.Stats.Components;

namespace ECS_Base.Mechanics.Combat.Systems
{
    /// <summary>
    /// Sets invulnerability while blocking.
    /// </summary>
    public class ShieldSystem
    {
        public void Update(World world, float deltaTime)
        {
            foreach (var entity in world.Query<ShieldComponent, InputComponent, StatsComponent>())
            {
                var input = world.GetComponent<InputComponent>(entity);
                var stats = world.GetComponent<StatsComponent>(entity);

                stats.IsInvulnerable = input.BlockHeld;
                world.AddComponent(entity, stats);

                world.AddComponent(entity, new BlockingComponent(input.BlockHeld));
            }
        }
    }
}
