using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Editor.Components;
using Microsoft.Xna.Framework.Input;

namespace ECS_Base.Mechanics.Editor.Systems
{
    /// <summary>
    /// Toggles editor mode and handles global editor hotkeys.
    /// </summary>
    public class EditorModeSystem : IAlwaysUpdateSystem
    {
        private KeyboardState _previous;

        public void Update(World world, float deltaTime)
        {
            var keyboard = Keyboard.GetState();

            foreach (var entity in world.Query<EditorStateComponent>())
            {
                var state = world.GetComponent<EditorStateComponent>(entity);

                if (keyboard.IsKeyDown(Keys.F1) && !_previous.IsKeyDown(Keys.F1))
                {
                    state.IsEditing = !state.IsEditing;
                }

                if (state.IsEditing)
                {
                    if (keyboard.IsKeyDown(Keys.T) && !_previous.IsKeyDown(Keys.T))
                        state.Tool = EditorTool.TilePaint;
                    if (keyboard.IsKeyDown(Keys.E) && !_previous.IsKeyDown(Keys.E))
                        state.Tool = EditorTool.TileErase;
                    if (keyboard.IsKeyDown(Keys.Y) && !_previous.IsKeyDown(Keys.Y))
                        state.Tool = EditorTool.EntityPlace;

                    if (keyboard.IsKeyDown(Keys.F2) && !_previous.IsKeyDown(Keys.F2))
                    {
                        state.SelectedEntityType = (EditorEntityType)(((int)state.SelectedEntityType + 1) % 5);
                        state.SelectedEntityKind = GetDefaultKind(state.SelectedEntityType);
                    }
                    if (keyboard.IsKeyDown(Keys.F3) && !_previous.IsKeyDown(Keys.F3))
                    {
                        state.SelectedEntityKind = NextKind(state.SelectedEntityType, state.SelectedEntityKind);
                    }

                    if (keyboard.IsKeyDown(Keys.D1)) state.SelectedTileValue = 1;
                    if (keyboard.IsKeyDown(Keys.D2)) state.SelectedTileValue = 2;
                    if (keyboard.IsKeyDown(Keys.D3)) state.SelectedTileValue = 3;
                    if (keyboard.IsKeyDown(Keys.D4)) state.SelectedTileValue = 4;
                    if (keyboard.IsKeyDown(Keys.D5)) state.SelectedTileValue = 5;
                    if (keyboard.IsKeyDown(Keys.D6)) state.SelectedTileValue = 6;
                    if (keyboard.IsKeyDown(Keys.D7)) state.SelectedTileValue = 10;
                    if (keyboard.IsKeyDown(Keys.D8)) state.SelectedTileValue = 11;
                    if (keyboard.IsKeyDown(Keys.D9)) state.SelectedTileValue = 12;
                }

                world.AddComponent(entity, state);
            }

            _previous = keyboard;
        }

        private static string GetDefaultKind(EditorEntityType type)
        {
            return type switch
            {
                EditorEntityType.Enemy => "Goomba",
                EditorEntityType.Pickup => "Coin",
                EditorEntityType.Hazard => "Spike",
                _ => ""
            };
        }

        private static string NextKind(EditorEntityType type, string current)
        {
            string[] kinds = type switch
            {
                EditorEntityType.Enemy => new[] { "Goomba", "Shooter" },
                EditorEntityType.Pickup => new[] { "Coin", "Star", "Mushroom", "Rupee", "Heart", "Arrow", "Bomb" },
                EditorEntityType.Hazard => new[] { "Spike" },
                _ => new[] { "" }
            };

            if (kinds.Length == 0)
                return "";

            int index = 0;
            for (int i = 0; i < kinds.Length; i++)
            {
                if (kinds[i] == current)
                {
                    index = i;
                    break;
                }
            }
            index = (index + 1) % kinds.Length;
            return kinds[index];
        }
    }
}
