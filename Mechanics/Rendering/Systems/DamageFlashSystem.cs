using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Rendering.Components;

namespace ECS_Base.Mechanics.Rendering.Systems
{
    /// <summary>
    /// Flashes ShapeComponent color for damage feedback.
    /// </summary>
    public class DamageFlashSystem
    {
        public void Update(World world, float deltaTime)
        {
            foreach (var entity in world.Query<DamageFlashComponent, ShapeComponent>())
            {
                if (!world.TryGetComponent<DamageFlashComponent>(entity, out var flash))
                    continue;

                if (!world.TryGetComponent<ShapeComponent>(entity, out var shape))
                    continue;

                flash.TimeRemaining -= deltaTime;
                flash.TimeSinceToggle += deltaTime;

                if (flash.TimeSinceToggle >= flash.FlashInterval)
                {
                    flash.TimeSinceToggle = 0f;
                    flash.FlashOn = !flash.FlashOn;
                }

                shape.Color = flash.FlashOn ? flash.FlashColor : flash.OriginalColor;
                world.AddComponent(entity, shape);

                if (flash.TimeRemaining <= 0f)
                {
                    shape.Color = flash.OriginalColor;
                    world.AddComponent(entity, shape);
                    world.RemoveComponent<DamageFlashComponent>(entity);
                }
                else
                {
                    world.AddComponent(entity, flash);
                }
            }
        }
    }
}
