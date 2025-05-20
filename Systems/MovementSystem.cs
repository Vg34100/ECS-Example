// Systems/MovementSystem.cs
using Microsoft.Xna.Framework;
using ECS_Example.Components;

namespace ECS_Example.Systems
{
    public class MovementSystem
    {
        public void Update(World world, float deltaTime)
        {
            foreach (var entity in world.GetEntities())
            {
                if (world.TryGetComponent<Position>(entity, out var position) &&
                    world.TryGetComponent<Velocity>(entity, out var velocity))
                {
                    position.Value += velocity.Value * deltaTime;
                    world.AddComponent(entity, position);
                }
            }
        }
    }
}