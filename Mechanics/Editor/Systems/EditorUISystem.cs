using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Editor.Components;
using ECS_Base.Mechanics.Level.Components;
using ECS_Base.Mechanics.UI.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ECS_Base.Mechanics.Editor.Systems
{
    /// <summary>
    /// Basic Mario-Maker style UI: draw/erase buttons, hotbar, and palette grid.
    /// </summary>
    public class EditorUISystem : IAlwaysUpdateSystem
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteBatch _spriteBatch;
        private readonly SpriteFont _font;
        private readonly Texture2D _pixel;
        private Texture2D _slimeTexture;
        private MouseState _previous;
        private KeyboardState _previousKeyboard;
        private bool _wasEditing;
        private const int GridButtonWidth = 60;

        private const int SlotSize = 40;
        private const int SlotPadding = 6;
        private const int HotbarSlots = 10;
        private const int PanelPadding = 8;
        private const int ButtonWidth = 70;
        private const int ButtonHeight = 26;

        public EditorUISystem(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SpriteFont font)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _font = font;
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public void Update(World world, float deltaTime)
        {
            var mouse = Mouse.GetState();
            var keyboard = Keyboard.GetState();
            var stateEntity = world.Query<EditorStateComponent>().FirstOrDefault();
            if (stateEntity == null)
            {
                _previous = mouse;
                _previousKeyboard = keyboard;
                _wasEditing = false;
                return;
            }

            var state = world.GetComponent<EditorStateComponent>(stateEntity);
            if (!state.IsEditing)
            {
                _previous = mouse;
                _previousKeyboard = keyboard;
                _wasEditing = false;
                return;
            }

            var uiEntity = EnsureUIState(world);
            var uiState = world.GetComponent<EditorUIStateComponent>(uiEntity);

            var hotbarEntity = EnsureHotbar(world);
            var hotbar = world.GetComponent<EditorHotbarComponent>(hotbarEntity);

            var paletteEntity = EnsurePalette(world);
            var palette = world.GetComponent<EditorPaletteComponent>(paletteEntity);

            if (hotbar.Items.Count == 0)
            {
                var seed = state.Tool == EditorTool.EntityPlace
                    ? EditorPaletteItem.Entity(state.SelectedEntityType, state.SelectedEntityKind, state.SelectedEntityType.ToString())
                    : EditorPaletteItem.Tile(state.SelectedTileValue, $"Tile {state.SelectedTileValue}");
                PushHotbarItem(ref hotbar, seed);
            }

            if (uiState.ToggleCooldownFrames > 0)
                uiState.ToggleCooldownFrames--;

            if (uiState.SaveToastTime > 0f)
                uiState.SaveToastTime -= deltaTime;

            if (!_wasEditing)
            {
                _previous = mouse;
                _previousKeyboard = keyboard;
                uiState.ShowPalette = true;
                _wasEditing = true;
            }

            if (keyboard.IsKeyDown(Keys.P) && !_previousKeyboard.IsKeyDown(Keys.P) && uiState.ToggleCooldownFrames == 0)
            {
                uiState.ShowPalette = !uiState.ShowPalette;
                uiState.ToggleCooldownFrames = 2;
            }

            if (keyboard.IsKeyDown(Keys.G) && !_previousKeyboard.IsKeyDown(Keys.G))
            {
                uiState.ShowGrid = !uiState.ShowGrid;
            }

            var mousePoint = new Point(mouse.X, mouse.Y);
            bool click = mouse.LeftButton == ButtonState.Pressed && _previous.LeftButton != ButtonState.Pressed;
            int wheelDelta = mouse.ScrollWheelValue - _previous.ScrollWheelValue;

            var layout = BuildLayout();

            uiState.IsHoveringUI = layout.AllRects(uiState.ShowPalette).Any(r => r.Contains(mousePoint));

            if (click)
            {
                if (layout.DrawButton.Contains(mousePoint))
                {
                    state.Tool = state.Tool == EditorTool.EntityPlace ? EditorTool.EntityPlace : EditorTool.TilePaint;
                }
                else if (layout.EraseButton.Contains(mousePoint))
                {
                    state.Tool = EditorTool.TileErase;
                }
                else if (layout.PaletteButton.Contains(mousePoint) && uiState.ToggleCooldownFrames == 0)
                {
                    uiState.ShowPalette = !uiState.ShowPalette;
                    uiState.ToggleCooldownFrames = 2;
                }
                else if (layout.GridButton.Contains(mousePoint))
                {
                    uiState.ShowGrid = !uiState.ShowGrid;
                }
                else
                {
                    bool handled = false;
                    for (int i = 0; i < layout.HotbarSlots.Count; i++)
                    {
                        if (layout.HotbarSlots[i].Contains(mousePoint))
                        {
                            if (i < hotbar.Items.Count)
                            {
                                hotbar.SelectedIndex = i;
                                ApplySelection(hotbar.Items[i], ref state);
                            }
                            handled = true;
                            break;
                        }
                    }

                    if (!handled && uiState.ShowPalette)
                    {
                        for (int i = 0; i < layout.PaletteSlots.Count; i++)
                        {
                            if (layout.PaletteSlots[i].Contains(mousePoint))
                            {
                                if (i < palette.Items.Count)
                                {
                                    var item = palette.Items[i];
                                    ApplySelection(item, ref state);
                                    PushHotbarItem(ref hotbar, item);
                                }
                                handled = true;
                                break;
                            }
                        }
                    }
                }
            }

            if (wheelDelta != 0 && hotbar.Items.Count > 0 && !uiState.IsHoveringUI)
            {
                int direction = wheelDelta > 0 ? -1 : 1;
                hotbar.SelectedIndex = (hotbar.SelectedIndex + direction + hotbar.Items.Count) % hotbar.Items.Count;
                ApplySelection(hotbar.Items[hotbar.SelectedIndex], ref state);
            }

            world.AddComponent(stateEntity, state);
            world.AddComponent(uiEntity, uiState);
            world.AddComponent(hotbarEntity, hotbar);
            world.AddComponent(paletteEntity, palette);

            _previous = mouse;
            _previousKeyboard = keyboard;
            _wasEditing = true;
        }

        public void Draw(World world)
        {
            var stateEntity = world.Query<EditorStateComponent>().FirstOrDefault();
            if (stateEntity == null)
                return;

            var state = world.GetComponent<EditorStateComponent>(stateEntity);
            if (!state.IsEditing)
                return;

            var uiEntity = EnsureUIState(world);
            var uiState = world.GetComponent<EditorUIStateComponent>(uiEntity);

            var hotbarEntity = EnsureHotbar(world);
            var hotbar = world.GetComponent<EditorHotbarComponent>(hotbarEntity);

            var paletteEntity = EnsurePalette(world);
            var palette = world.GetComponent<EditorPaletteComponent>(paletteEntity);

            var tileColors = GetTileColors(world);

            var layout = BuildLayout();

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            DrawButton(layout.DrawButton, "Draw", state.Tool == EditorTool.TilePaint || state.Tool == EditorTool.EntityPlace);
            DrawButton(layout.EraseButton, "Erase", state.Tool == EditorTool.TileErase);
            DrawButton(layout.PaletteButton, "Palette", uiState.ShowPalette);
            DrawButton(layout.GridButton, "Grid", uiState.ShowGrid);

            DrawPanel(layout.HotbarBounds, new Color(20, 20, 20, 220));
            for (int i = 0; i < layout.HotbarSlots.Count; i++)
            {
                bool selected = i == hotbar.SelectedIndex;
                DrawSlot(layout.HotbarSlots[i], selected);
                if (i < hotbar.Items.Count)
                    DrawSlotItem(layout.HotbarSlots[i], hotbar.Items[i], tileColors);
            }

            if (uiState.ShowPalette)
            {
                DrawPanel(layout.PaletteBounds, new Color(18, 18, 18, 230));
                for (int i = 0; i < layout.PaletteSlots.Count; i++)
                {
                    DrawSlot(layout.PaletteSlots[i], false);
                    if (i < palette.Items.Count)
                        DrawSlotItem(layout.PaletteSlots[i], palette.Items[i], tileColors);
                }
            }

            if (uiState.SaveToastTime > 0f && _font != null)
            {
                var toast = "Saved";
                var size = _font.MeasureString(toast);
                var pos = new Vector2(
                    (_graphicsDevice.Viewport.Width - size.X) * 0.5f,
                    _graphicsDevice.Viewport.Height - 96f
                );
                _spriteBatch.DrawString(_font, toast, pos, Color.White);
            }

            _spriteBatch.End();
        }

        private static Entity EnsureUIState(World world)
        {
            var entity = world.Query<EditorUIStateComponent>().FirstOrDefault();
            if (entity != null)
                return entity;

            var created = world.CreateEntity();
            world.AddComponent(created, new EditorUIStateComponent(showPalette: false));
            return created;
        }

        private static Entity EnsureHotbar(World world)
        {
            var entity = world.Query<EditorHotbarComponent>().FirstOrDefault();
            if (entity != null)
                return entity;

            var created = world.CreateEntity();
            world.AddComponent(created, new EditorHotbarComponent(HotbarSlots));
            return created;
        }

        private static Entity EnsurePalette(World world)
        {
            var entity = world.Query<EditorPaletteComponent>().FirstOrDefault();
            if (entity != null)
                return entity;

            var created = world.CreateEntity();
            var items = BuildPaletteItems(world);
            world.AddComponent(created, new EditorPaletteComponent(items));
            return created;
        }

        private static List<EditorPaletteItem> BuildPaletteItems(World world)
        {
            var mode = LevelSpawnMode.Platformer;
            foreach (var entity in world.Query<LevelSpawnConfigComponent>())
            {
                mode = world.GetComponent<LevelSpawnConfigComponent>(entity).Mode;
                break;
            }

            if (mode == LevelSpawnMode.Topdown)
            {
                return new List<EditorPaletteItem>
                {
                    EditorPaletteItem.Tile(10, "Grass"),
                    EditorPaletteItem.Tile(11, "Dirt"),
                    EditorPaletteItem.Tile(12, "Water"),
                    EditorPaletteItem.Tile(13, "Cliff"),
                    EditorPaletteItem.Tile(14, "Tree"),
                    EditorPaletteItem.Tile(15, "Bridge"),
                    EditorPaletteItem.Entity(EditorEntityType.Player, "", "Player"),
                    EditorPaletteItem.Entity(EditorEntityType.Enemy, "Slime", "Slime"),
                    EditorPaletteItem.Entity(EditorEntityType.Enemy, "Shooter", "Shooter"),
                    EditorPaletteItem.Entity(EditorEntityType.Pickup, "Rupee", "Rupee", UIIconType.Rupee, true),
                    EditorPaletteItem.Entity(EditorEntityType.Pickup, "Heart", "Heart", UIIconType.Star, true),
                    EditorPaletteItem.Entity(EditorEntityType.Pickup, "Arrow", "Arrow", UIIconType.Arrow, true),
                    EditorPaletteItem.Entity(EditorEntityType.Pickup, "Bomb", "Bomb", UIIconType.Bomb, true)
                };
            }

            return new List<EditorPaletteItem>
            {
                EditorPaletteItem.Tile(1, "Ground"),
                EditorPaletteItem.Tile(2, "Rock"),
                EditorPaletteItem.Tile(3, "Brick"),
                EditorPaletteItem.Tile(4, "Question"),
                EditorPaletteItem.Tile(5, "Pipe"),
                EditorPaletteItem.Tile(6, "Spike"),
                EditorPaletteItem.Entity(EditorEntityType.Player, "", "Player"),
                EditorPaletteItem.Entity(EditorEntityType.Enemy, "Goomba", "Goomba"),
                EditorPaletteItem.Entity(EditorEntityType.Pickup, "Coin", "Coin", UIIconType.Coin, true),
                EditorPaletteItem.Entity(EditorEntityType.Pickup, "Star", "Star", UIIconType.Star, true),
                EditorPaletteItem.Entity(EditorEntityType.Pickup, "Mushroom", "Mushroom", UIIconType.Mushroom, true),
                EditorPaletteItem.Entity(EditorEntityType.Hazard, "Spike", "Hazard")
            };
        }

        private static Dictionary<int, Color> GetTileColors(World world)
        {
            foreach (var entity in world.Query<LevelTileColorConfigComponent>())
            {
                return world.GetComponent<LevelTileColorConfigComponent>(entity).TileColors;
            }
            return new Dictionary<int, Color>();
        }

        private void ApplySelection(EditorPaletteItem item, ref EditorStateComponent state)
        {
            if (item.IsTile)
            {
                state.SelectedTileValue = item.TileValue;
                state.Tool = state.Tool == EditorTool.TileErase ? EditorTool.TileErase : EditorTool.TilePaint;
            }
            else
            {
                state.SelectedEntityType = item.EntityType;
                state.SelectedEntityKind = item.EntityKind;
                state.Tool = EditorTool.EntityPlace;
            }
        }

        private static void PushHotbarItem(ref EditorHotbarComponent hotbar, EditorPaletteItem item)
        {
            int existing = hotbar.Items.FindIndex(i => i.Equals(item));
            if (existing >= 0)
            {
                hotbar.Items.RemoveAt(existing);
            }

            hotbar.Items.Insert(0, item);
            if (hotbar.Items.Count > hotbar.MaxItems)
                hotbar.Items.RemoveAt(hotbar.Items.Count - 1);

            hotbar.SelectedIndex = 0;
        }

        private void DrawPanel(Rectangle rect, Color color)
        {
            _spriteBatch.Draw(_pixel, rect, color);
        }

        private void DrawButton(Rectangle rect, string text, bool active)
        {
            Color bg = active ? new Color(70, 120, 200, 230) : new Color(40, 40, 40, 220);
            _spriteBatch.Draw(_pixel, rect, bg);
            if (_font != null)
            {
                var size = _font.MeasureString(text);
                var pos = new Vector2(rect.X + (rect.Width - size.X) / 2f, rect.Y + (rect.Height - size.Y) / 2f);
                _spriteBatch.DrawString(_font, text, pos, Color.White);
            }
        }

        private void DrawSlot(Rectangle rect, bool selected)
        {
            Color bg = selected ? new Color(90, 90, 120, 220) : new Color(30, 30, 30, 220);
            _spriteBatch.Draw(_pixel, rect, bg);
            _spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), Color.Black);
            _spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, 2, rect.Height), Color.Black);
            _spriteBatch.Draw(_pixel, new Rectangle(rect.Right - 2, rect.Y, 2, rect.Height), Color.Black);
            _spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Bottom - 2, rect.Width, 2), Color.Black);
        }

        private void DrawSlotItem(Rectangle rect, EditorPaletteItem item, Dictionary<int, Color> tileColors)
        {
            if (item.IsTile)
            {
                tileColors.TryGetValue(item.TileValue, out var color);
                if (color == default)
                    color = new Color(90, 90, 90);
                var inner = new Rectangle(rect.X + 6, rect.Y + 6, rect.Width - 12, rect.Height - 12);
                _spriteBatch.Draw(_pixel, inner, color);
                DrawLabel(rect, item.Label);
            }
            else
            {
                if (IsSlimeItem(item))
                {
                    DrawSlimePreview(rect);
                }
                else if (item.UseIcon)
                {
                    DrawIcon(rect, item.Icon, Color.White);
                }
                DrawLabel(rect, item.Label);
            }
        }

        private bool IsSlimeItem(EditorPaletteItem item)
        {
            return item.EntityType == EditorEntityType.Enemy &&
                   (item.EntityKind == "Slime" || item.EntityKind == "Chaser");
        }

        private void DrawSlimePreview(Rectangle rect)
        {
            EnsureSlimeTextureLoaded();
            if (_slimeTexture == null)
            {
                var fallback = new Rectangle(rect.X + 8, rect.Y + 8, rect.Width - 16, rect.Height - 16);
                _spriteBatch.Draw(_pixel, fallback, new Color(60, 180, 80));
                return;
            }

            const int frameSize = 16;
            var source = new Rectangle(0, 0, frameSize, frameSize);
            var size = rect.Width - 12;
            var dest = new Rectangle(rect.X + (rect.Width - size) / 2, rect.Y + 4, size, size);
            _spriteBatch.Draw(_slimeTexture, dest, source, Color.White);
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

        private void DrawLabel(Rectangle rect, string text)
        {
            if (_font == null || string.IsNullOrWhiteSpace(text))
                return;
            string label = text.Length > 8 ? text.Substring(0, 8) : text;
            var size = _font.MeasureString(label);
            var pos = new Vector2(rect.X + (rect.Width - size.X) / 2f, rect.Bottom - size.Y - 2);
            _spriteBatch.DrawString(_font, label, pos, Color.White);
        }

        private void DrawIcon(Rectangle rect, UIIconType icon, Color color)
        {
            // Simple 8x8 pixel icon block
            int iconSize = 8;
            int pixelSize = 2;
            var iconPos = new Vector2(rect.X + (rect.Width - iconSize * pixelSize) / 2f, rect.Y + 6);

            string[] mask = icon switch
            {
                UIIconType.Coin => new[] { "00111100","01111110","11111111","11111111","11111111","11111111","01111110","00111100" },
                UIIconType.Star => new[] { "00100100","00100100","11111111","01111110","00111100","01111110","11111111","00100100" },
                UIIconType.Rupee => new[] { "00011000","00111100","01111110","11111111","11111111","01111110","00111100","00011000" },
                UIIconType.Arrow => new[] { "00011000","00111100","01111110","11111111","00011000","00011000","00011000","00011000" },
                UIIconType.Mushroom => new[] { "01111110","11111111","11111111","11111111","01111110","00111100","00111100","01111110" },
                UIIconType.Bomb => new[] { "00111100","01111110","11111111","11111111","11111111","01111110","00111100","00011000" },
                _ => new[] { "00111100","01111110","11111111","11111111","11111111","11111111","01111110","00111100" }
            };

            for (int y = 0; y < mask.Length; y++)
            {
                for (int x = 0; x < mask[y].Length; x++)
                {
                    if (mask[y][x] != '1')
                        continue;
                    _spriteBatch.Draw(_pixel, new Rectangle((int)iconPos.X + x * pixelSize, (int)iconPos.Y + y * pixelSize, pixelSize, pixelSize), color);
                }
            }
        }

        private struct EditorLayout
        {
            public Rectangle DrawButton;
            public Rectangle EraseButton;
            public Rectangle PaletteButton;
            public Rectangle GridButton;
            public Rectangle HotbarBounds;
            public List<Rectangle> HotbarSlots;
            public Rectangle PaletteBounds;
            public List<Rectangle> PaletteSlots;
            public List<Rectangle> AllRects(bool includePalette)
            {
                var list = new List<Rectangle>
                {
                    DrawButton,
                    EraseButton,
                    PaletteButton,
                    GridButton,
                    HotbarBounds
                };
                list.AddRange(HotbarSlots);
                if (includePalette)
                {
                    list.Add(PaletteBounds);
                    list.AddRange(PaletteSlots);
                }
                return list;
            }
        }

        private EditorLayout BuildLayout()
        {
            int width = _graphicsDevice.Viewport.Width;
            int height = _graphicsDevice.Viewport.Height;

            var drawButton = new Rectangle(width - ButtonWidth * 3 - GridButtonWidth - PanelPadding * 4, PanelPadding, ButtonWidth, ButtonHeight);
            var eraseButton = new Rectangle(width - ButtonWidth * 2 - GridButtonWidth - PanelPadding * 3, PanelPadding, ButtonWidth, ButtonHeight);
            var paletteButton = new Rectangle(width - ButtonWidth - GridButtonWidth - PanelPadding * 2, PanelPadding, ButtonWidth, ButtonHeight);
            var gridButton = new Rectangle(width - GridButtonWidth - PanelPadding, PanelPadding, GridButtonWidth, ButtonHeight);

            int hotbarWidth = HotbarSlots * SlotSize + (HotbarSlots - 1) * SlotPadding;
            int hotbarX = (width - hotbarWidth) / 2;
            int hotbarY = height - SlotSize - PanelPadding;
            var hotbarBounds = new Rectangle(hotbarX - PanelPadding, hotbarY - PanelPadding, hotbarWidth + PanelPadding * 2, SlotSize + PanelPadding * 2);

            var hotbarSlots = new List<Rectangle>();
            for (int i = 0; i < HotbarSlots; i++)
            {
                int x = hotbarX + i * (SlotSize + SlotPadding);
                hotbarSlots.Add(new Rectangle(x, hotbarY, SlotSize, SlotSize));
            }

            int paletteCols = 6;
            int paletteRows = 3;
            int paletteWidth = paletteCols * SlotSize + (paletteCols - 1) * SlotPadding;
            int paletteHeight = paletteRows * SlotSize + (paletteRows - 1) * SlotPadding;
            int paletteX = width - paletteWidth - PanelPadding * 2;
            int paletteY = drawButton.Bottom + PanelPadding * 2;
            var paletteBounds = new Rectangle(paletteX - PanelPadding, paletteY - PanelPadding, paletteWidth + PanelPadding * 2, paletteHeight + PanelPadding * 2);

            var paletteSlots = new List<Rectangle>();
            for (int row = 0; row < paletteRows; row++)
            {
                for (int col = 0; col < paletteCols; col++)
                {
                    int x = paletteX + col * (SlotSize + SlotPadding);
                    int y = paletteY + row * (SlotSize + SlotPadding);
                    paletteSlots.Add(new Rectangle(x, y, SlotSize, SlotSize));
                }
            }

            return new EditorLayout
            {
                DrawButton = drawButton,
                EraseButton = eraseButton,
                PaletteButton = paletteButton,
                GridButton = gridButton,
                HotbarBounds = hotbarBounds,
                HotbarSlots = hotbarSlots,
                PaletteBounds = paletteBounds,
                PaletteSlots = paletteSlots
            };
        }
    }
}
