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

        public WorldIconComponent(UIIconType type, Color color, int pixelSize = 2)
        {
            Type = type;
            Color = color;
            PixelSize = pixelSize;
        }
    }
}
