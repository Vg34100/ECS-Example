using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Editor.Components;
using ECS_Base.Mechanics.Level.Components;
using ECS_Base.LevelData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Linq;

namespace ECS_Base.Mechanics.Editor.Systems
{
    /// <summary>
    /// Simple overlay for editor state and entity markers.
    /// </summary>
    public class EditorOverlaySystem
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly SpriteFont _font;
        private readonly Texture2D _pixel;
        private readonly GraphicsDevice _graphicsDevice;
        private Texture2D _slimeTexture;

        public EditorOverlaySystem(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SpriteFont font)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _font = font;
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public void Draw(World world)
        {
            var stateEntity = world.Query<EditorStateComponent>().FirstOrDefault();
            if (stateEntity == null)
                return;

            var state = world.GetComponent<EditorStateComponent>(stateEntity);
            if (!state.IsEditing)
                return;

            var view = GetViewMatrix(world);

            _spriteBatch.Begin(transformMatrix: view, samplerState: SamplerState.PointClamp);
            var level = FindActiveLevel(world, state.ActiveLevelId);
            if (level != null)
            {
                if (ShouldShowGrid(world))
                    DrawGrid(level);

                DrawEntities(level, Color.LimeGreen); // Player
                DrawEntities(level, Color.Red, "Enemy");
                DrawEntities(level, Color.Yellow, "Pickup");
                DrawEntities(level, Color.LightGray, "Hazard");

                if (!IsHoveringUI(world))
                    DrawPreview(world, state, level);

                DrawDraggedMarker(world, level);
            }

            if (state.Tool == EditorTool.TileErase && level != null && !IsHoveringUI(world))
            {
                DrawEraseCursor(world, level);
            }
            _spriteBatch.End();

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            bool paletteOpen = false;
            foreach (var entity in world.Query<EditorUIStateComponent>())
            {
                paletteOpen = world.GetComponent<EditorUIStateComponent>(entity).ShowPalette;
                break;
            }
            string text = $"EDITOR | Tool: {state.Tool} | Tile: {state.SelectedTileValue} | Entity: {state.SelectedEntityType}:{state.SelectedEntityKind} | Palette: {(paletteOpen ? "On" : "Off")} | Level: {state.ActiveLevelId}";
            if (_font != null)
                _spriteBatch.DrawString(_font, text, new Vector2(8, 8), Color.White);
            _spriteBatch.End();
        }

        private void DrawEntities(ECS_Base.LevelData.Level level, Color color, string typeFilter = null)
        {
            if (typeFilter == null)
            {
                foreach (var p in level.Players)
                    DrawRect(level, p, color);
                return;
            }

            if (typeFilter == "Enemy")
            {
                foreach (var e in level.Enemies)
                    DrawEnemyRect(level, e, color);
            }
            else if (typeFilter == "Pickup")
            {
                foreach (var p in level.Pickups)
                    DrawRect(level, p, color);
            }
            else if (typeFilter == "Hazard")
            {
                foreach (var h in level.Hazards)
                    DrawRect(level, h, color);
            }
        }

        private void DrawRect(ECS_Base.LevelData.Level level, LevelEntity entity, Color color)
        {
            var rect = new Rectangle(level.X + entity.X, level.Y + entity.Y, entity.Width, entity.Height);
            _spriteBatch.Draw(_pixel, rect, color * 0.6f);
        }

