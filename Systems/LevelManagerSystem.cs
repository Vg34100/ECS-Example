// Systems/LevelManagerSystem.cs
using ECS_Example.Components;
using ECS_Example.LevelData;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System;

namespace ECS_Example.Systems
{
    public class LevelManagerSystem
    {
        private List<Level> _loadedLevels = new List<Level>();
        private bool _levelsLoaded = false;

        public void LoadAllLevels(World world, string basePath, GraphicsDevice graphicsDevice)
        {
            if (_levelsLoaded) return;

            try
            {
                _loadedLevels = Level.LoadLevelsFromDirectory(basePath, graphicsDevice);

                // Create entities for each level
                foreach (var level in _loadedLevels)
                {
                    var levelEntity = world.CreateEntity();
                    world.AddComponent(levelEntity, new LevelComponent(level, true));
                    Console.WriteLine($"Created level entity for: {level.Identifier} at ({level.X}, {level.Y})");
                }

                _levelsLoaded = true;
                Console.WriteLine($"Loaded {_loadedLevels.Count} levels into world");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load levels: {ex.Message}");
            }
        }

        public Level GetLevelByIdentifier(string identifier)
        {
            return _loadedLevels.Find(l => l.Identifier == identifier);
        }

        public Level GetLevelByUniqueId(string uniqueId)
        {
            return _loadedLevels.Find(l => l.UniqueIdentifier == uniqueId);
        }

        public List<Level> GetAllLevels()
        {
            return new List<Level>(_loadedLevels);
        }
    }
}