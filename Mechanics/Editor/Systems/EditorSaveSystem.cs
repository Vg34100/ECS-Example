using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Editor.Components;
using ECS_Base.Mechanics.Level.Components;
using ECS_Base.LevelData;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Linq;

namespace ECS_Base.Mechanics.Editor.Systems
{
    /// <summary>
    /// Saves current level to disk (Ctrl+S).
    /// </summary>
    public class EditorSaveSystem : IEditorSystem
    {
        private KeyboardState _previous;

        public void Update(World world, float deltaTime)
        {
            var keyboard = Keyboard.GetState();
            bool ctrl = keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl);
            bool save = ctrl && keyboard.IsKeyDown(Keys.S) && !_previous.IsKeyDown(Keys.S);

            if (!save)
            {
                _previous = keyboard;
                return;
            }

            var stateEntity = world.Query<EditorStateComponent>().FirstOrDefault();
            if (stateEntity == null)
            {
                _previous = keyboard;
                return;
            }

            var state = world.GetComponent<EditorStateComponent>(stateEntity);
            if (!state.IsEditing)
            {
                _previous = keyboard;
                return;
            }

            var level = FindActiveLevel(world, state.ActiveLevelId);
            if (level == null)
            {
                _previous = keyboard;
                return;
            }

            var levelDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Levels/ldtk-programmatic/simplified", level.Identifier);
            EditorLevelSerializer.SaveLevel(level, levelDir);
            _previous = keyboard;
        }

        private static ECS_Base.LevelData.Level FindActiveLevel(World world, string levelId)
        {
            foreach (var entity in world.Query<LevelComponent>())
            {
                var levelComp = world.GetComponent<LevelComponent>(entity);
                if (!levelComp.IsActive)
                    continue;
                if (string.IsNullOrWhiteSpace(levelId) || levelComp.LevelData.Identifier == levelId)
                    return levelComp.LevelData;
            }
            return null;
        }
    }
}
