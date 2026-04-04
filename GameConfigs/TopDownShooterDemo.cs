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
using ECS_Base.Mechanics.Animation.Systems;

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
            game.SpriteSystem = new SpriteSystem(null);
            // Systems in order
            systemManager.AddSystem(new InputSystem());
            systemManager.AddSystem(new TopDownMovementSystem()); // Top-down instead of platformer
            systemManager.AddSystem(new EnemyAISystem());
            systemManager.AddSystem(new TopdownSlimeSpriteSystem(game.GraphicsDevice));
            systemManager.AddSystem(new AnimationSystem());
            var statsSystem = new StatsSystem();
            systemManager.AddSystem(statsSystem);
            var meleeSystem = new MeleeCombatSystem(statsSystem);
            systemManager.AddSystem(new SwordSystem(meleeSystem));
            systemManager.AddSystem(meleeSystem);
            systemManager.AddSystem(new ShieldSystem());
            systemManager.AddSystem(new ShieldVisualSystem());
            systemManager.AddSystem(new BowVisualSystem());
            systemManager.AddSystem(new BombSystem(statsSystem));
            systemManager.AddSystem(new ProjectileSystem());
            systemManager.AddSystem(new KnockbackSystem());
            systemManager.AddSystem(new MovementSystem());
            systemManager.AddSystem(new FloatingSystem());
            systemManager.AddSystem(new ExplosionSystem());
            systemManager.AddSystem(new FacingIndicatorSystem());

            // Collision systems
            var collisionSystem = new CollisionSystem();
            systemManager.AddSystem(collisionSystem);
            game.CollisionSystem = collisionSystem;
            systemManager.AddSystem(new SweptCollisionSystem()); // For projectiles

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
            world.AddComponent(player, new AmmoComponent(arrows: 20, bombs: 3));
            world.AddComponent(player, new MeleeWeaponComponent(damage: 2f, range: 36f, attackSpeed: 0.4f));
            if (world.TryGetComponent<MeleeWeaponComponent>(player, out var sword))
            {
                sword.Knockback = 120f;
                world.AddComponent(player, sword);
            }
            world.AddComponent(player, new ShieldComponent(damageMultiplier: 0f));
            world.AddComponent(player, new CameraTargetComponent { IsActive = true });

            // UI: hearts (3 hearts with half-health units)
            var ui = world.CreateEntity();
            world.AddComponent(ui, new UIHeartsComponent(
                position: new Vector2(48, 16),
                targetEntityId: player.Id,
                unitsPerHeart: 2,
                heartSize: 16,
                heartSpacing: 4
            ));

            var rupeesText = world.CreateEntity();
            world.AddComponent(rupeesText, new UITextComponent(null, "Rupees: 0", new Vector2(16, 36)));
            world.AddComponent(rupeesText, new UICounterComponent(player.Id, CounterType.Rupees, "Rupees: "));
            world.AddComponent(rupeesText, new UIIconComponent(new Vector2(16, 36), UIIconType.Rupee, Color.LawnGreen, 2));

            var arrowsText = world.CreateEntity();
            world.AddComponent(arrowsText, new UITextComponent(null, "0", new Vector2(36, 56)));
            world.AddComponent(arrowsText, new UICounterComponent(player.Id, CounterType.Arrows, ""));
            world.AddComponent(arrowsText, new UIIconComponent(new Vector2(16, 56), UIIconType.Arrow, Color.White, 2));

            var bombsText = world.CreateEntity();
            world.AddComponent(bombsText, new UITextComponent(null, "0", new Vector2(36, 72)));
            world.AddComponent(bombsText, new UICounterComponent(player.Id, CounterType.Bombs, ""));
            world.AddComponent(bombsText, new UIIconComponent(new Vector2(16, 72), UIIconType.Bomb, Color.Black, 2));

            // Action indicators
            var swordIcon = world.CreateEntity();
            world.AddComponent(swordIcon, new UIIconComponent(new Vector2(200, 16), UIIconType.Star, Color.White, 2));
            world.AddComponent(swordIcon, new UIActionIndicatorComponent(player.Id, ActionType.Sword, Color.White, new Color(80, 80, 80)));

            var shieldIcon = world.CreateEntity();
            world.AddComponent(shieldIcon, new UIIconComponent(new Vector2(220, 16), UIIconType.Star, Color.White, 2));
            world.AddComponent(shieldIcon, new UIActionIndicatorComponent(player.Id, ActionType.Shield, Color.Cyan, new Color(80, 80, 80)));

            var bowIcon = world.CreateEntity();
            world.AddComponent(bowIcon, new UIIconComponent(new Vector2(240, 16), UIIconType.Arrow, Color.White, 2));
            world.AddComponent(bowIcon, new UIActionIndicatorComponent(player.Id, ActionType.Bow, Color.Yellow, new Color(80, 80, 80)));

            // Facing indicator (world)
            var facing = world.CreateEntity();
            world.AddComponent(facing, new PositionComponent(0, 0));
            world.AddComponent(facing, new WorldIconComponent(UIIconType.Arrow, Color.White, 1));
            world.AddComponent(facing, new FacingIndicatorComponent(player.Id, distance: 16f, offset: Vector2.Zero));

            // Shield visual (world)
            var shield = world.CreateEntity();
            world.AddComponent(shield, new PositionComponent(0, 0));
            world.AddComponent(shield, new ShapeComponent(ShapeComponent.ShapeType.Rectangle, Color.Transparent, new Vector2(28, 8)));
            world.AddComponent(shield, new ShieldVisualComponent(player.Id, new Vector2(28, 8), distance: 12f, color: new Color(120, 200, 255, 160)));

            // Bow visual (world)
            var bow = world.CreateEntity();
            world.AddComponent(bow, new PositionComponent(0, 0));
            world.AddComponent(bow, new ShapeComponent(ShapeComponent.ShapeType.Rectangle, Color.Transparent, new Vector2(12, 4)));
            world.AddComponent(bow, new BowVisualComponent(player.Id, new Vector2(12, 4), distance: 10f, timeRemaining: 0f, color: new Color(255, 220, 120, 180)));

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
                chaseSpeed: canShoot ? 30f : 28f, // Slimes move slower than normal chasers
                chaseRange: 300f,
                stopDistance: canShoot ? 90f : 12f, // Shooters keep distance
                separationRadius: canShoot ? 25f : 0f,
                canShoot: canShoot,
                shootRange: 180f,
                shootCooldown: 3.0f
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
            if (!canShoot)
            {
                world.AddComponent(enemy, new TopdownSlimeVisualComponent());
                world.AddComponent(enemy, new SlimeMovementComponent(hopDuration: 0.20f, pauseDuration: 0.35f, lungeSpeedMultiplier: 3.0f));
            }
            world.AddComponent(enemy, new StatsComponent(maxHealth: 6, attack: 5f, defense: 0f, speed: 1f));
            world.AddComponent(enemy, new ContactDamageComponent(damage: 1));
            world.AddComponent(enemy, new DamageFlashOnHitComponent(flashColor: Color.White, duration: 0.25f));
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
            world.AddComponent(rupee, new WorldIconComponent(UIIconType.Rupee, Color.LawnGreen, 2));
            world.AddComponent(rupee, new FloatingComponent(position, amplitude: 2.5f, speed: 2.0f));
        }
    }
}
