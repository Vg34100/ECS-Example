using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.Movement.Systems;
using ECS_Base.Mechanics.Rendering.Components;
using ECS_Base.Mechanics.Camera.Components;
using ECS_Base.Mechanics.Camera.Systems;
using ECS_Base.Mechanics.Platformer.Components;
using ECS_Base.Mechanics.Platformer.Systems;
using ECS_Base.Mechanics.Collision.Components;
using ECS_Base.Mechanics.Collision.Systems;
using ECS_Base.Mechanics.PlayerController.Components;
using System.Collections.Generic;

namespace ECS_Base.GameConfigs
{
    /// <summary>
    /// Demo showcasing platform system with various platform types
    /// </summary>
    public class PlatformDemo : IGameConfig
    {
        public string Name => "Platform System Demo";

        public void Initialize(Game1 game, World world, SystemManager systemManager)
        {
            // Add systems in correct order
            var physicsSystem = new PlatformerPhysicsSystem();

            // Input first
            systemManager.AddSystem(new PlatformDemoInputSystem(physicsSystem));

            // Physics (gravity)
            systemManager.AddSystem(physicsSystem);

            // Platform movement
            var platformSystem = new PlatformSystem();
            systemManager.AddSystem(platformSystem);

            // Apply velocity (movement must be AFTER physics and platform updates)
            systemManager.AddSystem(new MovementSystem());

            // Collision detection and resolution (after movement)
            var collisionSystem = new CollisionSystem();
            systemManager.AddSystem(collisionSystem);
            game.CollisionSystem = collisionSystem;

            // Camera
            var cameraSystem = new CameraSystem(new Vector2(
                game.GraphicsDeviceManager.PreferredBackBufferWidth,
                game.GraphicsDeviceManager.PreferredBackBufferHeight
            ));
            systemManager.AddSystem(cameraSystem);
            game.CameraSystem = cameraSystem;

            // Create camera
            var cameraEntity = world.CreateEntity();
            world.AddComponent(cameraEntity, new CameraComponent(
                initialPosition: new Vector2(640, 360),
                lagFactor: 0.1f,
                offset: Vector2.Zero,
                zoom: 1f,
                dampeningThreshold: 5f));

            // Create player (spawn on ground platform at Y=575 so player is standing on it)
            var player = world.CreateEntity();
            world.AddComponent(player, new PositionComponent { Value = new Vector2(200, 575) });
            world.AddComponent(player, new VelocityComponent { Value = Vector2.Zero });

            var physics = new PlatformerPhysicsComponent(500f, 1200f);
            physics.MaxAirJumps = 1; // Double jump
            world.AddComponent(player, physics);

            world.AddComponent(player, new ShapeComponent(
                ShapeComponent.ShapeType.Rectangle,
                Color.Blue,
                new Vector2(20, 30)));

            // Add ColliderComponent for collision detection
            world.AddComponent(player, new ColliderComponent(
                new Rectangle(0, 0, 20, 30),  // Match ShapeComponent size
                ColliderComponent.ColliderType.Dynamic
            ));

            // Add GroundedComponent for ground state tracking
            world.AddComponent(player, new GroundedComponent(false));

            world.AddComponent(player, new CameraTargetComponent { IsActive = true });

            // Mark as player for input system
            world.AddComponent(player, new PlatformDemoPlayerComponent());

            // Static ground platform
            CreateStaticPlatform(world, new Vector2(200, 600), 400, 20, Color.Green, false);

            // One-way platform (can jump through from below)
            CreateStaticPlatform(world, new Vector2(400, 500), 200, 15, Color.Yellow, true);

            // Linear horizontal moving platform
            var linearH = world.CreateEntity();
            world.AddComponent(linearH, new PositionComponent { Value = new Vector2(100, 400) });
            world.AddComponent(linearH, MovingPlatformComponent.CreateLinear(
                new Vector2(100, 400),
                new Vector2(400, 400),
                50f));
            world.AddComponent(linearH, new PlatformComponent(100f, 15f));
            world.AddComponent(linearH, new ColliderComponent(
                new Rectangle(0, 0, 100, 15),
                ColliderComponent.ColliderType.Static
            ));
            world.AddComponent(linearH, new ShapeComponent(
                ShapeComponent.ShapeType.Rectangle,
                Color.Orange,
                new Vector2(100, 15)));

            // Linear vertical moving platform
            var linearV = world.CreateEntity();
            world.AddComponent(linearV, new PositionComponent { Value = new Vector2(600, 200) });
            world.AddComponent(linearV, MovingPlatformComponent.CreateLinear(
                new Vector2(600, 200),
                new Vector2(600, 500),
                75f));
            world.AddComponent(linearV, new PlatformComponent(80f, 15f));
            world.AddComponent(linearV, new ColliderComponent(
                new Rectangle(0, 0, 80, 15),
                ColliderComponent.ColliderType.Static
            ));
            world.AddComponent(linearV, new ShapeComponent(
                ShapeComponent.ShapeType.Rectangle,
                Color.Purple,
                new Vector2(80, 15)));

            // Circular moving platform
            var circular = world.CreateEntity();
            var centerPoint = new Vector2(800, 350);
            var startPos = centerPoint + new Vector2(100, 0);
            world.AddComponent(circular, new PositionComponent { Value = startPos });
            world.AddComponent(circular, MovingPlatformComponent.CreateCircular(centerPoint, 100f, 1f));
            world.AddComponent(circular, new PlatformComponent(60f, 15f));
            world.AddComponent(circular, new ColliderComponent(
                new Rectangle(0, 0, 60, 15),
                ColliderComponent.ColliderType.Static
            ));
            world.AddComponent(circular, new ShapeComponent(
                ShapeComponent.ShapeType.Rectangle,
                Color.Cyan,
                new Vector2(60, 15)));

            // Circular center marker
            var circleCenter = world.CreateEntity();
            world.AddComponent(circleCenter, new PositionComponent { Value = centerPoint });
            world.AddComponent(circleCenter, new ShapeComponent(
                ShapeComponent.ShapeType.Circle,
                Color.White * 0.3f,
                new Vector2(5, 5)));

            // Waypoint platform (triangle path)
            var waypoints = new List<Vector2>
            {
                new Vector2(950, 550),
                new Vector2(1150, 550),
                new Vector2(1050, 400)
            };

            var waypoint = world.CreateEntity();
            world.AddComponent(waypoint, new PositionComponent { Value = waypoints[0] });
            world.AddComponent(waypoint, MovingPlatformComponent.CreateWaypoint(waypoints, 60f, 0.5f));
            world.AddComponent(waypoint, new PlatformComponent(70f, 15f));
            world.AddComponent(waypoint, new ColliderComponent(
                new Rectangle(0, 0, 70, 15),
                ColliderComponent.ColliderType.Static
            ));
            world.AddComponent(waypoint, new ShapeComponent(
                ShapeComponent.ShapeType.Rectangle,
                Color.Pink,
                new Vector2(70, 15)));

            // Waypoint markers
            foreach (var wp in waypoints)
            {
                var marker = world.CreateEntity();
                world.AddComponent(marker, new PositionComponent { Value = wp });
                world.AddComponent(marker, new ShapeComponent(
                    ShapeComponent.ShapeType.Circle,
                    Color.White * 0.3f,
                    new Vector2(5, 5)));
            }

            // Static platforms for parkour
            CreateStaticPlatform(world, new Vector2(150, 300), 80, 15, Color.Green, false);
            CreateStaticPlatform(world, new Vector2(250, 250), 80, 15, Color.Green, false);
            CreateStaticPlatform(world, new Vector2(350, 200), 80, 15, Color.Green, false);

            System.Console.WriteLine("PlatformDemo: Initialized with various platform types");
        }

