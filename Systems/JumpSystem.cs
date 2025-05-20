// Systems/JumpSystem.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ECS_Example.Components;

namespace ECS_Example.Systems
{
    public class JumpSystem
    {
        public void Update(World world)
        {
            var keyboardState = Keyboard.GetState();

            foreach (var entity in world.GetEntities())
            {
                if (!world.TryGetComponent<Jump>(entity, out var jump) ||
                    !world.TryGetComponent<Gravity>(entity, out var gravity) ||
                    !world.TryGetComponent<Velocity>(entity, out var velocity))
                    continue;

                if (keyboardState.IsKeyDown(Keys.Space) && gravity.IsGrounded)
                {
                    velocity.Value.Y = jump.JumpVelocity;
                    world.AddComponent(entity, velocity);
                }
            }
        }
    }
}