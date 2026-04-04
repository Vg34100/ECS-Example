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
            foreach (var entity in world.Query<DamageFlashComponent>())
            {
                if (!world.TryGetComponent<DamageFlashComponent>(entity, out var flash))
                    continue;

                flash.TimeRemaining -= deltaTime;
                flash.TimeSinceToggle += deltaTime;

                if (flash.TimeSinceToggle >= flash.FlashInterval)
                {
                    flash.TimeSinceToggle = 0f;
                    flash.FlashOn = !flash.FlashOn;
                }

                var currentColor = flash.FlashOn ? flash.FlashColor : flash.OriginalColor;

                if (world.TryGetComponent<ShapeComponent>(entity, out var shape))
                {
                    shape.Color = currentColor;
                    world.AddComponent(entity, shape);
                }

                if (world.TryGetComponent<SpriteComponent>(entity, out var sprite))
                {
                    sprite.Tint = currentColor;
                    world.AddComponent(entity, sprite);
                }

                if (flash.TimeRemaining <= 0f)
                {
                    if (world.TryGetComponent<ShapeComponent>(entity, out var finalShape))
                    {
                        finalShape.Color = flash.OriginalColor;
                        world.AddComponent(entity, finalShape);
                    }

                    if (world.TryGetComponent<SpriteComponent>(entity, out var finalSprite))
                    {
                        finalSprite.Tint = flash.OriginalColor;
                        world.AddComponent(entity, finalSprite);
                    }

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
