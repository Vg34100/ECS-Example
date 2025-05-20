// Game1.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ECS_Example.Components;
using ECS_Example.Systems;
using ECS_Example.Factories;
using System.Linq;

namespace ECS_Example
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private World _world;
        private RenderSystem _renderSystem;

        private MovementSystem _movementSystem;
        private InputSystem _inputSystem;

        private GravitySystem _gravitySystem;
        private CollisionSystem _collisionSystem;

        private PlayerMovementSystem _playerMovementSystem;
        private JumpSystem _jumpSystem;

        // v-patrol
        private PatrolSystem _patrolSystem;

        // v-health
        private HealthSystem _healthSystem;
        private DamageSystem _damageSystem;

        // v-feedback
        private FlashSystem _flashSystem;
        private StunSystem _stunSystem;

        // attack
        private AttackSystem _attackSystem;
        // controller
        private float _deadZoneThreshold = 0.2f;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        private void Restart()
        {
            // Clear existing entities
            var entities = _world.GetEntities().ToList();
            foreach (var entity in entities)
            {
                _world.RemoveEntity(entity);
            }
            Initialize();
        }

        protected override void Initialize()
        {
            _world = new World();

            var playerEntity = PlayerFactory.CreatePlayer(_world, new Vector2(100, 300));

            // Create the ground
            var groundEntity = _world.CreateEntity();
            _world.AddComponent(groundEntity, new Position(0, 450));
            _world.AddComponent(groundEntity, new Shape(
                Shape.ShapeType.Rectangle,
                Color.Brown,
                new Vector2(800, 50)
            ));
            _world.AddComponent(groundEntity, new Collider(
                new Rectangle(0, 0, 800, 50),
                Collider.ColliderType.Static
            ));

            // Create some platforms
            CreatePlatform(200, 350, 100, 20);
            CreatePlatform(400, 250, 150, 20);
            CreatePlatform(600, 350, 100, 20);

            CreatePlatform(200, 425, 100, 20);

            CreatePlatform(600, 425, 100, 20);

            // Goomba-style enemy (will walk off edges) - Red
            EnemyFactory.CreatePatrolEnemy(_world, new Vector2(300, 400), 100, false, Color.Red, 20);

            // Koopa-style enemy (avoids edges) - Orange
            EnemyFactory.CreatePatrolEnemy(_world, new Vector2(200, 300), 100, true, Color.Orange);
            EnemyFactory.CreatePatrolEnemy(_world, new Vector2(400, 200), 100, true, Color.Orange);

            // Initialize systems
            _playerMovementSystem = new PlayerMovementSystem();
            _jumpSystem = new JumpSystem();

            // Initialize the patrol system
            _patrolSystem = new PatrolSystem();

            _movementSystem = new MovementSystem();
            _inputSystem = new InputSystem();
            _gravitySystem = new GravitySystem();
            _collisionSystem = new CollisionSystem();

            _healthSystem = new HealthSystem();
            _damageSystem = new DamageSystem();

            _flashSystem = new FlashSystem();
            _stunSystem = new StunSystem();

            _attackSystem = new AttackSystem();

            base.Initialize();
        }

        private void CreatePlatform(float x, float y, float width, float height)
        {
            var platform = _world.CreateEntity();
            _world.AddComponent(platform, new Position(x, y));
            _world.AddComponent(platform, new Shape(
                Shape.ShapeType.Rectangle,
                Color.Gray,
                new Vector2(width, height)
            ));
            _world.AddComponent(platform, new Collider(
                new Rectangle(0, 0, (int)width, (int)height),
                Collider.ColliderType.Static
            ));
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _renderSystem = new RenderSystem(_spriteBatch, GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Back))
            {
                Restart();
            }

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _playerMovementSystem.Update(_world);
            _jumpSystem.Update(_world);

            _patrolSystem.Update(_world);

            _inputSystem.Update(_world);
            _gravitySystem.Update(_world, deltaTime);
            _movementSystem.Update(_world, deltaTime);
            _collisionSystem.Update(_world);

            _damageSystem.Update(_world);  // Check for damage after physics
            _healthSystem.Update(_world, deltaTime);  // Handle health and death

            _flashSystem.Update(_world, deltaTime);
            _stunSystem.Update(_world, deltaTime);

            _attackSystem.Update(_world, deltaTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _renderSystem.Draw(_world);

            base.Draw(gameTime);
        }
    }
}