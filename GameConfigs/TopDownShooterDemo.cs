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
using ECS_Base.Mechanics.Rendering.Components;
using ECS_Base.Mechanics.Combat.Components;
using ECS_Base.Mechanics.Combat.Systems;
using ECS_Base.Mechanics.Stats.Components;
using ECS_Base.Mechanics.Stats.Systems;
using ECS_Base.Mechanics.UI.Components;
using ECS_Base.Mechanics.Progression.Components;
using ECS_Base.Mechanics.Progression.Systems;
using ECS_Base.Mechanics.Rendering.Systems;

namespace ECS_Base.GameConfigs
{
    /// <summary>
    /// Top-down shooter demo (Isaac-style)
    /// - WASD movement (8-directional, no gravity)
    /// - Space to shoot projectiles in movement direction
    /// - Enemies that chase you
    /// - Health system
    /// </summary>
    public class TopDownShooterDemo : IGameConfig
    {
        public string Name => "Top-Down Shooter Demo";

        public void Initialize(Game1 game, World world, SystemManager systemManager)
        {
            // Systems in order
            systemManager.AddSystem(new InputSystem());
            systemManager.AddSystem(new TopDownMovementSystem()); // Top-down instead of platformer
            systemManager.AddSystem(new EnemyAISystem());
            systemManager.AddSystem(new ProjectileSystem());
            systemManager.AddSystem(new MovementSystem());

            // Collision systems
            var collisionSystem = new CollisionSystem();
            systemManager.AddSystem(collisionSystem);
            game.CollisionSystem = collisionSystem;
            systemManager.AddSystem(new SweptCollisionSystem()); // For projectiles

            var statsSystem = new StatsSystem();
            systemManager.AddSystem(statsSystem);
            systemManager.AddSystem(new InvulnerabilitySystem());
            systemManager.AddSystem(new RespawnSystem());
            systemManager.AddSystem(new DamageFlashSystem());
            systemManager.AddSystem(new CombatSystem(statsSystem));
            systemManager.AddSystem(new CollectibleSystem());
            systemManager.AddSystem(new ContactDamageSystem(statsSystem));

            var cameraSystem = new CameraSystem(new Vector2(
                game.GraphicsDeviceManager.PreferredBackBufferWidth,
                game.GraphicsDeviceManager.PreferredBackBufferHeight
            ));
            systemManager.AddSystem(cameraSystem);
            game.CameraSystem = cameraSystem;

            // Camera
            var cameraEntity = world.CreateEntity();
            world.AddComponent(cameraEntity, new CameraComponent(
                initialPosition: Vector2.Zero,
                lagFactor: 0.90f,
                offset: Vector2.Zero,
                zoom: 2.5f,
                dampeningThreshold: 5.0f
            ));

            // Create walls (room boundaries)
            CreateWall(world, 100, 100, 600, 16, Color.Gray); // Top
            CreateWall(world, 100, 100, 16, 400, Color.Gray); // Left
            CreateWall(world, 684, 100, 16, 400, Color.Gray); // Right
            CreateWall(world, 100, 484, 600, 16, Color.Gray); // Bottom

            // Some obstacles
            CreateWall(world, 250, 200, 80, 80, Color.DarkGray);
            CreateWall(world, 450, 300, 100, 60, Color.DarkGray);

            // Player
            var player = world.CreateEntity();
            world.AddComponent(player, new PositionComponent(350, 250));
            world.AddComponent(player, new VelocityComponent());
            world.AddComponent(player, new TopDownMovementComponent(moveSpeed: 100f));
            world.AddComponent(player, new InputComponent());
            world.AddComponent(player, new ColliderComponent(
                new Rectangle(0, 0, 12, 16),
                ColliderComponent.ColliderType.Dynamic
            ));
            world.AddComponent(player, new ShapeComponent(
                ShapeComponent.ShapeType.Rectangle,
                Color.Blue,
                new Vector2(12, 16)
            ));
            world.AddComponent(player, new PlayerComponent()); // Marker
            world.AddComponent(player, new StatsComponent(maxHealth: 6, attack: 10f, defense: 0f, speed: 1f));
            world.AddComponent(player, new InvulnerabilityOnHitComponent(duration: 0.8f));
            world.AddComponent(player, new DamageFlashOnHitComponent(flashColor: Color.White, duration: 0.35f));
            world.AddComponent(player, new RespawnComponent(new Vector2(350, 250), delay: 2.0f));
            world.AddComponent(player, new CurrencyComponent());
            world.AddComponent(player, new CameraTargetComponent { IsActive = true });

            // UI: hearts (3 hearts with half-health units)
            var ui = world.CreateEntity();
            world.AddComponent(ui, new UIHeartsComponent(
                position: new Vector2(16, 16),
                targetEntityId: player.Id,
                unitsPerHeart: 2,
                heartSize: 16,
                heartSpacing: 4
            ));

            var rupeesText = world.CreateEntity();
            world.AddComponent(rupeesText, new UITextComponent(null, "Rupees: 0", new Vector2(16, 36)));
            world.AddComponent(rupeesText, new UICounterComponent(player.Id, CounterType.Rupees, "Rupees: "));

            // Spawn melee enemies (chase only)
            SpawnEnemy(world, 200, 150, canShoot: false);
            SpawnEnemy(world, 550, 350, canShoot: false);

            // Spawn shooter enemies (shoot at player)
            SpawnEnemy(world, 500, 150, canShoot: true);
            SpawnEnemy(world, 300, 400, canShoot: true);

            System.Console.WriteLine("=== Top-Down Shooter Demo ===");
            System.Console.WriteLine("WASD: Move (8-directional)");
            System.Console.WriteLine("SPACE (hold): Shoot in movement direction");
            System.Console.WriteLine("Red enemies: melee (chase only)");
            System.Console.WriteLine("Orange enemies: ranged (shoot red projectiles)");
            System.Console.WriteLine("Kill all enemies!");

            // Rupees (currency)
            SpawnRupee(world, new Vector2(160, 160));
            SpawnRupee(world, new Vector2(620, 140));
            SpawnRupee(world, new Vector2(620, 420));
        }

