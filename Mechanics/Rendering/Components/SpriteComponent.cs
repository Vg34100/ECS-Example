using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ECS_Base.Mechanics.Rendering.Components
{
    /// <summary>
    /// Component for sprite-based rendering with support for sprite sheets, rotation, scaling, and layering
    /// Replaces/enhances shape-only rendering for production games
    /// </summary>
    public struct SpriteComponent
    {
        public Texture2D Texture;
        public Rectangle SourceRectangle; // For sprite sheets (portion of texture to draw)
        public Vector2 Origin; // Pivot point (rotation center)
        public Vector2 Scale;
        public float Rotation; // Radians
        public Color Tint;
        public float Opacity; // 0-1, multiplied with Tint alpha
        public int Layer; // Z-order sorting (lower draws first)
        public SpriteEffects Effects; // Flip horizontal/vertical

        public SpriteComponent(Texture2D texture)
        {
            Texture = texture;
            if (texture != null)
            {
                SourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
                Origin = new Vector2(texture.Width / 2f, texture.Height / 2f); // Center by default
            }
            else
            {
                // For testing without actual texture
                SourceRectangle = new Rectangle(0, 0, 32, 32);
                Origin = new Vector2(16, 16);
            }
            Scale = Vector2.One;
            Rotation = 0f;
            Tint = Color.White;
            Opacity = 1f;
            Layer = 0;
            Effects = SpriteEffects.None;
        }

        public SpriteComponent(Texture2D texture, Rectangle sourceRect)
        {
            Texture = texture;
            SourceRectangle = sourceRect;
            Origin = new Vector2(sourceRect.Width / 2f, sourceRect.Height / 2f);
            Scale = Vector2.One;
            Rotation = 0f;
            Tint = Color.White;
            Opacity = 1f;
            Layer = 0;
            Effects = SpriteEffects.None;
        }
    }
}
