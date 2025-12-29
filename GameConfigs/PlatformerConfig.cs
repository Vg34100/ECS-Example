using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Camera.Components;
using ECS_Base.Mechanics.Camera.Systems;
using ECS_Base.Mechanics.Movement.Systems;
using ECS_Base.Mechanics.Level.Systems;
using ECS_Base.Mechanics.Collision.Systems;
using ECS_Base.Mechanics.Input.Systems;
using ECS_Base.Mechanics.PlayerController.Systems;
using ECS_Base.Mechanics.Gravity.Systems;
using ECS_Base.Mechanics.Rendering.Systems;

namespace ECS_Base.GameConfigs
{
    /// <summary>
    /// Platformer test configuration - full game setup with levels and collision
    /// </summary>
    public class PlatformerConfig : IGameConfig
    {
        public string Name => "Platformer Test";

        public void Initialize(Game1 game, World world, SystemManager systemManager)
        {
            // Input system (first - reads input)
            systemManager.AddSystem(new InputSystem());

            // Player controller (uses input to set velocity)
            systemManager.AddSystem(new PlayerControllerSystem());

            // Gravity system (applies gravity to velocity)
            systemManager.AddSystem(new GravitySystem());

            // Movement system (applies velocity to position)
            systemManager.AddSystem(new MovementSystem());

            // Level entity spawning
            systemManager.AddSystem(new LevelEntitySystem());

            // Collision system (must run after movement)
            var collisionSystem = new CollisionSystem();
            systemManager.AddSystem(collisionSystem);
            game.CollisionSystem = collisionSystem;

            // Camera system with smooth following
            var cameraSystem = new CameraSystem(new Vector2(
                game.GraphicsDeviceManager.PreferredBackBufferWidth,
                game.GraphicsDeviceManager.PreferredBackBufferHeight
            ));
            systemManager.AddSystem(cameraSystem);
            game.CameraSystem = cameraSystem;

            // Level manager system
            var levelManagerSystem = new LevelManagerSystem();
            game.LevelManagerSystem = levelManagerSystem;

            // Debug collision visualization
            systemManager.AddSystem(new CollisionDebugRenderSystem(
                game.GraphicsDevice.Viewport.Width > 0 ? new SpriteBatch(game.GraphicsDevice) : null,
                game.GraphicsDevice,
                cameraSystem,
                collisionSystem
            ));

            // Create camera entity with smooth following
            var cameraEntity = world.CreateEntity();
            world.AddComponent(cameraEntity, new CameraComponent(
                initialPosition: Vector2.Zero,
                lagFactor: 0.97f,
                offset: new Vector2(0, -50),
                zoom: 3.0f,
                dampeningThreshold: 5.0f
            ));

            // Load levels after content is loaded (will be called from LoadContent)
            // This is handled separately in Game1.LoadContent
        }
    }
}
