using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ECS_Base.Mechanics.UI.Components
{
    /// <summary>
    /// Component for rendering text in screen space (UI layer)
    /// </summary>
    public struct UITextComponent
    {
        /// <summary>Font to use for rendering</summary>
        public SpriteFont Font;

        /// <summary>Text to display</summary>
        public string Text;

        /// <summary>Screen position (in pixels)</summary>
        public Vector2 Position;

        /// <summary>Text color</summary>
        public Color Color;

        /// <summary>Text scale</summary>
        public float Scale;

        /// <summary>Text rotation in radians</summary>
        public float Rotation;

        /// <summary>Origin point for rotation/scaling (default: Vector2.Zero)</summary>
        public Vector2 Origin;

        /// <summary>Text alignment (Left, Center, Right)</summary>
        public TextAlignment Alignment;

        /// <summary>Layer depth for sorting (0=front, 1=back)</summary>
        public float LayerDepth;

        public UITextComponent(SpriteFont font, string text, Vector2 position)
        {
            Font = font;
            Text = text;
            Position = position;
            Color = Color.White;
            Scale = 1.0f;
            Rotation = 0f;
            Origin = Vector2.Zero;
            Alignment = TextAlignment.Left;
            LayerDepth = 0f;
        }
    }

    public enum TextAlignment
    {
        Left,
        Center,
        Right
    }
}
