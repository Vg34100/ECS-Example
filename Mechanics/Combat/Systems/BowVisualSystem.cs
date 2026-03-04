using ECS_Base.Mechanics.Combat.Components;
using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Input.Components;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.Collision.Components;
using ECS_Base.Mechanics.Rendering.Components;
using Microsoft.Xna.Framework;
using System.Linq;

namespace ECS_Base.Mechanics.Combat.Systems
{
    /// <summary>
    /// Shows a short-lived bow visual when shooting.
    /// </summary>
    public class BowVisualSystem
    {
        public void Update(World world, float deltaTime)
        {
            foreach (var entity in world.Query<BowVisualComponent, PositionComponent, ShapeComponent>())
            {
                var visual = world.GetComponent<BowVisualComponent>(entity);
                var target = world.GetEntities().FirstOrDefault(e => e.Id == visual.TargetEntityId);
                if (target == null)
                    continue;

                if (world.TryGetComponent<InputComponent>(target, out var input) && input.ShootHeld)
                {
                    visual.TimeRemaining = 0.2f;
                    world.AddComponent(entity, visual);
                }

                visual.TimeRemaining -= deltaTime;
                if (visual.TimeRemaining <= 0f)
                {
                    if (world.TryGetComponent<Rendering.Components.RotatedRectComponent>(entity, out _))
                        world.RemoveComponent<Rendering.Components.RotatedRectComponent>(entity);

                    var shapeOff = world.GetComponent<ShapeComponent>(entity);
                    shapeOff.Color = Color.Transparent;
                    world.AddComponent(entity, shapeOff);
                    continue;
                }

                if (!world.TryGetComponent<PositionComponent>(target, out var targetPos))
                    continue;

                var targetCenter = targetPos.Value;
                if (world.TryGetComponent<ColliderComponent>(target, out var targetCollider))
                {
                    targetCenter += new Vector2(
                        targetCollider.Bounds.Width * 0.5f,
                        targetCollider.Bounds.Height * 0.5f
                    );
                }

                var dir = new Vector2(1, 0);
                if (world.TryGetComponent<FacingComponent>(target, out var facing) &&
                    facing.Direction.LengthSquared() > 0.01f)
                {
                    dir = facing.Direction;
                }

                var pos = world.GetComponent<PositionComponent>(entity);
                pos.Value = targetCenter + dir * visual.Distance;
                world.AddComponent(entity, pos);

                var rotation = (float)System.Math.Atan2(dir.Y, dir.X) + MathHelper.PiOver2;
                var size = new Vector2(visual.Size.X, visual.Size.Y);

                world.AddComponent(entity, new Rendering.Components.RotatedRectComponent(
                    center: pos.Value,
                    size: size,
                    rotation: rotation,
                    color: visual.Color
                ));

                world.AddComponent(entity, visual);
            }
        }
    }
}
