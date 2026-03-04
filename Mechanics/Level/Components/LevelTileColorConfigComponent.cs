using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Level.Components
{
    /// <summary>
    /// Color-based tile rendering for intgrid values.
    /// </summary>
    public struct LevelTileColorConfigComponent
    {
        public Dictionary<int, Color> TileColors;
        public Color DefaultColor;

        public LevelTileColorConfigComponent(Dictionary<int, Color> tileColors, Color defaultColor)
        {
            TileColors = tileColors;
            DefaultColor = defaultColor;
        }
    }
}
