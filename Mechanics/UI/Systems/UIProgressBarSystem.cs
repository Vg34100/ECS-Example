using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.UI.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ECS_Base.Mechanics.UI.Systems
{
    /// <summary>
    /// Renders progress bars
    /// </summary>
    public class UIProgressBarSystem
    {
        private Texture2D _pixelTexture;

        public UIProgressBarSystem(GraphicsDevice graphicsDevice)
        {
            // Create a 1x1 white pixel texture for drawing rectangles
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }

        public void Draw(World world, SpriteBatch spriteBatch)
        {
            foreach (var entity in world.Query<UIProgressBarComponent>())
            {
                if (!world.TryGetComponent<UIProgressBarComponent>(entity, out var bar))
                    continue;

                // Draw background
                spriteBatch.Draw(
                    _pixelTexture,
                    bar.Bounds,
                    bar.BackgroundColor
                );

                // Calculate fill rectangle based on direction
                Rectangle fillRect = CalculateFillRectangle(bar);

                // Draw foreground
                if (fillRect.Width > 0 && fillRect.Height > 0)
                {
                    spriteBatch.Draw(
                        _pixelTexture,
                        fillRect,
                        bar.ForegroundColor
                    );
                }

                // Draw border
                if (bar.BorderColor != Color.Transparent && bar.BorderThickness > 0)
                {
                    DrawBorder(spriteBatch, bar.Bounds, bar.BorderColor, bar.BorderThickness);
                }
            }
        }

        private Rectangle CalculateFillRectangle(UIProgressBarComponent bar)
        {
            float fillPercentage = bar.FillPercentage;
            Rectangle fillRect = bar.Bounds;

            switch (bar.Direction)
            {
                case FillDirection.LeftToRight:
                    fillRect.Width = (int)(bar.Bounds.Width * fillPercentage);
                    break;

                case FillDirection.RightToLeft:
                    int width = (int)(bar.Bounds.Width * fillPercentage);
                    fillRect.X = bar.Bounds.Right - width;
                    fillRect.Width = width;
                    break;

                case FillDirection.BottomToTop:
                    int height = (int)(bar.Bounds.Height * fillPercentage);
                    fillRect.Y = bar.Bounds.Bottom - height;
                    fillRect.Height = height;
                    break;

                case FillDirection.TopToBottom:
                    fillRect.Height = (int)(bar.Bounds.Height * fillPercentage);
                    break;
            }

            return fillRect;
        }

        private void DrawBorder(SpriteBatch spriteBatch, Rectangle bounds, Color color, int thickness)
        {
            // Top
            spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.X, bounds.Y, bounds.Width, thickness), color);
            // Bottom
            spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.X, bounds.Bottom - thickness, bounds.Width, thickness), color);
            // Left
            spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.X, bounds.Y, thickness, bounds.Height), color);
            // Right
            spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.Right - thickness, bounds.Y, thickness, bounds.Height), color);
        }
    }
}
