using Microsoft.Xna.Framework;
using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Camera.Components;
using ECS_Base.Mechanics.Camera.Systems;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.Rendering.Components;

namespace ECS_Base.GameConfigs
{
    /// <summary>
    /// Blank screen test configuration - minimal setup for testing
    /// </summary>
    public class BlankScreenConfig : IGameConfig
    {
        public string Name => "Blank Screen Test";

        public void Initialize(Game1 game, World world, SystemManager systemManager)
        {
            // Add only the camera system for basic functionality
            var cameraSystem = new CameraSystem(new Vector2(
                game.GraphicsDeviceManager.PreferredBackBufferWidth,
                game.GraphicsDeviceManager.PreferredBackBufferHeight
            ));
            systemManager.AddSystem(cameraSystem);
            game.CameraSystem = cameraSystem;

            // Create a simple camera entity
            var cameraEntity = world.CreateEntity();
            world.AddComponent(cameraEntity, new CameraComponent(
                initialPosition: Vector2.Zero,
                lagFactor: 1.0f,
                offset: Vector2.Zero,
                zoom: 1.0f,
                dampeningThreshold: 0.0f
            ));

            // Add a bright test square in the center of the screen
            var testEntity = world.CreateEntity();
            world.AddComponent(testEntity, new PositionComponent(640, 360)); // Center of 1280x720
            world.AddComponent(testEntity, new ShapeComponent(
                ShapeComponent.ShapeType.Rectangle,
                Color.Cyan,  // Bright cyan so it's very visible
                new Vector2(100, 100)  // Bigger square
            ));

            System.Console.WriteLine("BlankScreenConfig: Created cyan 100x100 square at center (640, 360)");
        }
    }
}