        private void CreateWall(World world, float x, float y, int width, int height, Color color)
        {
            var wall = world.CreateEntity();
            world.AddComponent(wall, new PositionComponent(x, y));
            world.AddComponent(wall, new ColliderComponent(
                new Rectangle(0, 0, width, height),
                ColliderComponent.ColliderType.Static
            ));
            world.AddComponent(wall, new ShapeComponent(
                ShapeComponent.ShapeType.Rectangle,
                color,
                new Vector2(width, height)
            ));
        }

        private void SpawnEnemy(World world, float x, float y, bool canShoot)
        {
            var enemy = world.CreateEntity();
            world.AddComponent(enemy, new PositionComponent(x, y));
            world.AddComponent(enemy, new VelocityComponent());
            world.AddComponent(enemy, new EnemyComponent(
                chaseSpeed: canShoot ? 40f : 60f, // Shooters move slower
                chaseRange: 300f,
                stopDistance: canShoot ? 120f : 30f, // Shooters keep distance
                separationRadius: 25f,
                canShoot: canShoot,
                shootRange: 180f,
                shootCooldown: 2.0f
            ));
            world.AddComponent(enemy, new ColliderComponent(
                new Rectangle(0, 0, 14, 14),
                ColliderComponent.ColliderType.Dynamic
            ));
            world.AddComponent(enemy, new ShapeComponent(
                ShapeComponent.ShapeType.Rectangle,
                canShoot ? Color.Orange : Color.Red, // Orange for shooters
                new Vector2(14, 14)
            ));
            world.AddComponent(enemy, new StatsComponent(maxHealth: 30, attack: 5f, defense: 0f, speed: 1f));
            world.AddComponent(enemy, new ContactDamageComponent(damage: 1));
        }

        private void SpawnRupee(World world, Vector2 position)
        {
            var rupee = world.CreateEntity();
            world.AddComponent(rupee, new PositionComponent(position.X, position.Y));
            world.AddComponent(rupee, new CollectibleComponent(CollectibleType.Rupee, amount: 1, scoreValue: 50));
            world.AddComponent(rupee, new ColliderComponent(
                new Rectangle(0, 0, 8, 12),
                ColliderComponent.ColliderType.Dynamic
            ));
            world.AddComponent(rupee, new ShapeComponent(
                ShapeComponent.ShapeType.Rectangle,
                Color.LawnGreen,
                new Vector2(8, 12)
            ));
        }
    }
}
