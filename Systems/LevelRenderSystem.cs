// Systems/LevelRenderSystem.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ECS_Example.Components;

namespace ECS_Example.Systems
{
    public class LevelRenderSystem
    {
        private SpriteBatch _spriteBatch;
        private CameraSystem _cameraSystem;
        private const int TILE_SIZE = 16;

        public LevelRenderSystem(SpriteBatch spriteBatch, CameraSystem cameraSystem)
        {
            _spriteBatch = spriteBatch;
            _cameraSystem = cameraSystem;
        }

        public void Draw(World world)
        {
            var viewMatrix = _cameraSystem.GetViewMatrix(world);
            _spriteBatch.Begin(transformMatrix: viewMatrix, samplerState: SamplerState.PointClamp);

            foreach (var entity in world.GetEntities())
            {
                if (world.TryGetComponent<LevelComponent>(entity, out var levelComponent) &&
                    levelComponent.IsActive &&
                    levelComponent.LevelData.TileTexture != null)
                {
                    var level = levelComponent.LevelData;

                    // Render the full level image at its world position
                    _spriteBatch.Draw(
                        level.TileTexture,
                        new Vector2(level.X, level.Y),
                        Color.White
                    );
                }
            }

            _spriteBatch.End();
        }
    }
}