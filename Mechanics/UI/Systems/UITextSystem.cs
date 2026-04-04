using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.UI.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ECS_Base.Mechanics.UI.Systems
{
    /// <summary>
    /// Renders UI text in screen space
    /// </summary>
    public class UITextSystem
    {
        public void Draw(World world, SpriteBatch spriteBatch)
        {
            foreach (var entity in world.Query<UITextComponent>())
            {
                if (!world.TryGetComponent<UITextComponent>(entity, out var text))
                    continue;

                if (text.Font == null || string.IsNullOrEmpty(text.Text))
                    continue;

                Vector2 position = text.Position;

                // Apply alignment
                if (text.Alignment != TextAlignment.Left)
                {
                    Vector2 textSize = text.Font.MeasureString(text.Text);

                    if (text.Alignment == TextAlignment.Center)
                    {
                        position.X -= (textSize.X * text.Scale) / 2f;
                    }
                    else if (text.Alignment == TextAlignment.Right)
                    {
                        position.X -= textSize.X * text.Scale;
                    }
                }

                spriteBatch.DrawString(
                    text.Font,
                    text.Text,
                    position,
                    text.Color,
                    text.Rotation,
                    text.Origin,
                    text.Scale,
                    SpriteEffects.None,
                    text.LayerDepth
                );
            }
        }
    }
}
