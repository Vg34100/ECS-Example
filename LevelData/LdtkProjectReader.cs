using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;

namespace ECS_Base.LevelData
{
    /// <summary>
    /// Minimal LDtk project reader for intgrid -> tileset rect mapping.
    /// </summary>
    public static class LdtkProjectReader
    {
        public static bool TryLoadIntGridMapping(string projectPath, out string tilesetRelPath, out int tileSize, out Dictionary<int, Rectangle> mapping)
        {
            tilesetRelPath = null;
            tileSize = 16;
            mapping = new Dictionary<int, Rectangle>();

            if (!File.Exists(projectPath))
                return false;

            var json = File.ReadAllText(projectPath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("defs", out var defs))
                return false;

            if (!defs.TryGetProperty("tilesets", out var tilesets))
                return false;

            var tilesetByUid = new Dictionary<int, (string relPath, int gridSize)>();
            foreach (var tileset in tilesets.EnumerateArray())
            {
                if (!tileset.TryGetProperty("uid", out var uidProp))
                    continue;
                int uid = uidProp.GetInt32();
                string relPath = tileset.TryGetProperty("relPath", out var rel) ? rel.GetString() : null;
                int grid = tileset.TryGetProperty("tileGridSize", out var gridProp) ? gridProp.GetInt32() : 16;
                tilesetByUid[uid] = (relPath, grid);
            }

            if (!defs.TryGetProperty("layers", out var layers))
                return false;

            foreach (var layer in layers.EnumerateArray())
            {
                if (!layer.TryGetProperty("identifier", out var ident) || ident.GetString() != "Tiles")
                    continue;

                if (!layer.TryGetProperty("intGridValues", out var intGridValues))
                    continue;

                foreach (var valueDef in intGridValues.EnumerateArray())
                {
                    if (!valueDef.TryGetProperty("value", out var valueProp))
                        continue;
                    int value = valueProp.GetInt32();

                    if (!valueDef.TryGetProperty("tile", out var tile))
                        continue;

                    int tilesetUid = tile.TryGetProperty("tilesetUid", out var tsUidProp) ? tsUidProp.GetInt32() : -1;
                    int x = tile.TryGetProperty("x", out var xProp) ? xProp.GetInt32() : 0;
                    int y = tile.TryGetProperty("y", out var yProp) ? yProp.GetInt32() : 0;
                    int w = tile.TryGetProperty("w", out var wProp) ? wProp.GetInt32() : 16;
                    int h = tile.TryGetProperty("h", out var hProp) ? hProp.GetInt32() : 16;

                    mapping[value] = new Rectangle(x, y, w, h);

                    if (tilesetRelPath == null && tilesetByUid.TryGetValue(tilesetUid, out var tilesetInfo))
                    {
                        tilesetRelPath = tilesetInfo.relPath;
                        tileSize = tilesetInfo.gridSize;
                    }
                }
            }

            return mapping.Count > 0 && tilesetRelPath != null;
        }
    }
}
