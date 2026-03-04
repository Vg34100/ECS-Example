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
        public float Rotation;

        public UIIconComponent(Vector2 position, UIIconType type, Color color, int pixelSize = 2, float rotation = 0f)
        {
            Position = position;
            Type = type;
            Color = color;
            PixelSize = pixelSize;
            Rotation = rotation;
        }
    }

    public enum UIIconType
    {
        Coin,
        Star,
        Rupee,
        Arrow,
        Mushroom,
        Bomb
    }
}
