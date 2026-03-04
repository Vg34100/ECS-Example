using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Camera.Systems;
using ECS_Base.Mechanics.Level.Components;
using System.Linq;

namespace ECS_Base.Mechanics.Level.Systems
{
    public class LevelRenderSystem
    {
        private SpriteBatch _spriteBatch;
        private CameraSystem _cameraSystem;
        private const int TILE_SIZE = 16;
        private Texture2D _pixel;

        public LevelRenderSystem(SpriteBatch spriteBatch, CameraSystem cameraSystem)
        {
            _spriteBatch = spriteBatch;
            _cameraSystem = cameraSystem;
            _pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public void Draw(World world)
        {
            var viewMatrix = _cameraSystem.GetViewMatrix(world);
            _spriteBatch.Begin(transformMatrix: viewMatrix, samplerState: SamplerState.PointClamp);

            var useIntGrid = world.Query<LevelTilesetConfigComponent>().Any(cfg =>
            {
                var c = world.GetComponent<LevelTilesetConfigComponent>(cfg);
                return c.UseIntGridRender;
            });

            LevelTileColorConfigComponent? colorConfig = null;
            foreach (var entity in world.Query<LevelTileColorConfigComponent>())
            {
                colorConfig = world.GetComponent<LevelTileColorConfigComponent>(entity);
                break;
            }

            LevelTilesetComponent? tileset = null;
            foreach (var entity in world.Query<LevelTilesetComponent>())
            {
                tileset = world.GetComponent<LevelTilesetComponent>(entity);
                break;
            }

            foreach (var entity in world.GetEntities())
            {
                if (world.TryGetComponent<LevelComponent>(entity, out var levelComponent) &&
                    levelComponent.IsActive &&
                    levelComponent.LevelData != null)
                {
                    var level = levelComponent.LevelData;

                    if (colorConfig.HasValue && level.TileData != null)
                    {
                        DrawColorGrid(level, colorConfig.Value);
                    }
                    else if (useIntGrid && tileset.HasValue && level.TileData != null)
                    {
                        DrawIntGrid(level, tileset.Value);
                    }
                    else if (level.TileTexture != null)
                    {
                        _spriteBatch.Draw(
                            level.TileTexture,
                            new Vector2(level.X, level.Y),
                            Color.White
                        );
                    }
                }
            }

            _spriteBatch.End();
        }

        private void DrawIntGrid(ECS_Base.LevelData.Level level, LevelTilesetComponent tileset)
        {
            int rows = level.TileData.GetLength(0);
            int cols = level.TileData.GetLength(1);

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int value = level.TileData[row, col];
                    if (value == 0)
                        continue;

                    if (!tileset.TileRects.TryGetValue(value, out var src))
                        continue;

                    var dest = new Rectangle(
                        level.X + (col * tileset.TileSize),
                        level.Y + (row * tileset.TileSize),
                        tileset.TileSize,
                        tileset.TileSize
                    );

                    _spriteBatch.Draw(tileset.TilesetTexture, dest, src, Color.White);
                }
            }
        }

        private void DrawColorGrid(ECS_Base.LevelData.Level level, LevelTileColorConfigComponent colorConfig)
        {
            int rows = level.TileData.GetLength(0);
            int cols = level.TileData.GetLength(1);

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int value = level.TileData[row, col];
                    if (value == 0)
                        continue;

                    var color = colorConfig.DefaultColor;
                    if (colorConfig.TileColors != null && colorConfig.TileColors.TryGetValue(value, out var c))
                        color = c;

                    var dest = new Rectangle(
                        level.X + (col * TILE_SIZE),
                        level.Y + (row * TILE_SIZE),
                        TILE_SIZE,
                        TILE_SIZE
                    );

                    _spriteBatch.Draw(_pixel, dest, color);
                }
            }
        }
    }
}