        private void DrawEnemyRect(ECS_Base.LevelData.Level level, LevelEntity entity, Color color)
        {
            if (IsSlime(entity))
            {
                DrawSlimeSprite(new Rectangle(level.X + entity.X, level.Y + entity.Y, entity.Width, entity.Height), 0.75f);
                return;
            }

            DrawRect(level, entity, color);
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

        private static Matrix GetViewMatrix(World world)
        {
            var cameraEntity = world.GetEntities()
                .FirstOrDefault(e => world.TryGetComponent<Mechanics.Camera.Components.CameraComponent>(e, out var cam) && cam.IsActive);

            if (cameraEntity != null && world.TryGetComponent<Mechanics.Camera.Components.CameraComponent>(cameraEntity, out var camera))
            {
                return Matrix.CreateTranslation(-camera.Position.X, -camera.Position.Y, 0) *
                       Matrix.CreateScale(camera.Zoom, camera.Zoom, 1.0f);
            }

            return Matrix.Identity;
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

        private static bool ShouldShowGrid(World world)
        {
            foreach (var entity in world.Query<EditorUIStateComponent>())
            {
                return world.GetComponent<EditorUIStateComponent>(entity).ShowGrid;
            }
            return true;
        }

        private void DrawEraseCursor(World world, ECS_Base.LevelData.Level level)
        {
            var mouse = Microsoft.Xna.Framework.Input.Mouse.GetState();
            var worldPos = ScreenToWorld(world, new Vector2(mouse.X, mouse.Y));
            int col = (int)((worldPos.X - level.X) / 16);
            int row = (int)((worldPos.Y - level.Y) / 16);
            if (row < 0 || col < 0 || row >= level.TileData.GetLength(0) || col >= level.TileData.GetLength(1))
                return;

            int x = level.X + col * 16;
            int y = level.Y + row * 16;
            var rect = new Rectangle(x, y, 16, 16);
            _spriteBatch.Draw(_pixel, rect, Color.Red * 0.2f);
            _spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), Color.Red);
            _spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, 2, rect.Height), Color.Red);
            _spriteBatch.Draw(_pixel, new Rectangle(rect.Right - 2, rect.Y, 2, rect.Height), Color.Red);
            _spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Bottom - 2, rect.Width, 2), Color.Red);
        }

        private static Vector2 ScreenToWorld(World world, Vector2 screen)
        {
            var cameraEntity = world.GetEntities()
                .FirstOrDefault(e => world.TryGetComponent<Mechanics.Camera.Components.CameraComponent>(e, out var cam) && cam.IsActive);

            if (cameraEntity != null && world.TryGetComponent<Mechanics.Camera.Components.CameraComponent>(cameraEntity, out var camera))
            {
                return (screen / camera.Zoom) + camera.Position;
            }

            return screen;
        }

        private void DrawGrid(ECS_Base.LevelData.Level level)
        {
            if (level.TileData == null)
                return;

            int rows = level.TileData.GetLength(0);
            int cols = level.TileData.GetLength(1);
            int width = cols * 16;
            int height = rows * 16;

            for (int x = 0; x <= cols; x++)
            {
                int drawX = level.X + (x * 16);
                _spriteBatch.Draw(_pixel, new Rectangle(drawX, level.Y, 1, height), Color.Black * 0.2f);
            }

            for (int y = 0; y <= rows; y++)
            {
                int drawY = level.Y + (y * 16);
                _spriteBatch.Draw(_pixel, new Rectangle(level.X, drawY, width, 1), Color.Black * 0.2f);
            }
        }

        private void DrawPreview(World world, EditorStateComponent state, ECS_Base.LevelData.Level level)
        {
            var mouse = Microsoft.Xna.Framework.Input.Mouse.GetState();
            var worldPos = ScreenToWorld(world, new Vector2(mouse.X, mouse.Y));
            int gridX = (int)((worldPos.X - level.X) / 16) * 16;
            int gridY = (int)((worldPos.Y - level.Y) / 16) * 16;
            if (gridX < 0 || gridY < 0)
                return;

            var rect = new Rectangle(level.X + gridX, level.Y + gridY, 16, 16);

            if (state.Tool == EditorTool.TilePaint)
            {
                var color = GetTilePreviewColor(world, state.SelectedTileValue);
                _spriteBatch.Draw(_pixel, rect, color * 0.55f);
                return;
            }

            if (state.Tool == EditorTool.EntityPlace)
            {
                if (state.SelectedEntityType == EditorEntityType.Enemy && state.SelectedEntityKind == "Slime")
                {
                    DrawSlimeSprite(rect, 0.7f);
                    return;
                }

                var color = state.SelectedEntityType switch
                {
                    EditorEntityType.Player => Color.LimeGreen,
                    EditorEntityType.Enemy => Color.Red,
                    EditorEntityType.Pickup => Color.Yellow,
                    EditorEntityType.Hazard => Color.LightGray,
                    _ => Color.White
                };
                _spriteBatch.Draw(_pixel, rect, color * 0.45f);
            }
        }

        private static Color GetTilePreviewColor(World world, int tileValue)
        {
            foreach (var entity in world.Query<LevelTileColorConfigComponent>())
            {
                var config = world.GetComponent<LevelTileColorConfigComponent>(entity);
                if (config.TileColors != null && config.TileColors.TryGetValue(tileValue, out var color))
                    return color;
                return config.DefaultColor;
            }

            return Color.White;
        }

        private void DrawDraggedMarker(World world, ECS_Base.LevelData.Level level)
        {
            foreach (var entity in world.Query<EditorDragStateComponent>())
            {
                var drag = world.GetComponent<EditorDragStateComponent>(entity);
                if (!drag.IsDragging)
                    return;

                var dragged = FindDraggedEntity(level, drag);
                if (dragged == null)
                    return;

                var rect = new Rectangle(level.X + dragged.X, level.Y + dragged.Y, dragged.Width, dragged.Height);
                if (drag.EntityType == EditorEntityType.Enemy && IsSlime(dragged))
                {
                    DrawSlimeSprite(rect, 0.9f);
                }

                _spriteBatch.Draw(_pixel, rect, Color.Cyan * 0.22f);
                _spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), Color.Cyan);
                _spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, 2, rect.Height), Color.Cyan);
                _spriteBatch.Draw(_pixel, new Rectangle(rect.Right - 2, rect.Y, 2, rect.Height), Color.Cyan);
                _spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Bottom - 2, rect.Width, 2), Color.Cyan);
                return;
            }
        }

        private static LevelEntity FindDraggedEntity(ECS_Base.LevelData.Level level, EditorDragStateComponent drag)
        {
            if (drag.EntityType == EditorEntityType.Player)
                return level.Players.FirstOrDefault(e => e.Iid == drag.EntityIid);
            if (drag.EntityType == EditorEntityType.Enemy)
                return level.Enemies.FirstOrDefault(e => e.Iid == drag.EntityIid);
            if (drag.EntityType == EditorEntityType.Pickup)
                return level.Pickups.FirstOrDefault(e => e.Iid == drag.EntityIid);
            if (drag.EntityType == EditorEntityType.Hazard)
                return level.Hazards.FirstOrDefault(e => e.Iid == drag.EntityIid);
            return null;
        }

        private static bool IsSlime(LevelEntity entity)
        {
            if (!entity.CustomFields.TryGetValue("Kind", out var kindObj))
                return false;

            var kind = kindObj as string;
            return string.Equals(kind, "Slime", System.StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(kind, "Chaser", System.StringComparison.OrdinalIgnoreCase);
        }

        private void DrawSlimeSprite(Rectangle rect, float alpha)
        {
            EnsureSlimeTextureLoaded();
            if (_slimeTexture == null)
            {
                _spriteBatch.Draw(_pixel, rect, new Color(60, 180, 80) * alpha);
                return;
            }

            const int frameSize = 16;
            var source = new Rectangle(0, 0, frameSize, frameSize);
            _spriteBatch.Draw(_slimeTexture, rect, source, Color.White * alpha);
        }

        private void EnsureSlimeTextureLoaded()
        {
            if (_slimeTexture != null)
                return;

            var path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "../../../Assets/Sprites/slime.png");
            if (!File.Exists(path))
                return;

            using var stream = File.OpenRead(path);
            _slimeTexture = Texture2D.FromStream(_graphicsDevice, stream);
        }
    }
}
