// Systems/FlashSystem.cs
using ECS_Example.Components;

namespace ECS_Example.Systems
{
    public class FlashSystem
    {
        public void Update(World world, float deltaTime)
        {
            foreach (var entity in world.GetEntities())
            {
                if (world.TryGetComponent<FlashEffect>(entity, out var flash) &&
                    world.TryGetComponent<Invulnerable>(entity, out var invuln))
                {
                    flash.TimeUntilFlash -= deltaTime;

                    if (flash.TimeUntilFlash <= 0)
                    {
                        flash.IsVisible = !flash.IsVisible;
                        flash.TimeUntilFlash = flash.FlashInterval;
                    }

                    world.AddComponent(entity, flash);
                }
                else if (world.TryGetComponent<FlashEffect>(entity, out flash))
                {
                    // Remove flash effect if no longer invulnerable
                    world.RemoveComponent<FlashEffect>(entity);
                }
            }
        }
    }
}