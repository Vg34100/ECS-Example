// Systems/StunSystem.cs
using ECS_Example.Components;

namespace ECS_Example.Systems
{
    public class StunSystem
    {
        public void Update(World world, float deltaTime)
        {
            foreach (var entity in world.GetEntities())
            {
                if (world.TryGetComponent<Stunned>(entity, out var stun))
                {
                    stun.TimeRemaining -= deltaTime;

                    if (stun.TimeRemaining <= 0)
                    {
                        world.RemoveComponent<Stunned>(entity);
                    }
                    else
                    {
                        world.AddComponent(entity, stun);
                    }
                }
            }
        }
    }
}