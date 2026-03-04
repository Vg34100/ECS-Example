using ECS_Base.Mechanics.Combat.Components;
using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Rendering.Components;
using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Combat.Systems
{
    /// <summary>
    /// Updates explosion visuals.
    /// </summary>
    public class ExplosionSystem
    {
        public void Update(World world, float deltaTime)
        {
            foreach (var entity in world.Query<ExplosionComponent, ShapeComponent>())
            {
                if (!world.TryGetComponent<ExplosionComponent>(entity, out var explosion))
                    continue;

                if (!world.TryGetComponent<ShapeComponent>(entity, out var shape))
                    continue;

                explosion.TimeRemaining -= deltaTime;
                float t = 1f - (explosion.TimeRemaining / explosion.Duration);
                float radius = MathHelper.Lerp(4f, explosion.MaxRadius, t);

                if (world.TryGetComponent<Movement.Components.PositionComponent>(entity, out var pos))
                {
                    pos.Value = explosion.Center;
                    world.AddComponent(entity, pos);
                }

                shape.Type = ShapeComponent.ShapeType.Circle;
                shape.Size = new Vector2(radius * 2f, radius * 2f);
                shape.Color = explosion.Color * (1f - t);

                world.AddComponent(entity, shape);

                if (explosion.TimeRemaining <= 0f)
                {
                    world.RemoveEntity(entity);
                }
                else
                {
                    world.AddComponent(entity, explosion);
                }
            }
        }
    }
}
