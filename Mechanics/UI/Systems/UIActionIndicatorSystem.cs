using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Input.Components;
using ECS_Base.Mechanics.Progression.Components;
using ECS_Base.Mechanics.UI.Components;
using System.Linq;

namespace ECS_Base.Mechanics.UI.Systems
{
    /// <summary>
    /// Updates UI icons based on player action state.
    /// </summary>
    public class UIActionIndicatorSystem
    {
        public void Update(World world)
        {
            foreach (var entity in world.Query<UIActionIndicatorComponent, UIIconComponent>())
            {
                var indicator = world.GetComponent<UIActionIndicatorComponent>(entity);
                var icon = world.GetComponent<UIIconComponent>(entity);

                var target = world.GetEntities().FirstOrDefault(e => e.Id == indicator.TargetEntityId);
                if (target == null || !world.TryGetComponent<InputComponent>(target, out var input))
                    continue;

                bool active = indicator.Action switch
                {
                    ActionType.Sword => input.Attack,
                    ActionType.Shield => input.BlockHeld,
                    ActionType.Bow => input.ShootHeld && HasArrows(world, target),
                    _ => false
                };

                icon.Color = active ? indicator.OnColor : indicator.OffColor;
                world.AddComponent(entity, icon);
            }
        }

        private bool HasArrows(World world, Entity target)
        {
            if (world.TryGetComponent(target, out AmmoComponent ammo))
            {
                return ammo.Arrows > 0;
            }
            return true;
        }
    }
}
