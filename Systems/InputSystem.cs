// Systems/InputSystem.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ECS_Example.Components;
using System.Collections.Specialized;
using System.Diagnostics;

namespace ECS_Example.Systems
{
    public class InputSystem
    {
        public void Update(World world)
        {
            var keyboardState = Keyboard.GetState();

            foreach (var entity in world.GetEntities())
            {
                if (world.TryGetComponent<Input>(entity, out var input) &&
                    world.TryGetComponent<Velocity>(entity, out var velocity) &&
                    input.IsPlayerControlled)
                {
                    // Left/right movement
                    velocity.Value.X = 0;

                    if (keyboardState.IsKeyDown(Keys.Left))
                        velocity.Value.X = -200;
                    else if (keyboardState.IsKeyDown(Keys.Right))
                        velocity.Value.X = 200;

                    // Jumping
                    if (keyboardState.IsKeyDown(Keys.Space) &&
                        world.TryGetComponent<Gravity>(entity, out var gravity) &&
                        gravity.IsGrounded)
                    {
                        velocity.Value.Y = -400; // Jump velocity
                    }

                    world.AddComponent(entity, velocity);
                }
            }
        }
    }
}