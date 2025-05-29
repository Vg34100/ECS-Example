// Level.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ECS_Example.LevelData
{
    public enum RoomType
    {
        None,
        Start
    }

    public class LevelEntity
    {
        public string Id { get; set; }
        public string Iid { get; set; }
        public string Layer { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Color { get; set; }
        public Dictionary<string, object> CustomFields { get; set; } = new Dictionary<string, object>();
    }

    public class PathEntity : LevelEntity
    {
        public string NextDoorEntityIid { get; set; }
        public string NextDoorLevelIid { get; set; }
    }

    public class NeighbourLevel
    {
        public string LevelIid { get; set; }
        public string Dir { get; set; }
    }

    public class Level
    {
        public string Identifier { get; set; }
        public string UniqueIdentifier { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string BgColor { get; set; }
        public RoomType RoomType { get; set; }
        public List<NeighbourLevel> NeighbourLevels { get; set; } = new List<NeighbourLevel>();
        public List<string> Layers { get; set; } = new List<string>();

        // Entity collections
        public List<LevelEntity> Players { get; set; } = new List<LevelEntity>();
        public List<LevelEntity> Enemies { get; set; } = new List<LevelEntity>();
        public List<PathEntity> Paths { get; set; } = new List<PathEntity>();

        // Tile data
        public int[,] TileData { get; set; }
        public Texture2D TileTexture { get; set; }

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public static List<Level> LoadLevelsFromDirectory(string basePath, GraphicsDevice graphicsDevice)
        {
            var levels = new List<Level>();
            string simplifiedPath = Path.Combine(basePath, "Levels", "test-tiles", "simplified");

            Debug.WriteLine(simplifiedPath);

            if (!Directory.Exists(simplifiedPath))
            {
                throw new DirectoryNotFoundException($"Simplified levels directory not found: {simplifiedPath}");
            }

            var levelDirectories = Directory.GetDirectories(simplifiedPath);

            foreach (var levelDir in levelDirectories)
            {
                try
                {
                    var level = LoadLevelFromDirectory(levelDir, graphicsDevice);
                    if (level != null)
                    {
                        levels.Add(level);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to load level from {levelDir}: {ex.Message}");
                }
            }

            return levels;
        }

        private static Level LoadLevelFromDirectory(string levelDirectory, GraphicsDevice graphicsDevice)
        {
            try
            {
                Debug.WriteLine($"Loading level from: {levelDirectory}");

                string dataJsonPath = Path.Combine(levelDirectory, "data.json");
                string tilesCsvPath = Path.Combine(levelDirectory, "Tiles.csv");

                if (!File.Exists(dataJsonPath))
                {
                    Debug.WriteLine($"data.json not found in {levelDirectory}");
                    return null;
                }

                // Parse JSON data
                string jsonContent = File.ReadAllText(dataJsonPath);
                var jsonDoc = JsonDocument.Parse(jsonContent);
                var root = jsonDoc.RootElement;

                var level = new Level();

                // Parse basic level properties with error handling
                try
                {
                    level.Identifier = root.GetProperty("identifier").GetString();
                    level.UniqueIdentifier = root.GetProperty("uniqueIdentifer").GetString();
                    level.X = root.GetProperty("x").GetInt32();
                    level.Y = root.GetProperty("y").GetInt32();
                    level.Width = root.GetProperty("width").GetInt32();
                    level.Height = root.GetProperty("height").GetInt32();
                    level.BgColor = root.GetProperty("bgColor").GetString();

                    Debug.WriteLine($"Parsed basic properties for level: {level.Identifier}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error parsing basic level properties: {ex.Message}");
                    throw;
                }

                // Parse custom fields for room type
                try
                {
                    if (root.TryGetProperty("customFields", out var customFields))
                    {
                        if (customFields.TryGetProperty("RoomType", out var roomTypeElement))
                        {
                            var roomTypeStr = roomTypeElement.GetString();
                            level.RoomType = roomTypeStr switch
                            {
                                "Start" => RoomType.Start,
                                _ => RoomType.None
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error parsing custom fields: {ex.Message}");
                }

                // Parse neighbour levels
                try
                {
                    if (root.TryGetProperty("neighbourLevels", out var neighbourLevels))
                    {
                        foreach (var neighbour in neighbourLevels.EnumerateArray())
                        {
                            level.NeighbourLevels.Add(new NeighbourLevel
                            {
                                LevelIid = neighbour.GetProperty("levelIid").GetString(),
                                Dir = neighbour.GetProperty("dir").GetString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error parsing neighbour levels: {ex.Message}");
                }

                // Parse layers
                try
                {
                    if (root.TryGetProperty("layers", out var layers))
                    {
                        foreach (var layer in layers.EnumerateArray())
                        {
                            level.Layers.Add(layer.GetString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error parsing layers: {ex.Message}");
                }

                // Parse entities
                try
                {
                    if (root.TryGetProperty("entities", out var entities))
                    {
                        ParseEntities(entities, level);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error parsing entities: {ex.Message}");
                }

                // Load tile data
                try
                {
                    if (File.Exists(tilesCsvPath))
                    {
                        level.TileData = LoadTileData(tilesCsvPath);
                    }
                    else
                    {
                        Debug.WriteLine($"Tiles.csv not found: {tilesCsvPath}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading tile data: {ex.Message}");
                }

                // Load tile texture (skipping for now since it requires MonoGame content pipeline)
                
                string tileTexturePath = Path.Combine(levelDirectory, "Tiles.png");
                if (File.Exists(tileTexturePath))
                {
                    try
                    {
                        using var fileStream = new FileStream(tileTexturePath, FileMode.Open);
                        level.TileTexture = Texture2D.FromStream(graphicsDevice, fileStream);
                        Debug.WriteLine($"Loaded texture: {tileTexturePath}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to load texture {tileTexturePath}: {ex.Message}");
                    }
                }


                Debug.WriteLine($"Successfully loaded level: {level.Identifier}");
                return level;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load level from {levelDirectory}: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        private static void ParseEntities(JsonElement entities, Level level)
        {
            // Parse Players
            if (entities.TryGetProperty("Player", out var players))
            {
                foreach (var player in players.EnumerateArray())
                {
                    level.Players.Add(ParseLevelEntity(player));
                }
            }

            // Parse Enemies
            if (entities.TryGetProperty("Enemy", out var enemies))
            {
                foreach (var enemy in enemies.EnumerateArray())
                {
                    level.Enemies.Add(ParseLevelEntity(enemy));
                }
            }

            // Parse Paths
            if (entities.TryGetProperty("Path", out var paths))
            {
                foreach (var path in paths.EnumerateArray())
                {
                    level.Paths.Add(ParsePathEntity(path));
                }
            }
        }

        private static LevelEntity ParseLevelEntity(JsonElement entityElement)
        {
            var entity = new LevelEntity
            {
                Id = entityElement.GetProperty("id").GetString(),
                Iid = entityElement.GetProperty("iid").GetString(),
                Layer = entityElement.GetProperty("layer").GetString(),
                X = entityElement.GetProperty("x").GetInt32(),
                Y = entityElement.GetProperty("y").GetInt32(),
                Width = entityElement.GetProperty("width").GetInt32(),
                Height = entityElement.GetProperty("height").GetInt32(),
                Color = entityElement.GetProperty("color").GetInt32()
            };

            // Parse custom fields if they exist
            if (entityElement.TryGetProperty("customFields", out var customFields))
            {
                foreach (var field in customFields.EnumerateObject())
                {
                    entity.CustomFields[field.Name] = field.Value.ToString();
                }
            }

            return entity;
        }

        private static PathEntity ParsePathEntity(JsonElement pathElement)
        {
            var pathEntity = new PathEntity
            {
                Id = pathElement.GetProperty("id").GetString(),
                Iid = pathElement.GetProperty("iid").GetString(),
                Layer = pathElement.GetProperty("layer").GetString(),
                X = pathElement.GetProperty("x").GetInt32(),
                Y = pathElement.GetProperty("y").GetInt32(),
                Width = pathElement.GetProperty("width").GetInt32(),
                Height = pathElement.GetProperty("height").GetInt32(),
                Color = pathElement.GetProperty("color").GetInt32()
            };

            // Parse nextDoor custom field
            if (pathElement.TryGetProperty("customFields", out var customFields) &&
                customFields.TryGetProperty("nextDoor", out var nextDoor))
            {
                if (nextDoor.TryGetProperty("entityIid", out var entityIid))
                {
                    pathEntity.NextDoorEntityIid = entityIid.GetString();
                }
                if (nextDoor.TryGetProperty("levelIid", out var levelIid))
                {
                    pathEntity.NextDoorLevelIid = levelIid.GetString();
                }
            }

            return pathEntity;
        }

        private static int[,] LoadTileData(string csvPath)
        {
            try
            {
                var lines = File.ReadAllLines(csvPath);
                if (lines.Length == 0)
                {
                    Debug.WriteLine($"CSV file is empty: {csvPath}");
                    return null;
                }

                // Split first line to get column count, handling potential empty values
                var firstLineValues = lines[0].Split(',', StringSplitOptions.RemoveEmptyEntries);
                int cols = firstLineValues.Length;
                int rows = lines.Length;

                Debug.WriteLine($"Loading CSV: {Path.GetFileName(csvPath)} - {rows}x{cols}");

                var tileData = new int[rows, cols];

                for (int row = 0; row < rows; row++)
                {
                    var values = lines[row].Split(',');

                    // Handle lines that might have different column counts
                    int actualCols = Math.Min(values.Length, cols);

                    for (int col = 0; col < actualCols; col++)
                    {
                        if (int.TryParse(values[col].Trim(), out int tileValue))
                        {
                            tileData[row, col] = tileValue;
                        }
                        else
                        {
                            Debug.WriteLine($"Failed to parse tile value '{values[col]}' at row {row}, col {col}");
                            tileData[row, col] = 0; // Default to 0
                        }
                    }
                }

                return tileData;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading tile data from {csvPath}: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        // Test methods for debugging
        public void PrintTileData()
        {
            if (TileData == null)
            {
                Debug.WriteLine($"Level {Identifier}: No tile data loaded");
                return;
            }

            Debug.WriteLine($"Level {Identifier} Tile Data ({TileData.GetLength(0)}x{TileData.GetLength(1)}):");
            for (int row = 0; row < TileData.GetLength(0); row++)
            {
                for (int col = 0; col < TileData.GetLength(1); col++)
                {
                    Debug.Write($"{TileData[row, col]},");
                }
                Debug.WriteLine("");
            }
            Debug.WriteLine("");
        }

        public void PrintEntities()
        {
            Debug.WriteLine($"Level {Identifier} Entities:");

            Debug.WriteLine($"  Players ({Players.Count}):");
            foreach (var player in Players)
            {
                Debug.WriteLine($"    - {player.Id} at ({player.X}, {player.Y})");
            }

            Debug.WriteLine($"  Enemies ({Enemies.Count}):");
            foreach (var enemy in Enemies)
            {
                Debug.WriteLine($"    - {enemy.Id} at ({enemy.X}, {enemy.Y})");
            }

            Debug.WriteLine($"  Paths ({Paths.Count}):");
            foreach (var path in Paths)
            {
                Debug.WriteLine($"    - {path.Id} at ({path.X}, {path.Y}) -> Level: {path.NextDoorLevelIid}, Entity: {path.NextDoorEntityIid}");
            }
            Debug.WriteLine("");
        }
    }
}