using Microsoft.Xna.Framework;
using ECS_Base.Mechanics.UI.Components;

namespace ECS_Base.Mechanics.Rendering.Components
{
    /// <summary>
    /// Renders a pixel icon in world space.
    /// </summary>
    public struct WorldIconComponent
    {
        public UIIconType Type;
        public Color Color;
        public int PixelSize;
        public float Rotation;

        public WorldIconComponent(UIIconType type, Color color, int pixelSize = 2, float rotation = 0f)
        {
            Type = type;
            Color = color;
            PixelSize = pixelSize;
            Rotation = rotation;
        }
    }
}
