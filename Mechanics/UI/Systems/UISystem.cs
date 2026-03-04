using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.UI.Components;
using Microsoft.Xna.Framework.Graphics;

namespace ECS_Base.Mechanics.UI.Systems
{
    /// <summary>
    /// Wrapper that allows UI systems to be driven by SystemManager.
    /// </summary>
    public class UISystem
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly SpriteFont _defaultFont;
        private readonly UITextSystem _textSystem;
        private readonly UIProgressBarSystem _progressBarSystem;
        private readonly UIButtonSystem _buttonSystem;
        private readonly UIHeartsSystem _heartsSystem;
        private readonly UISegmentedBarSystem _segmentedBarSystem;
        private readonly UICounterSystem _counterSystem;
        private readonly UIIconSystem _iconSystem;

        public UISystem(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SpriteFont defaultFont)
        {
            _spriteBatch = spriteBatch;
            _defaultFont = defaultFont;

            _textSystem = new UITextSystem();
            _progressBarSystem = new UIProgressBarSystem(graphicsDevice);
            _buttonSystem = new UIButtonSystem(graphicsDevice);
            _heartsSystem = new UIHeartsSystem(graphicsDevice);
            _segmentedBarSystem = new UISegmentedBarSystem(graphicsDevice);
            _counterSystem = new UICounterSystem();
            _iconSystem = new UIIconSystem(graphicsDevice);
        }

        public void Update(World world)
        {
            _buttonSystem.Update(world);
            _counterSystem.Update(world);
        }

        public void Draw(World world)
        {
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            ApplyDefaultFont(world);
            _segmentedBarSystem.Draw(world, _spriteBatch);
            _heartsSystem.Draw(world, _spriteBatch);
            _progressBarSystem.Draw(world, _spriteBatch);
            _iconSystem.Draw(world, _spriteBatch);
            _textSystem.Draw(world, _spriteBatch);
            _buttonSystem.Draw(world, _spriteBatch, _defaultFont);

            _spriteBatch.End();
        }

        private void ApplyDefaultFont(World world)
        {
            if (_defaultFont == null)
                return;

            foreach (var entity in world.Query<UITextComponent>())
            {
                if (!world.TryGetComponent<UITextComponent>(entity, out var text))
                    continue;

                if (text.Font != null)
                    continue;

                text.Font = _defaultFont;
                world.AddComponent(entity, text);
            }
        }
    }
}
