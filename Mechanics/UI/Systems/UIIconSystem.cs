using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.UI.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ECS_Base.Mechanics.UI.Systems
{
    /// <summary>
    /// Renders small pixel icons.
    /// </summary>
    public class UIIconSystem
    {
        private readonly Texture2D _pixel;

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

        private static readonly string[] BombMask =
        {
            "00111100",
            "01111110",
            "11111111",
            "11111111",
            "11111111",
            "01111110",
            "00111100",
            "00011000"
        };

        public UIIconSystem(GraphicsDevice graphicsDevice)
        {
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public void Draw(World world, SpriteBatch spriteBatch)
        {
            foreach (var entity in world.Query<UIIconComponent>())
            {
                if (!world.TryGetComponent<UIIconComponent>(entity, out var icon))
                    continue;

                var mask = icon.Type switch
                {
                    UIIconType.Coin => CoinMask,
                    UIIconType.Star => StarMask,
                    UIIconType.Rupee => RupeeMask,
                    UIIconType.Arrow => ArrowMask,
                    UIIconType.Mushroom => MushroomMask,
                    UIIconType.Bomb => BombMask,
                    _ => CoinMask
                };

                DrawMask(spriteBatch, icon.Position, mask, icon.Color, icon.PixelSize, icon.Rotation);
            }
        }

        private void DrawMask(SpriteBatch spriteBatch, Vector2 pos, string[] mask, Color color, int pixelSize, float rotation)
        {
            float centerX = (mask[0].Length * pixelSize) / 2f;
            float centerY = (mask.Length * pixelSize) / 2f;

            for (int y = 0; y < mask.Length; y++)
            {
                for (int x = 0; x < mask[y].Length; x++)
                {
                    if (mask[y][x] != '1')
                        continue;

                    float localX = (x * pixelSize) - centerX;
                    float localY = (y * pixelSize) - centerY;

                    float cos = (float)System.Math.Cos(rotation);
                    float sin = (float)System.Math.Sin(rotation);

                    float rotX = localX * cos - localY * sin;
                    float rotY = localX * sin + localY * cos;

                    spriteBatch.Draw(
                        _pixel,
                        new Rectangle(
                            (int)(pos.X + centerX + rotX),
                            (int)(pos.Y + centerY + rotY),
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
