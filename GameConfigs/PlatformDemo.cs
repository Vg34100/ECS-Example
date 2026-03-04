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
using ECS_Base.Mechanics.Stats.Components;
using ECS_Base.Mechanics.Stats.Systems;
using ECS_Base.Mechanics.UI.Components;
using ECS_Base.Mechanics.Rendering.Systems;
using ECS_Base.Mechanics.Progression.Components;
using ECS_Base.Mechanics.Progression.Systems;
using ECS_Base.Mechanics.PlayerController.Components;
using ECS_Base.Mechanics.Combat.Components;
using ECS_Base.Mechanics.Combat.Systems;
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
            systemManager.AddSystem(new KnockbackSystem());

            // Stats (health, etc.)
            var statsSystem = new StatsSystem();
            systemManager.AddSystem(statsSystem);
            systemManager.AddSystem(new InvulnerabilitySystem());
            systemManager.AddSystem(new RespawnSystem());
            systemManager.AddSystem(new DamageFlashSystem());
            systemManager.AddSystem(new CollectibleSystem());

            // Platform movement
            var platformSystem = new PlatformSystem();
            systemManager.AddSystem(platformSystem);

            // Apply velocity (movement must be AFTER physics and platform updates)
            systemManager.AddSystem(new MovementSystem());

            // Collision detection and resolution (after movement)
            var collisionSystem = new CollisionSystem();
            systemManager.AddSystem(collisionSystem);
            game.CollisionSystem = collisionSystem;
            systemManager.AddSystem(new ContactDamageSystem(statsSystem));

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
                lagFactor: 0.95f,
                offset: Vector2.Zero,
                zoom: 1f,
                dampeningThreshold: 2f));

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
            world.AddComponent(player, new PlayerComponent());

            // Stats for player (3-hit health)
            world.AddComponent(player, new StatsComponent(maxHealth: 3, attack: 10f, defense: 0f, speed: 1f));
            world.AddComponent(player, new InvulnerabilityOnHitComponent(duration: 1.0f));
            world.AddComponent(player, new DamageFlashOnHitComponent(flashColor: Color.White, duration: 0.4f));
            world.AddComponent(player, new RespawnComponent(new Vector2(200, 575), delay: 2.5f));
            world.AddComponent(player, new CurrencyComponent());

            // UI: segmented bar (Mario-style)
            var ui = world.CreateEntity();
            world.AddComponent(ui, new UISegmentedBarComponent(
                position: new Vector2(16, 16),
                targetEntityId: player.Id,
                segments: 3
            ));

            // UI: counters
            var coinsText = world.CreateEntity();
            world.AddComponent(coinsText, new UITextComponent(null, "Coins: 0", new Vector2(16, 32)));
            world.AddComponent(coinsText, new UICounterComponent(player.Id, CounterType.Coins, "Coins: "));
            world.AddComponent(coinsText, new UIIconComponent(new Vector2(16, 32), UIIconType.Coin, Color.Gold, 2));

            var starsText = world.CreateEntity();
            world.AddComponent(starsText, new UITextComponent(null, "Stars: 0", new Vector2(16, 48)));
            world.AddComponent(starsText, new UICounterComponent(player.Id, CounterType.Stars, "Stars: "));
            world.AddComponent(starsText, new UIIconComponent(new Vector2(16, 48), UIIconType.Star, Color.Yellow, 2));

            var scoreText = world.CreateEntity();
            world.AddComponent(scoreText, new UITextComponent(null, "Score: 0", new Vector2(16, 64)));
            world.AddComponent(scoreText, new UICounterComponent(player.Id, CounterType.Score, "Score: "));

            // Static ground platform
            CreateStaticPlatform(world, new Vector2(0, 600), 1400, 20, Color.Green, false);
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

            // Hazard spikes (contact damage)
            CreateHazard(world, new Vector2(820, 565), 40, 18, Color.Red);

            System.Console.WriteLine("PlatformDemo: Initialized with various platform types");

            // Collectibles
            SpawnCoin(world, new Vector2(260, 540));
            SpawnCoin(world, new Vector2(310, 540));
            SpawnCoin(world, new Vector2(360, 540));
            SpawnStar(world, new Vector2(600, 180));
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

        private void SpawnCoin(World world, Vector2 position)
        {
            var coin = world.CreateEntity();
            world.AddComponent(coin, new PositionComponent { Value = position });
            world.AddComponent(coin, new CollectibleComponent(CollectibleType.Coin, amount: 1, scoreValue: 100));
            world.AddComponent(coin, new ColliderComponent(
                new Rectangle(0, 0, 10, 10),
                ColliderComponent.ColliderType.Dynamic
            ));
            world.AddComponent(coin, new ShapeComponent(
                ShapeComponent.ShapeType.Circle,
                Color.Gold,
                new Vector2(10, 10)
            ));
        }

        private void SpawnStar(World world, Vector2 position)
        {
            var star = world.CreateEntity();
            world.AddComponent(star, new PositionComponent { Value = position });
            world.AddComponent(star, new CollectibleComponent(CollectibleType.Star, amount: 1, scoreValue: 1000));
            world.AddComponent(star, new ColliderComponent(
                new Rectangle(0, 0, 12, 12),
                ColliderComponent.ColliderType.Dynamic
            ));
            world.AddComponent(star, new ShapeComponent(
                ShapeComponent.ShapeType.Circle,
                Color.Yellow,
                new Vector2(12, 12)
            ));
        }

        private void CreateHazard(World world, Vector2 position, int width, int height, Color color)
        {
            // Solid spike block
            var solid = world.CreateEntity();
            world.AddComponent(solid, new PositionComponent { Value = position });
            world.AddComponent(solid, new ColliderComponent(
                new Rectangle(0, 0, width, height),
                ColliderComponent.ColliderType.Static
            ));
            world.AddComponent(solid, new ShapeComponent(
                ShapeComponent.ShapeType.Rectangle,
                color,
                new Vector2(width, height)
            ));

            // Separate damage zone slightly above the solid surface
            var damage = world.CreateEntity();
            world.AddComponent(damage, new PositionComponent { Value = new Vector2(position.X + 6, position.Y - 2) });
            world.AddComponent(damage, new ColliderComponent(
                new Rectangle(0, 0, width - 12, height - 8),
                ColliderComponent.ColliderType.Dynamic
            ));
            world.AddComponent(damage, new ContactDamageComponent(damage: 1));
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

                    if (world.TryGetComponent<StatsComponent>(entity, out var stats) && stats.IsDead)
                    {
                        velocity.Value = Vector2.Zero;
                        world.AddComponent(entity, velocity);
                        continue;
                    }

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
