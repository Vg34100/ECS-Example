using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.UI.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ECS_Base.Mechanics.UI.Systems
{
    /// <summary>
    /// Handles button input and rendering
    /// </summary>
    public class UIButtonSystem
    {
        private MouseState _previousMouseState;
        private Texture2D _pixelTexture;

        public UIButtonSystem(GraphicsDevice graphicsDevice)
        {
            // Create a 1x1 white pixel texture for drawing rectangles
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }

        public void Update(World world)
        {
            var mouseState = Mouse.GetState();
            var mousePosition = new Point(mouseState.X, mouseState.Y);

            foreach (var entity in world.Query<UIButtonComponent>())
            {
                if (!world.TryGetComponent<UIButtonComponent>(entity, out var button))
                    continue;

                if (!button.IsEnabled)
                {
                    button.State = UIButtonState.Normal;
                    world.AddComponent(entity, button);
                    continue;
                }

                bool isHovered = button.Bounds.Contains(mousePosition);
                bool wasPressed = _previousMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
                bool isPressed = mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;

                if (isHovered)
                {
                    if (isPressed)
                    {
                        button.State = UIButtonState.Pressed;
                    }
                    else if (wasPressed && !isPressed)
                    {
                        // Click released on button - trigger callback
                        button.OnClick?.Invoke();
                        button.State = UIButtonState.Hovered;
                    }
                    else
                    {
                        button.State = UIButtonState.Hovered;
                    }
                }
                else
                {
                    button.State = UIButtonState.Normal;
                }

                world.AddComponent(entity, button);
            }

            _previousMouseState = mouseState;
        }

        public void Draw(World world, SpriteBatch spriteBatch, SpriteFont defaultFont = null)
        {
            foreach (var entity in world.Query<UIButtonComponent>())
            {
                if (!world.TryGetComponent<UIButtonComponent>(entity, out var button))
                    continue;

                // Choose color based on state
                Color buttonColor = button.State switch
                {
                    UIButtonState.Hovered => button.HoverColor,
                    UIButtonState.Pressed => button.PressedColor,
                    _ => button.NormalColor
                };

                // Draw button background
                spriteBatch.Draw(
                    _pixelTexture,
                    button.Bounds,
                    buttonColor
                );

                // Draw button text
                var font = button.Font ?? defaultFont;
                if (font != null && !string.IsNullOrEmpty(button.Text))
                {
                    Vector2 textSize = font.MeasureString(button.Text);
                    Vector2 textPosition = new Vector2(
                        button.Bounds.X + (button.Bounds.Width - textSize.X) / 2f,
                        button.Bounds.Y + (button.Bounds.Height - textSize.Y) / 2f
                    );

                    spriteBatch.DrawString(
                        font,
                        button.Text,
                        textPosition,
                        button.TextColor
                    );
                }
            }
        }
    }
}
