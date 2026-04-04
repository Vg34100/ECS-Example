using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Level.Components;
using ECS_Base.LevelData;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System;

namespace ECS_Base.Mechanics.Level.Systems
{
    public class LevelManagerSystem
    {
        private List<LevelData.Level> _loadedLevels = new List<LevelData.Level>();
        private bool _levelsLoaded = false;
        public string ProjectName { get; set; } = "test-tiles";

        public void LoadAllLevels(World world, string basePath, GraphicsDevice graphicsDevice)
        {
            if (_levelsLoaded) return;

            var debugPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "debug.txt");
            System.IO.File.AppendAllText(debugPath, $"\n=== LoadAllLevels Called ===\n");
            System.IO.File.AppendAllText(debugPath, $"Current Directory: {System.IO.Directory.GetCurrentDirectory()}\n");
            System.IO.File.AppendAllText(debugPath, $"Base Directory: {System.AppDomain.CurrentDomain.BaseDirectory}\n");
            System.IO.File.AppendAllText(debugPath, $"Base path argument: {basePath}\n");
            System.IO.File.AppendAllText(debugPath, $"Full path resolved: {System.IO.Path.GetFullPath(basePath)}\n");

            // Try the correct path directly
            var correctPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "../../../Levels");
            System.IO.File.AppendAllText(debugPath, $"Trying path from BaseDirectory: {System.IO.Path.GetFullPath(correctPath)}\n");
            System.IO.File.AppendAllText(debugPath, $"That path exists: {System.IO.Directory.Exists(correctPath)}\n");

            try
            {
                _loadedLevels = LevelData.Level.LoadLevelsFromDirectory(basePath, graphicsDevice, ProjectName);
                System.IO.File.AppendAllText(debugPath, $"Loaded {_loadedLevels.Count} levels from disk\n");

                HashSet<string> allowedIds = null;
                foreach (var entity in world.Query<LevelSelectionConfigComponent>())
                {
                    var selection = world.GetComponent<LevelSelectionConfigComponent>(entity);
                    allowedIds = selection.AllowedIdentifiers;
                    break;
                }

                // Create entities for each level
                foreach (var level in _loadedLevels)
                {
                    var levelEntity = world.CreateEntity();
                    bool isActive = allowedIds == null || allowedIds.Contains(level.Identifier);
                    world.AddComponent(levelEntity, new LevelComponent(level, isActive));
                    Console.WriteLine($"Created level entity for: {level.Identifier} at ({level.X}, {level.Y})");
                    System.IO.File.AppendAllText(debugPath, $"Created level entity for: {level.Identifier} at ({level.X}, {level.Y})\n");
                }

                _levelsLoaded = true;
                Console.WriteLine($"Loaded {_loadedLevels.Count} levels into world");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load levels: {ex.Message}");
                System.IO.File.AppendAllText(debugPath, $"ERROR loading levels: {ex.Message}\n");
                System.IO.File.AppendAllText(debugPath, $"Stack trace: {ex.StackTrace}\n");
            }
        }

        public LevelData.Level GetLevelByIdentifier(string identifier)
        {
            return _loadedLevels.Find(l => l.Identifier == identifier);
        }

        public LevelData.Level GetLevelByUniqueId(string uniqueId)
        {
            return _loadedLevels.Find(l => l.UniqueIdentifier == uniqueId);
        }

        public List<LevelData.Level> GetAllLevels()
        {
            return new List<LevelData.Level>(_loadedLevels);
        }
    }
}
