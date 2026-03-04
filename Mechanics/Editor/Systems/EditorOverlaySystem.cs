using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Editor.Components;
using ECS_Base.Mechanics.Level.Components;
using ECS_Base.LevelData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        public EditorOverlaySystem(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SpriteFont font)
        {
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
                DrawEntities(level, Color.LimeGreen); // Player
                DrawEntities(level, Color.Red, "Enemy");
                DrawEntities(level, Color.Yellow, "Pickup");
                DrawEntities(level, Color.LightGray, "Hazard");
            }
            _spriteBatch.End();

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            string text = $"EDITOR | Tool: {state.Tool} | Tile: {state.SelectedTileValue} | Entity: {state.SelectedEntityType}:{state.SelectedEntityKind} | Level: {state.ActiveLevelId}";
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
                    DrawRect(level, e, color);
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
    }
}
