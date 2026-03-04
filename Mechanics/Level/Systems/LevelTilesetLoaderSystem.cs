using System;
using System.IO;
using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Level.Components;
using ECS_Base.LevelData;
using Microsoft.Xna.Framework.Graphics;

namespace ECS_Base.Mechanics.Level.Systems
{
    /// <summary>
    /// Loads tileset texture and intgrid mapping from an LDtk project file.
    /// </summary>
    public class LevelTilesetLoaderSystem
    {
        private readonly GraphicsDevice _graphicsDevice;
        private bool _loaded;

        public LevelTilesetLoaderSystem(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        public void Update(World world, float deltaTime)
        {
            if (_loaded)
                return;

            foreach (var entity in world.Query<LevelTilesetConfigComponent>())
            {
                var config = world.GetComponent<LevelTilesetConfigComponent>(entity);

                var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../", config.LevelsRootRelative);
                var projectPath = Path.Combine(basePath, config.ProjectFileName);

                if (!LdtkProjectReader.TryLoadIntGridMapping(projectPath, out var tilesetRelPath, out var tileSize, out var mapping))
                    continue;

                var tilesetPath = Path.Combine(basePath, tilesetRelPath);
                if (!File.Exists(tilesetPath))
                    continue;

                using var stream = new FileStream(tilesetPath, FileMode.Open, FileAccess.Read);
                var texture = Texture2D.FromStream(_graphicsDevice, stream);

                var tilesetEntity = world.CreateEntity();
                world.AddComponent(tilesetEntity, new LevelTilesetComponent(texture, mapping, tileSize));
                _loaded = true;
                break;
            }
        }
    }
}
