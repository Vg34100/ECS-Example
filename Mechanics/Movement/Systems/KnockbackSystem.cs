using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Movement.Components;
using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Movement.Systems
{
    /// <summary>
    /// Applies knockback velocity and decays it.
    /// </summary>
    public class KnockbackSystem
    {
        public void Update(World world, float deltaTime)
        {
            foreach (var entity in world.Query<KnockbackComponent, VelocityComponent>())
            {
                if (!world.TryGetComponent<KnockbackComponent>(entity, out var knockback))
                    continue;

                if (!world.TryGetComponent<VelocityComponent>(entity, out var velocity))
                    continue;

                velocity.Value += knockback.Velocity;
                world.AddComponent(entity, velocity);

                // Exponential decay
                float decayFactor = MathHelper.Clamp(1f - knockback.Decay * deltaTime, 0f, 1f);
                knockback.Velocity *= decayFactor;

                if (knockback.Velocity.LengthSquared() < 1f)
                {
                    world.RemoveComponent<KnockbackComponent>(entity);
                }
                else
                {
                    world.AddComponent(entity, knockback);
                }
            }
        }
    }
}
