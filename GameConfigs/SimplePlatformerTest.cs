using Microsoft.Xna.Framework;
using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Camera.Components;
using ECS_Base.Mechanics.Camera.Systems;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.Movement.Systems;
using ECS_Base.Mechanics.Collision.Components;
using ECS_Base.Mechanics.Collision.Systems;
using ECS_Base.Mechanics.Input.Components;
using ECS_Base.Mechanics.Input.Systems;
using ECS_Base.Mechanics.PlayerController.Components;
using ECS_Base.Mechanics.PlayerController.Systems;
using ECS_Base.Mechanics.Gravity.Components;
using ECS_Base.Mechanics.Gravity.Systems;
using ECS_Base.Mechanics.Rendering.Components;
using ECS_Base.Mechanics.Rendering.Systems;

namespace ECS_Base.GameConfigs
{
    /// <summary>
    /// Simple platformer test - manually placed platforms for testing collision
    /// </summary>
    public class SimplePlatformerTest : IGameConfig
    {
        public string Name => "Simple Platformer Test";

        public void Initialize(Game1 game, World world, SystemManager systemManager)
        {
            // Add systems in order
            systemManager.AddSystem(new InputSystem());
            systemManager.AddSystem(new PlayerControllerSystem());
            systemManager.AddSystem(new GravitySystem());
            systemManager.AddSystem(new MovementSystem());

            var collisionSystem = new CollisionSystem();
            systemManager.AddSystem(collisionSystem);
            game.CollisionSystem = collisionSystem;

            var cameraSystem = new CameraSystem(new Vector2(
                game.GraphicsDeviceManager.PreferredBackBufferWidth,
                game.GraphicsDeviceManager.PreferredBackBufferHeight
            ));
            systemManager.AddSystem(cameraSystem);
            game.CameraSystem = cameraSystem;

            // Create camera entity
            var cameraEntity = world.CreateEntity();
            world.AddComponent(cameraEntity, new CameraComponent(
                initialPosition: Vector2.Zero,
                lagFactor: 0.97f,
                offset: new Vector2(0, -50),
                zoom: 3.0f,
                dampeningThreshold: 5.0f
            ));

            // Create player at center of screen
            var playerEntity = world.CreateEntity();
            world.AddComponent(playerEntity, new PositionComponent(400, 200));
            world.AddComponent(playerEntity, new VelocityComponent());
            world.AddComponent(playerEntity, new PlayerComponent(moveSpeed: 80f, jumpForce: 200f));
            world.AddComponent(playerEntity, new InputComponent());
            world.AddComponent(playerEntity, new GravityComponent(gravityScale: 1.0f));
            world.AddComponent(playerEntity, new ColliderComponent(
                new Rectangle(0, 0, 12, 16), // 12x16 player size
                ColliderComponent.ColliderType.Dynamic
            ));
            world.AddComponent(playerEntity, new GroundedComponent(false));
            world.AddComponent(playerEntity, new ShapeComponent(
                ShapeComponent.ShapeType.Rectangle,
                Color.Blue,
                new Vector2(12, 16)
            ));
            world.AddComponent(playerEntity, new CameraTargetComponent { IsActive = true });

            // Add platformer feel components (optional - comment these out to test without them)
            world.AddComponent(playerEntity, new CoyoteTimeComponent(timeWindow: 0.15f));
            world.AddComponent(playerEntity, new JumpBufferComponent(bufferWindow: 0.15f));
            world.AddComponent(playerEntity, new VariableJumpComponent(
                minJumpTime: 0.1f,
                maxJumpTime: 0.4f,
                jumpCutMultiplier: 0.5f
            ));
            world.AddComponent(playerEntity, new AirControlComponent(
                airControlFactor: 0.8f,
                airAcceleration: 0.5f
            ));

            // Create ground platform (wide platform at bottom)
            CreatePlatform(world, 300, 400, 400, 32, Color.Green);

            // Create a floating platform (left)
            CreatePlatform(world, 200, 300, 100, 16, Color.Brown);

            // Create a floating platform (right)
            CreatePlatform(world, 550, 250, 100, 16, Color.Brown);

            // Create a wall (left side)
            CreatePlatform(world, 200, 200, 16, 200, Color.Gray);

            // Create a wall (right side)
            CreatePlatform(world, 500, 200, 16, 200, Color.Gray);

            System.Console.WriteLine("SimplePlatformerTest: Created player and test platforms");
        }

        private void CreatePlatform(World world, float x, float y, int width, int height, Color color)
        {
            var platform = world.CreateEntity();
            world.AddComponent(platform, new PositionComponent(x, y));
            world.AddComponent(platform, new ColliderComponent(
                new Rectangle(0, 0, width, height),
                ColliderComponent.ColliderType.Static
            ));
            world.AddComponent(platform, new ShapeComponent(
                ShapeComponent.ShapeType.Rectangle,
                color,
                new Vector2(width, height)
            ));
        }
    }
}
