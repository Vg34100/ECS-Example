using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Movement.Components;
using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Movement.Systems
{
    /// <summary>
    /// Applies a simple vertical bob to entities.
    /// </summary>
    public class FloatingSystem
    {
        public void Update(World world, float deltaTime)
        {
            foreach (var entity in world.Query<FloatingComponent, PositionComponent>())
            {
                if (!world.TryGetComponent<FloatingComponent>(entity, out var floating))
                    continue;

                if (!world.TryGetComponent<PositionComponent>(entity, out var position))
                    continue;

                floating.Time += deltaTime * floating.Speed;
                position.Value = new Vector2(
                    floating.BasePosition.X,
                    floating.BasePosition.Y + (float)System.Math.Sin(floating.Time) * floating.Amplitude
                );

                world.AddComponent(entity, floating);
                world.AddComponent(entity, position);
            }
        }
    }
}
