using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.Movement.Systems;
using ECS_Base.Mechanics.Rendering.Components;
using ECS_Base.Mechanics.Rendering.Systems;
using ECS_Base.Mechanics.Camera.Components;
using ECS_Base.Mechanics.Camera.Systems;
using ECS_Base.Mechanics.Platformer.Components;
using ECS_Base.Mechanics.Platformer.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace ECS_Base.Demos
{
    /// <summary>
    /// Demo showcasing platform system with various platform types
    /// </summary>
    public class PlatformDemo : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;

        private World _world;
        private MovementSystem _movementSystem;
        private PlatformSystem _platformSystem;
        private PlatformerPhysicsSystem _physicsSystem;
        private RenderSystem _renderSystem;
        private CameraSystem _cameraSystem;

        private Entity _player;
        private Entity _camera;
        private KeyboardState _previousKeyboardState;

        public PlatformDemo()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _world = new World();
            _movementSystem = new MovementSystem();
            _platformSystem = new PlatformSystem();
            _physicsSystem = new PlatformerPhysicsSystem();
            _cameraSystem = new CameraSystem(new Vector2(1280, 720));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _renderSystem = new RenderSystem(_spriteBatch, GraphicsDevice, _cameraSystem);
            _font = Content.Load<SpriteFont>("Default");

            SetupDemo();
        }

        private void SetupDemo()
        {
            // Create camera
            _camera = _world.CreateEntity();
            _world.AddComponent(_camera, new CameraComponent(
                initialPosition: new Vector2(640, 360),
                lagFactor: 0.1f,
                offset: Vector2.Zero,
                zoom: 1f,
                dampeningThreshold: 5f));

            // Create player
            _player = _world.CreateEntity();
            _world.AddComponent(_player, new PositionComponent { Value = new Vector2(100, 100) });
            _world.AddComponent(_player, new VelocityComponent { Value = Vector2.Zero });

            var physics = new PlatformerPhysicsComponent(500f, 1200f);
            physics.MaxAirJumps = 1; // Double jump
            _world.AddComponent(_player, physics);

            _world.AddComponent(_player, new ShapeComponent(
                ShapeComponent.ShapeType.Rectangle,
                Color.Blue,
                new Vector2(20, 30)));

            // Static ground platform
            CreateStaticPlatform(new Vector2(200, 600), 400, 20, Color.Green, false);

            // One-way platform (can jump through from below)
            CreateStaticPlatform(new Vector2(400, 500), 200, 15, Color.Yellow, true);

            // Linear horizontal moving platform
            var linearH = _world.CreateEntity();
            _world.AddComponent(linearH, new PositionComponent { Value = new Vector2(100, 400) });
            _world.AddComponent(linearH, MovingPlatformComponent.CreateLinear(
                new Vector2(100, 400),
                new Vector2(400, 400),
                50f));
            _world.AddComponent(linearH, new PlatformComponent(100f, 15f));
            _world.AddComponent(linearH, new ShapeComponent(
                ShapeComponent.ShapeType.Rectangle,
                Color.Orange,
                new Vector2(100, 15)));

            // Linear vertical moving platform
            var linearV = _world.CreateEntity();
            _world.AddComponent(linearV, new PositionComponent { Value = new Vector2(600, 200) });
            _world.AddComponent(linearV, MovingPlatformComponent.CreateLinear(
                new Vector2(600, 200),
                new Vector2(600, 500),
                75f));
            _world.AddComponent(linearV, new PlatformComponent(80f, 15f));
            _world.AddComponent(linearV, new ShapeComponent(
                ShapeComponent.ShapeType.Rectangle,
                Color.Purple,
                new Vector2(80, 15)));

            // Circular moving platform
            var circular = _world.CreateEntity();
            var centerPoint = new Vector2(800, 350);
            var startPos = centerPoint + new Vector2(100, 0);
            _world.AddComponent(circular, new PositionComponent { Value = startPos });
            _world.AddComponent(circular, MovingPlatformComponent.CreateCircular(centerPoint, 100f, 1f));
            _world.AddComponent(circular, new PlatformComponent(60f, 15f));
            _world.AddComponent(circular, new ShapeComponent(
                ShapeComponent.ShapeType.Rectangle,
                Color.Cyan,
                new Vector2(60, 15)));

            // Circular center marker
            var circleCenter = _world.CreateEntity();
            _world.AddComponent(circleCenter, new PositionComponent { Value = centerPoint });
            _world.AddComponent(circleCenter, new ShapeComponent(
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

            var waypoint = _world.CreateEntity();
            _world.AddComponent(waypoint, new PositionComponent { Value = waypoints[0] });
            _world.AddComponent(waypoint, MovingPlatformComponent.CreateWaypoint(waypoints, 60f, 0.5f));
            _world.AddComponent(waypoint, new PlatformComponent(70f, 15f));
            _world.AddComponent(waypoint, new ShapeComponent(
                ShapeComponent.ShapeType.Rectangle,
                Color.Pink,
                new Vector2(70, 15)));

            // Waypoint markers
            foreach (var wp in waypoints)
            {
                var marker = _world.CreateEntity();
                _world.AddComponent(marker, new PositionComponent { Value = wp });
                _world.AddComponent(marker, new ShapeComponent(
                    ShapeComponent.ShapeType.Circle,
                    Color.White * 0.3f,
                    new Vector2(5, 5)));
            }

            // Static platforms for parkour
            CreateStaticPlatform(new Vector2(150, 300), 80, 15, Color.Green, false);
            CreateStaticPlatform(new Vector2(250, 250), 80, 15, Color.Green, false);
            CreateStaticPlatform(new Vector2(350, 200), 80, 15, Color.Green, false);
        }

        private void CreateStaticPlatform(Vector2 position, float width, float height, Color color, bool oneWay)
        {
            var platform = _world.CreateEntity();
            _world.AddComponent(platform, new PositionComponent { Value = position });
            _world.AddComponent(platform, new PlatformComponent(width, height, oneWay));
            _world.AddComponent(platform, new ShapeComponent(
                ShapeComponent.ShapeType.Rectangle,
                oneWay ? color * 0.7f : color,
                new Vector2(width, height)));
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var keyboardState = Keyboard.GetState();

            // Player input
            if (_world.TryGetComponent<VelocityComponent>(_player, out var velocity))
            {
                // Horizontal movement
                velocity.Value.X = 0;
                if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
                    velocity.Value.X = -200f;
                if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
                    velocity.Value.X = 200f;

                _world.AddComponent(_player, velocity);
            }

            // Jump
            if ((keyboardState.IsKeyDown(Keys.Space) || keyboardState.IsKeyDown(Keys.W) ||
                 keyboardState.IsKeyDown(Keys.Up)) && !_previousKeyboardState.IsKeyDown(Keys.Space) &&
                !_previousKeyboardState.IsKeyDown(Keys.W) && !_previousKeyboardState.IsKeyDown(Keys.Up))
            {
                _physicsSystem.Jump(_world, _player);
            }

            // Release jump for variable height
            if ((_previousKeyboardState.IsKeyDown(Keys.Space) && keyboardState.IsKeyUp(Keys.Space)) ||
                (_previousKeyboardState.IsKeyDown(Keys.W) && keyboardState.IsKeyUp(Keys.W)) ||
                (_previousKeyboardState.IsKeyDown(Keys.Up) && keyboardState.IsKeyUp(Keys.Up)))
            {
                _physicsSystem.ReleaseJump(_world, _player);
            }

            _previousKeyboardState = keyboardState;

            // Update systems
            _platformSystem.Update(_world, deltaTime);
            _physicsSystem.Update(_world, deltaTime);

            // Simple ground detection for demo (check if player is on any platform)
            if (_world.TryGetComponent<PositionComponent>(_player, out var playerPos))
            {
                bool onPlatform = false;

                foreach (var platformEntity in _world.Query<PlatformComponent, PositionComponent>())
                {
                    if (!_world.TryGetComponent<PlatformComponent>(platformEntity, out var platform))
                        continue;
                    if (!_world.TryGetComponent<PositionComponent>(platformEntity, out var platPos))
                        continue;

                    // Simple AABB collision check
                    float platformTop = platPos.Value.Y - platform.Height * 0.5f;
                    float platformBottom = platPos.Value.Y + platform.Height * 0.5f;
                    float platformLeft = platPos.Value.X - platform.Width * 0.5f;
                    float platformRight = platPos.Value.X + platform.Width * 0.5f;

                    float playerBottom = playerPos.Value.Y + 15; // Half height of player

                    if (playerPos.Value.X > platformLeft && playerPos.Value.X < platformRight &&
                        playerBottom >= platformTop && playerBottom <= platformTop + 5)
                    {
                        if (_world.TryGetComponent<VelocityComponent>(_player, out var pVel) && pVel.Value.Y >= 0)
                        {
                            // One-way check
                            if (!platform.OneWay || playerBottom <= platformTop + 2)
                            {
                                onPlatform = true;

                                // Snap to platform
                                playerPos.Value.Y = platformTop - 15;
                                _world.AddComponent(_player, playerPos);

                                // Reset vertical velocity
                                pVel.Value.Y = 0;
                                _world.AddComponent(_player, pVel);
                                break;
                            }
                        }
                    }
                }

                if (_world.TryGetComponent<PlatformerPhysicsComponent>(_player, out var pPhysics))
                {
                    _physicsSystem.SetGrounded(_world, _player, onPlatform);
                }
            }

            _movementSystem.Update(_world, deltaTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Render all entities with camera
            _renderSystem.Draw(_world);

            // Draw UI (no camera transform)
            _spriteBatch.Begin();

            // Draw instructions
            _spriteBatch.DrawString(_font, "Platform System Demo", new Vector2(10, 10), Color.White);
            _spriteBatch.DrawString(_font, "Arrow Keys / WASD: Move", new Vector2(10, 30), Color.White);
            _spriteBatch.DrawString(_font, "Space / W / Up: Jump (double jump available)", new Vector2(10, 50), Color.White);
            _spriteBatch.DrawString(_font, "Green: Static platforms", new Vector2(10, 80), Color.White);
            _spriteBatch.DrawString(_font, "Yellow: One-way platform (jump through)", new Vector2(10, 100), Color.White);
            _spriteBatch.DrawString(_font, "Orange: Linear horizontal platform", new Vector2(10, 120), Color.White);
            _spriteBatch.DrawString(_font, "Purple: Linear vertical platform", new Vector2(10, 140), Color.White);
            _spriteBatch.DrawString(_font, "Cyan: Circular platform", new Vector2(10, 160), Color.White);
            _spriteBatch.DrawString(_font, "Pink: Waypoint platform (triangle path)", new Vector2(10, 180), Color.White);

            // Draw player stats
            if (_world.TryGetComponent<PlatformerPhysicsComponent>(_player, out var physics))
            {
                string grounded = physics.IsGrounded ? "Yes" : "No";
                string airJumps = $"{physics.AirJumpsRemaining}/{physics.MaxAirJumps}";
                _spriteBatch.DrawString(_font, $"Grounded: {grounded}", new Vector2(10, 220), Color.Yellow);
                _spriteBatch.DrawString(_font, $"Air Jumps: {airJumps}", new Vector2(10, 240), Color.Yellow);
            }

            if (_world.TryGetComponent<VelocityComponent>(_player, out var vel))
            {
                _spriteBatch.DrawString(_font, $"Velocity: ({vel.Value.X:F1}, {vel.Value.Y:F1})",
                    new Vector2(10, 260), Color.Yellow);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
