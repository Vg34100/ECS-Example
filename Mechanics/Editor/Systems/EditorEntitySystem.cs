using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Editor.Components;
using ECS_Base.Mechanics.Level.Components;
using ECS_Base.LevelData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace ECS_Base.Mechanics.Editor.Systems
{
    /// <summary>
    /// Places entities into the level data while in editor mode.
    /// </summary>
    public class EditorEntitySystem : IEditorSystem
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
            if (!state.IsEditing || state.Tool != EditorTool.EntityPlace)
            {
                _previous = mouse;
                return;
            }

            if (mouse.LeftButton != ButtonState.Pressed || _previous.LeftButton == ButtonState.Pressed)
            {
                _previous = mouse;
                return;
            }

            var level = FindActiveLevel(world, state.ActiveLevelId);
            if (level == null)
            {
                _previous = mouse;
                return;
            }

            var worldPos = ScreenToWorld(world, new Vector2(mouse.X, mouse.Y));
            int gridX = (int)((worldPos.X - level.X) / 16) * 16;
            int gridY = (int)((worldPos.Y - level.Y) / 16) * 16;

            PlaceEntity(level, state, gridX, gridY);
            _previous = mouse;
        }

        private static void PlaceEntity(ECS_Base.LevelData.Level level, EditorStateComponent state, int x, int y)
        {
            var entity = new LevelEntity
            {
                Id = state.SelectedEntityType.ToString(),
                Iid = System.Guid.NewGuid().ToString(),
                Layer = "Entities",
                X = x,
                Y = y,
                Width = 16,
                Height = 16,
                Color = 0
            };

            if (!string.IsNullOrWhiteSpace(state.SelectedEntityKind))
                entity.CustomFields["Kind"] = state.SelectedEntityKind;

            switch (state.SelectedEntityType)
            {
                case EditorEntityType.Player:
                    level.Players.Clear();
                    level.Players.Add(entity);
                    break;
                case EditorEntityType.Enemy:
                    level.Enemies.Add(entity);
                    break;
                case EditorEntityType.Pickup:
                    level.Pickups.Add(entity);
                    break;
                case EditorEntityType.Hazard:
                    level.Hazards.Add(entity);
                    break;
                case EditorEntityType.Path:
                    level.Paths.Add(new PathEntity
                    {
                        Id = entity.Id,
                        Iid = entity.Iid,
                        Layer = entity.Layer,
                        X = entity.X,
                        Y = entity.Y,
                        Width = entity.Width,
                        Height = entity.Height,
                        Color = entity.Color
                    });
                    break;
            }
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
