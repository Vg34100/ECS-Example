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
                ClearDrag(world);
                _previous = mouse;
                return;
            }

            if (IsHoveringUI(world))
            {
                _previous = mouse;
                return;
            }

            var level = FindActiveLevel(world, state.ActiveLevelId);
            if (level == null)
            {
                ClearDrag(world);
                _previous = mouse;
                return;
            }

            var worldPos = ScreenToWorld(world, new Vector2(mouse.X, mouse.Y));
            int gridX = (int)((worldPos.X - level.X) / 16) * 16;
            int gridY = (int)((worldPos.Y - level.Y) / 16) * 16;
            var dragEntity = EnsureDragState(world);
            var drag = world.GetComponent<EditorDragStateComponent>(dragEntity);

            bool leftPressed = mouse.LeftButton == ButtonState.Pressed;
            bool leftJustPressed = leftPressed && _previous.LeftButton != ButtonState.Pressed;
            bool rightJustPressed = mouse.RightButton == ButtonState.Pressed && _previous.RightButton != ButtonState.Pressed;

            if (drag.IsDragging)
            {
                MoveDraggedEntity(level, drag, gridX, gridY);
            }

            if (rightJustPressed)
            {
                if (drag.IsDragging)
                {
                    RemoveDraggedEntity(level, drag);
                    drag.IsDragging = false;
                    drag.EntityIid = string.Empty;
                }
                else
                {
                    RemoveEntityAt(level, worldPos);
                }
            }
            else if (leftJustPressed)
            {
                if (drag.IsDragging)
                {
                    drag.IsDragging = false;
                    drag.EntityIid = string.Empty;
                }
                else if (TryFindEntityAt(level, worldPos, out var draggedType, out var draggedIid))
                {
                    drag.IsDragging = true;
                    drag.EntityType = draggedType;
                    drag.EntityIid = draggedIid;
                    MoveDraggedEntity(level, drag, gridX, gridY);
                }
                else
                {
                    var placed = PlaceEntity(level, state, gridX, gridY);
                    if (placed != null)
                    {
                        drag.IsDragging = true;
                        drag.EntityType = state.SelectedEntityType;
                        drag.EntityIid = placed.Iid;
                    }
                }
            }

            world.AddComponent(dragEntity, drag);
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

        private static Entity EnsureDragState(World world)
        {
            var entity = world.Query<EditorDragStateComponent>().FirstOrDefault();
            if (entity != null)
                return entity;

            var created = world.CreateEntity();
            world.AddComponent(created, new EditorDragStateComponent(false, EditorEntityType.Enemy, string.Empty));
            return created;
        }

        private static void ClearDrag(World world)
        {
            foreach (var entity in world.Query<EditorDragStateComponent>())
            {
                var drag = world.GetComponent<EditorDragStateComponent>(entity);
                drag.IsDragging = false;
                drag.EntityIid = string.Empty;
                world.AddComponent(entity, drag);
                return;
            }
        }

        private static LevelEntity PlaceEntity(ECS_Base.LevelData.Level level, EditorStateComponent state, int x, int y)
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
                    return entity;
                case EditorEntityType.Enemy:
                    level.Enemies.Add(entity);
                    return entity;
                case EditorEntityType.Pickup:
                    level.Pickups.Add(entity);
                    return entity;
                case EditorEntityType.Hazard:
                    level.Hazards.Add(entity);
                    return entity;
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
                    return entity;
            }

            return null;
        }

        private static bool TryFindEntityAt(ECS_Base.LevelData.Level level, Vector2 worldPos, out EditorEntityType entityType, out string iid)
        {
            if (TryFindEntityInList(level.Players, level, worldPos, out iid))
            {
                entityType = EditorEntityType.Player;
                return true;
            }
            if (TryFindEntityInList(level.Enemies, level, worldPos, out iid))
            {
                entityType = EditorEntityType.Enemy;
                return true;
            }
            if (TryFindEntityInList(level.Pickups, level, worldPos, out iid))
            {
                entityType = EditorEntityType.Pickup;
                return true;
            }
            if (TryFindEntityInList(level.Hazards, level, worldPos, out iid))
            {
                entityType = EditorEntityType.Hazard;
                return true;
            }

            entityType = EditorEntityType.Path;
            iid = string.Empty;
            return false;
        }

        private static bool TryFindEntityInList(System.Collections.Generic.List<LevelEntity> entities, ECS_Base.LevelData.Level level, Vector2 worldPos, out string iid)
        {
            for (int i = entities.Count - 1; i >= 0; i--)
            {
                var entity = entities[i];
                var rect = new Rectangle(level.X + entity.X, level.Y + entity.Y, entity.Width, entity.Height);
                if (rect.Contains(worldPos))
                {
                    iid = entity.Iid;
                    return true;
                }
            }

            iid = string.Empty;
            return false;
        }

        private static void MoveDraggedEntity(ECS_Base.LevelData.Level level, EditorDragStateComponent drag, int x, int y)
        {
            var entity = FindEntityByIid(level, drag.EntityType, drag.EntityIid);
            if (entity == null)
                return;

            entity.X = x;
            entity.Y = y;
        }

        private static void RemoveDraggedEntity(ECS_Base.LevelData.Level level, EditorDragStateComponent drag)
        {
            RemoveByIid(level.Players, drag.EntityIid);
            RemoveByIid(level.Enemies, drag.EntityIid);
            RemoveByIid(level.Pickups, drag.EntityIid);
            RemoveByIid(level.Hazards, drag.EntityIid);
        }

        private static void RemoveEntityAt(ECS_Base.LevelData.Level level, Vector2 worldPos)
        {
            if (RemoveFromList(level.Players, level, worldPos))
                return;
            if (RemoveFromList(level.Enemies, level, worldPos))
                return;
            if (RemoveFromList(level.Pickups, level, worldPos))
                return;
            RemoveFromList(level.Hazards, level, worldPos);
        }

        private static bool RemoveFromList(System.Collections.Generic.List<LevelEntity> entities, ECS_Base.LevelData.Level level, Vector2 worldPos)
        {
            for (int i = entities.Count - 1; i >= 0; i--)
            {
                var entity = entities[i];
                var rect = new Rectangle(level.X + entity.X, level.Y + entity.Y, entity.Width, entity.Height);
                if (rect.Contains(worldPos))
                {
                    entities.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        private static LevelEntity FindEntityByIid(ECS_Base.LevelData.Level level, EditorEntityType type, string iid)
        {
            return type switch
            {
                EditorEntityType.Player => level.Players.FirstOrDefault(e => e.Iid == iid),
                EditorEntityType.Enemy => level.Enemies.FirstOrDefault(e => e.Iid == iid),
                EditorEntityType.Pickup => level.Pickups.FirstOrDefault(e => e.Iid == iid),
                EditorEntityType.Hazard => level.Hazards.FirstOrDefault(e => e.Iid == iid),
                _ => null
            };
        }

        private static void RemoveByIid(System.Collections.Generic.List<LevelEntity> entities, string iid)
        {
            for (int i = entities.Count - 1; i >= 0; i--)
            {
                if (entities[i].Iid == iid)
                {
                    entities.RemoveAt(i);
                    return;
                }
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
