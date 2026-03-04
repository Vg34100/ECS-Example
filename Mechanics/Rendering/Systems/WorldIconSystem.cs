using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.Rendering.Components;
using ECS_Base.Mechanics.UI.Components;
using ECS_Base.Mechanics.Camera.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ECS_Base.Mechanics.Rendering.Systems
{
    /// <summary>
    /// Renders pixel icons in world space.
    /// </summary>
    public class WorldIconSystem
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly Texture2D _pixel;
        private readonly CameraSystem _cameraSystem;

        private static readonly string[] CoinMask =
        {
            "00111100",
            "01111110",
            "11111111",
            "11111111",
            "11111111",
            "11111111",
            "01111110",
            "00111100"
        };

        private static readonly string[] StarMask =
        {
            "00100100",
            "00100100",
            "11111111",
            "01111110",
            "00111100",
            "01111110",
            "11111111",
            "00100100"
        };

        private static readonly string[] RupeeMask =
        {
            "00011000",
            "00111100",
            "01111110",
            "11111111",
            "11111111",
            "01111110",
            "00111100",
            "00011000"
        };

        private static readonly string[] ArrowMask =
        {
            "00011000",
            "00111100",
            "01111110",
            "11111111",
            "00011000",
            "00011000",
            "00011000",
            "00011000"
        };

        private static readonly string[] MushroomMask =
        {
            "01111110",
            "11111111",
            "11111111",
            "11111111",
            "01111110",
            "00111100",
            "00111100",
            "01111110"
        };

        public WorldIconSystem(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, CameraSystem cameraSystem)
        {
            _spriteBatch = spriteBatch;
            _cameraSystem = cameraSystem;
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public void Draw(World world)
        {
            var viewMatrix = _cameraSystem.GetViewMatrix(world);
            _spriteBatch.Begin(transformMatrix: viewMatrix, samplerState: SamplerState.PointClamp);

            foreach (var entity in world.Query<WorldIconComponent, PositionComponent>())
            {
                var icon = world.GetComponent<WorldIconComponent>(entity);
                var position = world.GetComponent<PositionComponent>(entity);

                var mask = icon.Type switch
                {
                    UIIconType.Coin => CoinMask,
                    UIIconType.Star => StarMask,
                    UIIconType.Rupee => RupeeMask,
                    UIIconType.Arrow => ArrowMask,
                    UIIconType.Mushroom => MushroomMask,
                    _ => CoinMask
                };

                DrawMask(position.Value, mask, icon.Color, icon.PixelSize);
            }

            _spriteBatch.End();
        }

        private void DrawMask(Vector2 pos, string[] mask, Color color, int pixelSize)
        {
            for (int y = 0; y < mask.Length; y++)
            {
                for (int x = 0; x < mask[y].Length; x++)
                {
                    if (mask[y][x] != '1')
                        continue;

                    _spriteBatch.Draw(
                        _pixel,
                        new Rectangle(
                            (int)pos.X + x * pixelSize,
                            (int)pos.Y + y * pixelSize,
                            pixelSize,
                            pixelSize
                        ),
                        color
                    );
                }
            }
        }
    }
}
