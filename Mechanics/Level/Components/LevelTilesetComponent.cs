using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ECS_Base.Mechanics.Level.Components
{
    /// <summary>
    /// Loaded tileset texture + intgrid value -> source rect mapping.
    /// </summary>
    public struct LevelTilesetComponent
    {
        public Texture2D TilesetTexture;
        public Dictionary<int, Rectangle> TileRects;
        public int TileSize;

        public LevelTilesetComponent(Texture2D tilesetTexture, Dictionary<int, Rectangle> tileRects, int tileSize)
        {
            TilesetTexture = tilesetTexture;
            TileRects = tileRects;
            TileSize = tileSize;
        }
    }
}
