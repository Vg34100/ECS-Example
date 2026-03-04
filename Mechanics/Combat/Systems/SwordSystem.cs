using ECS_Base.Mechanics.Combat.Components;
using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Input.Components;
using ECS_Base.Mechanics.Movement.Components;
using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Combat.Systems
{
    /// <summary>
    /// Triggers melee attacks from input.
    /// </summary>
    public class SwordSystem
    {
        private readonly MeleeCombatSystem _meleeSystem;

        public SwordSystem(MeleeCombatSystem meleeSystem)
        {
            _meleeSystem = meleeSystem;
        }

        public void Update(World world, float deltaTime)
        {
            foreach (var entity in world.Query<MeleeWeaponComponent, InputComponent>())
            {
                var input = world.GetComponent<InputComponent>(entity);
                if (!input.Attack)
                    continue;

                Vector2 direction = new Vector2(1, 0);
                if (world.TryGetComponent<FacingComponent>(entity, out var facing) &&
                    facing.Direction.LengthSquared() > 0.01f)
                {
                    direction = facing.Direction;
                }

                _meleeSystem.StartAttack(world, entity, direction);
            }
        }
    }
}