        private void CreateStaticPlatform(World world, Vector2 position, float width, float height, Color color, bool oneWay)
        {
            var platform = world.CreateEntity();
            world.AddComponent(platform, new PositionComponent { Value = position });
            world.AddComponent(platform, new PlatformComponent(width, height, oneWay));

            // Add ColliderComponent for collision detection
            world.AddComponent(platform, new ColliderComponent(
                new Rectangle(0, 0, (int)width, (int)height),
                ColliderComponent.ColliderType.Static
            ));

            world.AddComponent(platform, new ShapeComponent(
                ShapeComponent.ShapeType.Rectangle,
                oneWay ? color * 0.7f : color,
                new Vector2(width, height)));
        }

        /// <summary>
        /// Marker component for the platform demo player
        /// </summary>
        private struct PlatformDemoPlayerComponent { }

        /// <summary>
        /// Custom input system for platform demo
        /// </summary>
        private class PlatformDemoInputSystem
        {
            private KeyboardState _previousKeyboardState;
            private PlatformerPhysicsSystem _physicsSystem;

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

                    // Horizontal movement
                    velocity.Value.X = 0;
                    if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
                        velocity.Value.X = -200f;
                    if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
                        velocity.Value.X = 200f;

                    world.AddComponent(entity, velocity);

                    // Jump
                    if ((keyboardState.IsKeyDown(Keys.Space) || keyboardState.IsKeyDown(Keys.W) ||
                         keyboardState.IsKeyDown(Keys.Up)) && !_previousKeyboardState.IsKeyDown(Keys.Space) &&
                        !_previousKeyboardState.IsKeyDown(Keys.W) && !_previousKeyboardState.IsKeyDown(Keys.Up))
                    {
                        _physicsSystem.Jump(world, entity);
                    }

                    // Release jump for variable height
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
}
