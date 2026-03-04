using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.Platformer.Components;
using ECS_Base.Mechanics.Platformer.Systems;
using ECS_Base.Mechanics.Stats.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ECS_Base.Mechanics.Platformer.Systems
{
    /// <summary>
    /// Input system for platform demo style movement.
    /// </summary>
    public class PlatformDemoInputSystem
    {
        private KeyboardState _previousKeyboardState;
        private readonly PlatformerPhysicsSystem _physicsSystem;

        public PlatformDemoInputSystem(PlatformerPhysicsSystem physicsSystem)
        {
            _physicsSystem = physicsSystem;
        }

        public void Update(World world, float deltaTime)
        {
            var keyboardState = Keyboard.GetState();

            foreach (var entity in world.Query<PlatformDemoPlayerComponent, VelocityComponent>())
            {
                if (!world.TryGetComponent<VelocityComponent>(entity, out var velocity))
                    continue;

                if (world.TryGetComponent<StatsComponent>(entity, out var stats) && stats.IsDead)
                {
                    velocity.Value = Vector2.Zero;
                    world.AddComponent(entity, velocity);
                    continue;
                }

                velocity.Value.X = 0;
                if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
                    velocity.Value.X = -110f;
                if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
                    velocity.Value.X = 110f;

                world.AddComponent(entity, velocity);

                if ((keyboardState.IsKeyDown(Keys.Space) || keyboardState.IsKeyDown(Keys.W) ||
                     keyboardState.IsKeyDown(Keys.Up)) && !_previousKeyboardState.IsKeyDown(Keys.Space) &&
                    !_previousKeyboardState.IsKeyDown(Keys.W) && !_previousKeyboardState.IsKeyDown(Keys.Up))
                {
                    _physicsSystem.Jump(world, entity);
                }

                if ((_previousKeyboardState.IsKeyDown(Keys.Space) && keyboardState.IsKeyUp(Keys.Space)) ||
                    (_previousKeyboardState.IsKeyDown(Keys.W) && keyboardState.IsKeyUp(Keys.W)) ||
                    (_previousKeyboardState.IsKeyDown(Keys.Up) && keyboardState.IsKeyUp(Keys.Up)))
                {
                    _physicsSystem.ReleaseJump(world, entity);
                }
            }

            _previousKeyboardState = keyboardState;
        }
    }
}
