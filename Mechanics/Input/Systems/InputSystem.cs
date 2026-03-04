using Microsoft.Xna.Framework.Input;
using ECS_Base.Mechanics.Core;
using Microsoft.Xna.Framework;
using ECS_Base.Mechanics.Input.Components;

namespace ECS_Base.Mechanics.Input.Systems
{
    public class InputSystem
    {
        private KeyboardState _previousKeyboardState;

        public void Update(World world)
        {
            var keyboardState = Keyboard.GetState();

            foreach (var entity in world.Query<InputComponent>())
            {
                var input = world.GetComponent<InputComponent>(entity);

                // Horizontal movement
                float horizontal = 0f;
                if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
                    horizontal -= 1f;
                if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
                    horizontal += 1f;

                // Vertical movement (for testing, not typical platformer)
                float vertical = 0f;
                if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W))
                    vertical -= 1f;
                if (keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S))
                    vertical += 1f;

                input.Movement = new Vector2(horizontal, vertical);

                // Jump detection (pressed this frame but not last frame)
                input.Jump = keyboardState.IsKeyDown(Keys.Space) && !_previousKeyboardState.IsKeyDown(Keys.Space);
                input.JumpHeld = keyboardState.IsKeyDown(Keys.Space);

                // Top-down actions
                input.Attack = keyboardState.IsKeyDown(Keys.J) && !_previousKeyboardState.IsKeyDown(Keys.J);
                input.BlockHeld = keyboardState.IsKeyDown(Keys.K);
                input.Block = keyboardState.IsKeyDown(Keys.K) && !_previousKeyboardState.IsKeyDown(Keys.K);
                input.Bomb = keyboardState.IsKeyDown(Keys.L) && !_previousKeyboardState.IsKeyDown(Keys.L);
                input.ShootHeld = keyboardState.IsKeyDown(Keys.Space);

                world.AddComponent(entity, input);
            }

            _previousKeyboardState = keyboardState;
        }
    }
}
