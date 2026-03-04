// Game1.cs - Enhanced with debug system integration
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Camera.Systems;
using ECS_Base.Mechanics.Level.Systems;
using ECS_Base.Mechanics.Collision.Systems;
using ECS_Base.Mechanics.Rendering.Systems;
using ECS_Base.Mechanics.UI.Systems;
using ECS_Base.GameConfigs;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace ECS_Base
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;
        private World _world;
        private SystemManager _systemManager;
        private IGameConfig _config;
        private UISystem _uiSystem;
        private int _previousMouseWheel;

        // Public properties for systems that configs might need to set
        public CameraSystem CameraSystem { get; set; }
        public LevelManagerSystem LevelManagerSystem { get; set; }
        public CollisionSystem CollisionSystem { get; set; }
        public SpriteSystem SpriteSystem { get; set; }
        public GraphicsDeviceManager GraphicsDeviceManager => _graphics;

        public Game1(IGameConfig config)
        {
            _config = config;
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.Title = $"ECS-Base - {config.Name}";
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

            // Use the configuration to initialize systems and entities
            _config.Initialize(this, _world, _systemManager);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("Default");

            System.Console.WriteLine("=== LoadContent Started ===");

            // Add render systems if camera system exists
            if (CameraSystem != null)
            {
                System.Console.WriteLine("Adding render systems...");

                // Create and add sprite system if config requested it
                if (SpriteSystem != null)
                {
                    // Re-create with actual SpriteBatch
                    SpriteSystem = new SpriteSystem(_spriteBatch);
                    _systemManager.AddSystem(SpriteSystem);
                }

                var renderSystem = new RenderSystem(_spriteBatch, GraphicsDevice, CameraSystem);
                _systemManager.AddSystem(renderSystem);
                _systemManager.AddSystem(new LevelRenderSystem(_spriteBatch, CameraSystem));
                _systemManager.AddSystem(new WorldIconSystem(_spriteBatch, GraphicsDevice, CameraSystem));

                // UI should render after world systems
                _uiSystem = new UISystem(GraphicsDevice, _spriteBatch, _font);
                _systemManager.AddSystem(_uiSystem);
            }

            // Load levels if level manager exists (platformer config)
            if (LevelManagerSystem != null)
            {
                System.Console.WriteLine("Loading levels...");
                // Use absolute path from the application's base directory
                var levelsPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "../../../");
                LevelManagerSystem.LoadAllLevels(_world, levelsPath, GraphicsDevice);
            }

            // Debug info
            System.Console.WriteLine("=== System Info ===");
            _systemManager.PrintSystemInfo();
            System.Console.WriteLine("=== Component Stats ===");
            _world.PrintComponentStats();
            System.Console.WriteLine($"Total entities: {_world.GetEntities().Count}");

            // Write detailed entity info to debug file
            var debugPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "debug.txt");
            System.IO.File.AppendAllText(debugPath, $"\n=== After LoadContent ===\n");
            System.IO.File.AppendAllText(debugPath, $"Total entities: {_world.GetEntities().Count}\n");

            foreach (var entity in _world.GetEntities())
            {
                System.IO.File.AppendAllText(debugPath, $"\nEntity {entity.Id}:\n");

                if (_world.TryGetComponent<ECS_Base.Mechanics.Movement.Components.PositionComponent>(entity, out var pos))
                    System.IO.File.AppendAllText(debugPath, $"  Position: ({pos.Value.X}, {pos.Value.Y})\n");

                if (_world.TryGetComponent<ECS_Base.Mechanics.Rendering.Components.ShapeComponent>(entity, out var shape))
                    System.IO.File.AppendAllText(debugPath, $"  Shape: {shape.Type}, Color: {shape.Color}, Size: {shape.Size}\n");

                if (_world.TryGetComponent<ECS_Base.Mechanics.Camera.Components.CameraComponent>(entity, out var cam))
                    System.IO.File.AppendAllText(debugPath, $"  Camera: Pos({cam.Position.X}, {cam.Position.Y}), Zoom: {cam.Zoom}\n");

                if (_world.TryGetComponent<ECS_Base.Mechanics.Level.Components.LevelComponent>(entity, out var level))
                    System.IO.File.AppendAllText(debugPath, $"  Level: {level.LevelData.Identifier}\n");

                if (_world.TryGetComponent<ECS_Base.Mechanics.PlayerController.Components.PlayerComponent>(entity, out var player))
                    System.IO.File.AppendAllText(debugPath, $"  Player: Speed={player.MoveSpeed}, Jump={player.JumpForce}\n");
            }
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

            // Mouse wheel zoom for active camera
            var mouse = Mouse.GetState();
            int wheelDelta = mouse.ScrollWheelValue - _previousMouseWheel;
            if (wheelDelta != 0)
            {
                var cameraEntity = _world.GetEntities()
                    .FirstOrDefault(e => _world.TryGetComponent<ECS_Base.Mechanics.Camera.Components.CameraComponent>(e, out var cam) && cam.IsActive);

                if (cameraEntity != null && _world.TryGetComponent<ECS_Base.Mechanics.Camera.Components.CameraComponent>(cameraEntity, out var camComp))
                {
                    camComp.Zoom = MathHelper.Clamp(camComp.Zoom + (wheelDelta * 0.001f), 0.6f, 3.0f);
                    _world.AddComponent(cameraEntity, camComp);
                }
            }
            _previousMouseWheel = mouse.ScrollWheelValue;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update all systems through the system manager
            _systemManager.Update(_world, deltaTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // Update camera matrix for sprite system
            if (SpriteSystem != null && CameraSystem != null)
            {
                SpriteSystem.SetViewMatrix(CameraSystem.GetViewMatrix(_world));
            }

            // Draw all systems through the system manager
            _systemManager.Draw(_world);

            base.Draw(gameTime);
        }
    }
}
