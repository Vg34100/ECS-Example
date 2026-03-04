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
    /// Press F5 to save and restart into play mode.
    /// </summary>
    public class EditorPlaytestSystem : IEditorSystem
    {
        private KeyboardState _previous;
        private readonly Game1 _game;

        public EditorPlaytestSystem(Game1 game)
        {
            _game = game;
        }

        public void Update(World world, float deltaTime)
        {
            var keyboard = Keyboard.GetState();
            if (keyboard.IsKeyDown(Keys.F5) && !_previous.IsKeyDown(Keys.F5))
            {
                SaveActiveLevel(world);
                _game.RequestRestart();
            }
            _previous = keyboard;
        }

        private static void SaveActiveLevel(World world)
        {
            var stateEntity = world.Query<EditorStateComponent>().FirstOrDefault();
            if (stateEntity == null)
                return;

            var state = world.GetComponent<EditorStateComponent>(stateEntity);
            if (!state.IsEditing)
                return;

            foreach (var entity in world.Query<LevelComponent>())
            {
                var levelComp = world.GetComponent<LevelComponent>(entity);
                if (!levelComp.IsActive)
                    continue;
                if (!string.IsNullOrWhiteSpace(state.ActiveLevelId) && levelComp.LevelData.Identifier != state.ActiveLevelId)
                    continue;

                var levelDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Levels/ldtk-programmatic/simplified", levelComp.LevelData.Identifier);
                EditorLevelSerializer.SaveLevel(levelComp.LevelData, levelDir);
                return;
            }
        }
    }
}
