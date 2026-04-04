using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Combat.Components;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.Rendering.Components;
using ECS_Base.Mechanics.Stats.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace ECS_Base.Mechanics.Rendering.Systems
{
    /// <summary>
    /// Initializes and updates slime sprite animations for top-down chaser enemies.
    /// </summary>
    public class TopdownSlimeSpriteSystem
    {
        private readonly GraphicsDevice _graphicsDevice;
        private Texture2D _slimeTexture;
        private const int FrameSize = 16;

        public TopdownSlimeSpriteSystem(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        public void Update(World world, float deltaTime)
        {
            EnsureTextureLoaded();
            if (_slimeTexture == null)
                return;

            foreach (var entity in world.Query<TopdownSlimeVisualComponent, PositionComponent>())
            {
                InitializeSprite(world, entity);
                UpdateSpriteState(world, entity, deltaTime);
            }
        }

        private void EnsureTextureLoaded()
        {
            if (_slimeTexture != null)
                return;

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Assets/Sprites/slime.png");
            if (!File.Exists(path))
                return;

            using var stream = File.OpenRead(path);
            _slimeTexture = Texture2D.FromStream(_graphicsDevice, stream);
        }

        private void InitializeSprite(World world, Entity entity)
        {
            if (!world.TryGetComponent<SpriteComponent>(entity, out _))
            {
                var sprite = new SpriteComponent(_slimeTexture, new Rectangle(0, 0, FrameSize, FrameSize))
                {
                    Layer = 20,
                    Origin = Vector2.Zero,
                    Scale = Vector2.One
                };
                world.AddComponent(entity, sprite);
            }

            if (world.HasComponent<ShapeComponent>(entity))
            {
                world.RemoveComponent<ShapeComponent>(entity);
            }

            if (world.HasComponent<Mechanics.Animation.Components.AnimationComponent>(entity))
            {
                world.RemoveComponent<Mechanics.Animation.Components.AnimationComponent>(entity);
            }
        }

        private void UpdateSpriteState(World world, Entity entity, float deltaTime)
        {
            if (!world.TryGetComponent<TopdownSlimeVisualComponent>(entity, out var slime))
                return;
            if (!world.TryGetComponent<SpriteComponent>(entity, out var sprite))
                return;

            var nextFacing = slime.Facing;
            bool isMoving = false;
            if (world.TryGetComponent<VelocityComponent>(entity, out var velocity) && velocity.Value.LengthSquared() > 1f)
            {
                nextFacing = GetFacing(velocity.Value);
                isMoving = velocity.Value.LengthSquared() > 4f;
            }

            slime.Facing = nextFacing;

            int column = nextFacing switch
            {
                SlimeFacing.Right => 1,
                SlimeFacing.Left => 2,
                SlimeFacing.Up => 3,
                _ => 0
            };

            int row;

            if (world.TryGetComponent<StatsComponent>(entity, out var stats) && stats.IsDead)
            {
                row = 3;
            }
            else if (world.HasComponent<DamageFlashComponent>(entity))
            {
                row = 2;
            }
            else
            {
                bool isLunging = world.TryGetComponent<SlimeMovementComponent>(entity, out var slimeMove) &&
                                 slimeMove.HopTimeRemaining > 0f;
                row = isLunging ? 1 : 0;
            }

            sprite.SourceRectangle = new Rectangle(column * FrameSize, row * FrameSize, FrameSize, FrameSize);

            world.AddComponent(entity, slime);
            world.AddComponent(entity, sprite);
        }

        private static SlimeFacing GetFacing(Vector2 velocity)
        {
            if (Math.Abs(velocity.X) > Math.Abs(velocity.Y))
                return velocity.X >= 0 ? SlimeFacing.Right : SlimeFacing.Left;

            return velocity.Y >= 0 ? SlimeFacing.Down : SlimeFacing.Up;
        }
    }
}
