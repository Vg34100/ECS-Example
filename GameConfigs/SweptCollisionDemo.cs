using Microsoft.Xna.Framework;
using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Camera.Components;
using ECS_Base.Mechanics.Camera.Systems;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.Movement.Systems;
using ECS_Base.Mechanics.Collision.Components;
using ECS_Base.Mechanics.Collision.Systems;
using ECS_Base.Mechanics.Gravity.Components;
using ECS_Base.Mechanics.Gravity.Systems;
using ECS_Base.Mechanics.Rendering.Components;
using ECS_Base.Mechanics.Rendering.Systems;
using ECS_Base.Mechanics.Input.Components;
using ECS_Base.Mechanics.Input.Systems;
using ECS_Base.Mechanics.PlayerController.Components;
using ECS_Base.Mechanics.PlayerController.Systems;

namespace ECS_Base.GameConfigs
{
    /// <summary>
    /// Demo showing both collision systems:
    /// - Blue box: Regular AABB collision (velocity limited)
    /// - Red box: Swept collision (no velocity limit - can go super fast!)
    /// </summary>
    public class SweptCollisionDemo : IGameConfig
    {
        public string Name => "Swept Collision Demo";

        public void Initialize(Game1 game, World world, SystemManager systemManager)
        {
            // Add systems in order
            systemManager.AddSystem(new InputSystem());
            systemManager.AddSystem(new PlayerControllerSystem());
            systemManager.AddSystem(new GravitySystem());
            systemManager.AddSystem(new MovementSystem());

            // Regular collision system (for blue box)
            var collisionSystem = new CollisionSystem();
            systemManager.AddSystem(collisionSystem);
            game.CollisionSystem = collisionSystem;

            // Swept collision system (for red box)
            systemManager.AddSystem(new SweptCollisionSystem());

            var cameraSystem = new CameraSystem(new Vector2(
                game.GraphicsDeviceManager.PreferredBackBufferWidth,
                game.GraphicsDeviceManager.PreferredBackBufferHeight
            ));
            systemManager.AddSystem(cameraSystem);
            game.CameraSystem = cameraSystem;

            // Create camera
            var cameraEntity = world.CreateEntity();
            world.AddComponent(cameraEntity, new CameraComponent(
                initialPosition: Vector2.Zero,
                lagFactor: 0.97f,
                offset: new Vector2(0, -100),
                zoom: 2.0f,
                dampeningThreshold: 5.0f
            ));

            // Create multi-level platforms for testing
            // Ground
            CreatePlatform(world, 100, 500, 800, 32, Color.Green);

            CreatePlatform(world, 100, 0, 32, 800, Color.Green);


            // Low platforms
            CreatePlatform(world, 150, 420, 80, 16, Color.Brown);
            CreatePlatform(world, 300, 420, 80, 16, Color.Brown);
            CreatePlatform(world, 450, 420, 80, 16, Color.Brown);

            // Mid platforms
            CreatePlatform(world, 200, 340, 80, 16, Color.Orange);
            CreatePlatform(world, 400, 340, 80, 16, Color.Orange);

            // High platforms
            CreatePlatform(world, 250, 260, 80, 16, Color.Red);
            CreatePlatform(world, 450, 260, 80, 16, Color.Red);

            // Very high platform
            CreatePlatform(world, 350, 150, 100, 16, Color.Purple);

            // Blue box - SWEPT collision - YOU CONTROL THIS ONE
            var blueBox = world.CreateEntity();
            world.AddComponent(blueBox, new PositionComponent(350, 100));
            world.AddComponent(blueBox, new VelocityComponent(0, 0));
            world.AddComponent(blueBox, new GravityComponent(1.0f));
            world.AddComponent(blueBox, new ColliderComponent(
                new Rectangle(0, 0, 12, 16),
                ColliderComponent.ColliderType.Dynamic
            ));
            world.AddComponent(blueBox, new GroundedComponent());
            world.AddComponent(blueBox, new ShapeComponent(
                ShapeComponent.ShapeType.Rectangle,
                Color.Blue,
                new Vector2(12, 16)
            ));
            world.AddComponent(blueBox, new PlayerComponent(moveSpeed: 120f, jumpForce: 280f)); // Faster movement, higher jump
            world.AddComponent(blueBox, new InputComponent());
            world.AddComponent(blueBox, new CameraTargetComponent { IsActive = true });
            world.AddComponent(blueBox, new SweptCollisionComponent()); // NOW USES SWEPT COLLISION!

            // Remove the red box - not needed for testing

            System.Console.WriteLine("=== Swept Collision Demo ===");
            System.Console.WriteLine("Blue player: SWEPT collision (no velocity limits!)");
            System.Console.WriteLine("  - Arrow keys/WASD to move, Space to jump");
            System.Console.WriteLine("  - Jump between platforms and fall from high places");
            System.Console.WriteLine("  - Should NEVER fall through platforms, even at high speed!");
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
