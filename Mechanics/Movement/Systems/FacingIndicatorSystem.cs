using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.Collision.Components;
using System.Linq;
using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Movement.Systems
{
    /// <summary>
    /// Updates indicator position based on target facing.
    /// </summary>
    public class FacingIndicatorSystem
    {
        public void Update(World world, float deltaTime)
        {
            foreach (var entity in world.Query<FacingIndicatorComponent, PositionComponent>())
            {
                var indicator = world.GetComponent<FacingIndicatorComponent>(entity);
                var target = world.GetEntities().FirstOrDefault(e => e.Id == indicator.TargetEntityId);
                if (target == null)
                    continue;

                if (!world.TryGetComponent<PositionComponent>(target, out var targetPos))
                    continue;

                var targetCenter = targetPos.Value;
                if (world.TryGetComponent<ColliderComponent>(target, out var targetCollider))
                {
                    targetCenter += new Microsoft.Xna.Framework.Vector2(
                        targetCollider.Bounds.Width * 0.5f,
                        targetCollider.Bounds.Height * 0.5f
                    );
                }

                var dir = new Microsoft.Xna.Framework.Vector2(1, 0);
                if (world.TryGetComponent<FacingComponent>(target, out var facing) &&
                    facing.Direction.LengthSquared() > 0.01f)
                {
                    dir = facing.Direction;
                }

                if (!world.TryGetComponent<PositionComponent>(entity, out var pos))
                    continue;

                if (world.TryGetComponent<Rendering.Components.WorldIconComponent>(entity, out var icon))
                {
                    float size = icon.PixelSize * 8f;
                    pos.Value = targetCenter + indicator.Offset + dir * indicator.Distance - new Microsoft.Xna.Framework.Vector2(size / 2f, size / 2f);
                }
                else
                {
                    pos.Value = targetCenter + indicator.Offset + dir * indicator.Distance;
                }
                world.AddComponent(entity, pos);

                if (world.TryGetComponent<Rendering.Components.WorldIconComponent>(entity, out var icon2))
                {
                    icon2.Type = UI.Components.UIIconType.Arrow;
                    icon2.Rotation = (float)System.Math.Atan2(dir.Y, dir.X) + MathHelper.PiOver2;
                    world.AddComponent(entity, icon2);
                }
            }
        }
    }
}
