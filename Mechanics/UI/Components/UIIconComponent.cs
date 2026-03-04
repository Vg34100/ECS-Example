using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.UI.Components
{
    /// <summary>
    /// Draws a small pixel icon in screen space.
    /// </summary>
    public struct UIIconComponent
    {
        public Vector2 Position;
        public UIIconType Type;
        public Color Color;
        public int PixelSize;

        public UIIconComponent(Vector2 position, UIIconType type, Color color, int pixelSize = 2)
        {
            Position = position;
            Type = type;
            Color = color;
            PixelSize = pixelSize;
        }
    }

    public enum UIIconType
    {
        Coin,
        Star,
        Rupee,
        Arrow
    }
}
