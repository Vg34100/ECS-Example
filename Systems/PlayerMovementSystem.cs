// Systems/PlayerMovementSystem.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ECS_Example.Components;

namespace ECS_Example.Systems
{
    public class PlayerMovementSystem
    {
        public void Update(World world)
        {
            var keyboardState = Keyboard.GetState();

            foreach (var entity in world.GetEntities())
            {
                if (!world.TryGetComponent<PlayerController>(entity, out var controller) ||
                    !world.TryGetComponent<Velocity>(entity, out var velocity))
                    continue;

                // Skip input if stunned
                if (world.TryGetComponent<Stunned>(entity, out _))
                    continue;

                // Horizontal movement
                velocity.Value.X = 0;

                if (keyboardState.IsKeyDown(Keys.Left))
                    velocity.Value.X = -controller.MoveSpeed;
                else if (keyboardState.IsKeyDown(Keys.Right))
                    velocity.Value.X = controller.MoveSpeed;

                world.AddComponent(entity, velocity);
            }
        }
    }
}