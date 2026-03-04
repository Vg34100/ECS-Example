using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ECS_Base.LevelData
{
    /// <summary>
    /// Writes simplified level data (Tiles.csv + data.json).
    /// </summary>
    public static class EditorLevelSerializer
    {
        public static void SaveLevel(Level level, string levelDirectory)
        {
            Directory.CreateDirectory(levelDirectory);
            var tilesPath = Path.Combine(levelDirectory, "Tiles.csv");
            var dataPath = Path.Combine(levelDirectory, "data.json");

            SaveTiles(level, tilesPath);
            SaveData(level, dataPath);
        }

        private static void SaveTiles(Level level, string csvPath)
        {
            if (level.TileData == null)
                return;

            int rows = level.TileData.GetLength(0);
            int cols = level.TileData.GetLength(1);
            using var writer = new StreamWriter(csvPath, false);
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    writer.Write(level.TileData[r, c]);
                    writer.Write(',');
                }
                writer.WriteLine();
            }
        }

        private static void SaveData(Level level, string jsonPath)
        {
            var entities = new Dictionary<string, object>
            {
                ["Player"] = level.Players,
                ["Enemy"] = level.Enemies,
                ["Pickup"] = level.Pickups,
                ["Hazard"] = level.Hazards,
                ["Path"] = level.Paths
            };

            var data = new Dictionary<string, object>
            {
                ["identifier"] = level.Identifier,
                ["uniqueIdentifer"] = level.UniqueIdentifier,
                ["x"] = level.X,
                ["y"] = level.Y,
                ["width"] = level.Width,
                ["height"] = level.Height,
                ["bgColor"] = level.BgColor,
                ["neighbourLevels"] = level.NeighbourLevels,
                ["customFields"] = new Dictionary<string, object> { ["RoomType"] = level.RoomType.ToString() },
                ["layers"] = new [] { "Tiles.png" },
                ["entities"] = entities
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            File.WriteAllText(jsonPath, JsonSerializer.Serialize(data, options));
        }
    }
}
