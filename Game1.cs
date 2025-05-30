// Game1.cs - Enhanced with debug system integration
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ECS_Example.Components;
using ECS_Example.Systems;
using System.Linq;

namespace ECS_Example
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;
        private World _world;
        private SystemManager _systemManager;
        private CameraSystem _cameraSystem;
        private LevelManagerSystem _levelManagerSystem;
        private DebugSystem _debugSystem;
        private LevelCollisionSystem _levelCollisionSystem;

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
            _systemManager = new SystemManager();

            // Initialize systems in the order they should run
            InitializeSystems();

            // Create camera entity
            CreateCameraEntity();

            base.Initialize();
        }

        private void InitializeSystems()
        {
            // Movement and input systems (run first)
            _systemManager.AddSystem(new PlayerMovementSystem());
            _systemManager.AddSystem(new JumpSystem());
            _systemManager.AddSystem(new InputSystem());

            // Physics systems (run in sequence)
            _systemManager.AddSystem(new GravitySystem());
            _systemManager.AddSystem(new MovementSystem());
            // Note: CollisionSystem will be added in LoadContent after debug system is created

            // Combat and health systems
            _systemManager.AddSystem(new DamageSystem());
            _systemManager.AddSystem(new HealthSystem());
            _systemManager.AddSystem(new AttackSystem());

            // Visual feedback systems
            _systemManager.AddSystem(new FlashSystem());
            _systemManager.AddSystem(new StunSystem());

            // Camera system (after movement)
            _cameraSystem = new CameraSystem(new Vector2(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight));
            _systemManager.AddSystem(_cameraSystem);

            // Level manager (handles loading)
            _levelManagerSystem = new LevelManagerSystem();

            // Level systems
            _systemManager.AddSystem(new LevelEntitySystem());
        }

        private void CreateCameraEntity()
        {
            var cameraEntity = _world.CreateEntity();
            _world.AddComponent(cameraEntity, new Camera(
                initialPosition: Vector2.Zero,
                lagFactor: 0.97f,
                offset: new Vector2(0, -50),
                zoom: 3.0f,
                dampeningThreshold: 5.0f
            ));
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("Default");

            // Initialize debug system with required dependencies
            _debugSystem = new DebugSystem(_spriteBatch, _font, _cameraSystem);

            // Add systems that need debug system now that it's available
            var patrolSystem = new PatrolSystem();
            _systemManager.AddSystem(patrolSystem);
            _systemManager.AddSystem(new CollisionSystem(_debugSystem));

            // Initialize level collision system with debug system
            _levelCollisionSystem = new LevelCollisionSystem(_debugSystem);
            _systemManager.AddSystem(_levelCollisionSystem);

            // Add render systems after graphics are initialized
            _systemManager.AddSystem(new LevelRenderSystem(_spriteBatch, _cameraSystem));
            var renderSystem = new RenderSystem(_spriteBatch, GraphicsDevice, _cameraSystem, _debugSystem);
            renderSystem.SetPatrolSystem(patrolSystem); // Connect patrol system for accurate debug visualization
            _systemManager.AddSystem(renderSystem);

            // Add debug system to system manager
            _systemManager.AddSystem(_debugSystem);

            // Load all levels into the world
            _levelManagerSystem.LoadAllLevels(_world, "../../..", GraphicsDevice);

            // Debug: Print system info
            _systemManager.PrintSystemInfo();
            _world.PrintComponentStats();
        }

        protected override void Update(GameTime gameTime)
        {
            // Handle global input
            if (GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Back))
            {
                Restart();
            }

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update all systems through the system manager
            _systemManager.Update(_world, deltaTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // Draw all systems through the system manager
            _systemManager.Draw(_world);

            base.Draw(gameTime);
        }
    }
}