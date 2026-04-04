using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Editor.Components;
using ECS_Base.Mechanics.Level.Components;
using ECS_Base.Mechanics.Movement.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace ECS_Base.Mechanics.Editor.Systems
{
    /// <summary>
    /// Paints tiles into the active level intgrid.
    /// </summary>
    public class EditorTilePaintSystem : IEditorSystem
    {
        private MouseState _previous;

        public void Update(World world, float deltaTime)
        {
            var mouse = Mouse.GetState();

            var stateEntity = world.Query<EditorStateComponent>().FirstOrDefault();
            if (stateEntity == null)
            {
                _previous = mouse;
                return;
            }

            var state = world.GetComponent<EditorStateComponent>(stateEntity);
            if (!state.IsEditing || (state.Tool != EditorTool.TilePaint && state.Tool != EditorTool.TileErase))
            {
                _previous = mouse;
                return;
            }

            if (IsHoveringUI(world))
            {
                _previous = mouse;
                return;
            }

            var level = FindActiveLevel(world, state.ActiveLevelId);
            if (level == null || level.TileData == null)
            {
                _previous = mouse;
                return;
            }

            var worldPos = ScreenToWorld(world, new Vector2(mouse.X, mouse.Y));
            int col = (int)((worldPos.X - level.X) / 16);
            int row = (int)((worldPos.Y - level.Y) / 16);

            if (row < 0 || col < 0 || row >= level.TileData.GetLength(0) || col >= level.TileData.GetLength(1))
            {
                _previous = mouse;
                return;
            }

            bool leftDown = mouse.LeftButton == ButtonState.Pressed;
            bool rightDown = mouse.RightButton == ButtonState.Pressed;

            if (leftDown)
            {
                int value = state.Tool == EditorTool.TileErase ? 0 : state.SelectedTileValue;
                level.TileData[row, col] = value;
            }
            else if (rightDown)
            {
                level.TileData[row, col] = 0;
            }

            _previous = mouse;
        }

        private static bool IsHoveringUI(World world)
        {
            foreach (var entity in world.Query<EditorUIStateComponent>())
            {
                var ui = world.GetComponent<EditorUIStateComponent>(entity);
                return ui.IsHoveringUI;
            }
            return false;
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

        private static Vector2 ScreenToWorld(World world, Vector2 screen)
        {
            var cameraEntity = world.GetEntities().FirstOrDefault(e => world.TryGetComponent<Mechanics.Camera.Components.CameraComponent>(e, out var cam) && cam.IsActive);
            if (cameraEntity != null && world.TryGetComponent<Mechanics.Camera.Components.CameraComponent>(cameraEntity, out var camera))
            {
                return (screen / camera.Zoom) + camera.Position;
            }
            return screen;
        }
    }
}
