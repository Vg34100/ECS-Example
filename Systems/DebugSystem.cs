// Systems/DebugSystem.cs - Enhanced with patrol debugging
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using ECS_Example.Components;
using System.Text;
using System.Linq;

namespace ECS_Example.Systems
{
    public class DebugSystem
    {
        private KeyboardState _previousKeyboardState;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;
        private CameraSystem _cameraSystem;

        // Debug flags
        public bool ShowPlayerInfo { get; private set; } = false;
        public bool ShowCollisionInfo { get; private set; } = false;
        public bool ShowTileCollisions { get; private set; } = false;
        public bool ShowEntityCollisions { get; private set; } = false;
        public bool ShowSystemInfo { get; private set; } = false;
        public bool ShowPatrolDebug { get; private set; } = false; // New patrol debug

        // Collision side tracking for player
        public bool PlayerTouchingLeft { get; set; } = false;
        public bool PlayerTouchingRight { get; set; } = false;
        public bool PlayerTouchingTop { get; set; } = false;
        public bool PlayerTouchingBottom { get; set; } = false;

        public DebugSystem(SpriteBatch spriteBatch, SpriteFont font, CameraSystem cameraSystem)
        {
            _spriteBatch = spriteBatch;
            _font = font;
            _cameraSystem = cameraSystem;
            _previousKeyboardState = Keyboard.GetState();
        }

        public void Update(World world, float deltaTime)
        {
            var keyboardState = Keyboard.GetState();

            // Toggle debug modes with F keys
            if (keyboardState.IsKeyDown(Keys.F1) && _previousKeyboardState.IsKeyUp(Keys.F1))
            {
                ShowPlayerInfo = !ShowPlayerInfo;
            }
            if (keyboardState.IsKeyDown(Keys.F2) && _previousKeyboardState.IsKeyUp(Keys.F2))
            {
                ShowCollisionInfo = !ShowCollisionInfo;
            }
            if (keyboardState.IsKeyDown(Keys.F3) && _previousKeyboardState.IsKeyUp(Keys.F3))
            {
                ShowTileCollisions = !ShowTileCollisions;
            }
            if (keyboardState.IsKeyDown(Keys.F4) && _previousKeyboardState.IsKeyUp(Keys.F4))
            {
                ShowEntityCollisions = !ShowEntityCollisions;
            }
            if (keyboardState.IsKeyDown(Keys.F5) && _previousKeyboardState.IsKeyUp(Keys.F5))
            {
                ShowSystemInfo = !ShowSystemInfo;
            }
            if (keyboardState.IsKeyDown(Keys.F6) && _previousKeyboardState.IsKeyUp(Keys.F6))
            {
                ShowPatrolDebug = !ShowPatrolDebug;
            }

            _previousKeyboardState = keyboardState;
        }

        public void Draw(World world)
        {
            if (!ShowPlayerInfo && !ShowCollisionInfo && !ShowSystemInfo && !ShowPatrolDebug)
                return;

            // Draw UI elements without camera transform
            _spriteBatch.Begin();

            var debugText = new StringBuilder();
            var player = world.GetEntities().FirstOrDefault(e => world.TryGetComponent<PlayerController>(e, out var cam) && cam.MoveSpeed > 0);

            // F key instructions
            debugText.AppendLine("=== DEBUG CONTROLS ===");
            debugText.AppendLine($"F1: Player Info [{(ShowPlayerInfo ? "ON" : "OFF")}]");
            debugText.AppendLine($"F2: Collision Info [{(ShowCollisionInfo ? "ON" : "OFF")}]");
            debugText.AppendLine($"F3: Tile Collisions [{(ShowTileCollisions ? "ON" : "OFF")}]");
            debugText.AppendLine($"F4: Entity Collisions [{(ShowEntityCollisions ? "ON" : "OFF")}]");
            debugText.AppendLine($"F5: System Info [{(ShowSystemInfo ? "ON" : "OFF")}]");
            debugText.AppendLine($"F6: Patrol Debug [{(ShowPatrolDebug ? "ON" : "OFF")}]");
            debugText.AppendLine();

            if (ShowPlayerInfo && player != null)
            {
                debugText.AppendLine("=== PLAYER INFO ===");

                if (world.TryGetComponent<Position>(player, out var pos))
                {
                    debugText.AppendLine($"Position: ({pos.Value.X:F1}, {pos.Value.Y:F1})");
                }

                if (world.TryGetComponent<Velocity>(player, out var vel))
                {
                    debugText.AppendLine($"Velocity: ({vel.Value.X:F1}, {vel.Value.Y:F1})");
                    debugText.AppendLine($"Speed: {vel.Value.Length():F1}");
                }

                if (world.TryGetComponent<Gravity>(player, out var gravity))
                {
                    debugText.AppendLine($"Grounded: {gravity.IsGrounded}");
                    debugText.AppendLine($"Gravity: {gravity.Value}");
                }

                if (world.TryGetComponent<Health>(player, out var health))
                {
                    debugText.AppendLine($"Health: {health.CurrentHealth}/{health.MaxHealth}");
                }

                if (world.TryGetComponent<Attack>(player, out var attack))
                {
                    debugText.AppendLine($"Attacking: {attack.IsAttacking}");
                    debugText.AppendLine($"Attack Cooldown: {attack.TimeUntilNextAttack:F2}");
                    debugText.AppendLine($"Facing Right: {attack.IsFacingRight}");
                }

                if (world.TryGetComponent<Invulnerable>(player, out var invuln))
                {
                    debugText.AppendLine($"Invulnerable: {invuln.TimeRemaining:F2}s");
                }

                if (world.TryGetComponent<Stunned>(player, out var stun))
                {
                    debugText.AppendLine($"Stunned: {stun.TimeRemaining:F2}s");
                }

                debugText.AppendLine();
            }

            if (ShowCollisionInfo && player != null)
            {
                debugText.AppendLine("=== COLLISION INFO ===");
                debugText.AppendLine($"Touching Left: {PlayerTouchingLeft}");
                debugText.AppendLine($"Touching Right: {PlayerTouchingRight}");
                debugText.AppendLine($"Touching Top: {PlayerTouchingTop}");
                debugText.AppendLine($"Touching Bottom: {PlayerTouchingBottom}");
                debugText.AppendLine();
            }

            if (ShowPatrolDebug)
            {
                debugText.AppendLine("=== PATROL DEBUG ===");
                var enemies = world.GetEntities().Where(e => world.TryGetComponent<Patrol>(e, out _)).ToList();
                debugText.AppendLine($"Patrol Enemies: {enemies.Count}");

                for (int i = 0; i < enemies.Count && i < 3; i++) // Show first 3 enemies
                {
                    var enemy = enemies[i];
                    if (world.TryGetComponent<Patrol>(enemy, out var patrol) &&
                        world.TryGetComponent<Position>(enemy, out var pos) &&
                        world.TryGetComponent<Velocity>(enemy, out var vel))
                    {
                        debugText.AppendLine($"Enemy {i + 1}:");
                        debugText.AppendLine($"  Pos: ({pos.Value.X:F0}, {pos.Value.Y:F0})");
                        debugText.AppendLine($"  Vel: ({vel.Value.X:F1}, {vel.Value.Y:F1})");
                        debugText.AppendLine($"  Facing Right: {patrol.FacingRight}");
                        debugText.AppendLine($"  Speed: {patrol.Speed}");
                        debugText.AppendLine($"  Avoid Falls: {patrol.AvoidFalling}");
                    }
                }
                debugText.AppendLine();
            }

            if (ShowSystemInfo)
            {
                debugText.AppendLine("=== SYSTEM INFO ===");
                debugText.AppendLine($"Total Entities: {world.GetEntities().Count}");
                debugText.AppendLine($"Players: {world.GetComponentCount<PlayerController>()}");
                debugText.AppendLine($"Enemies: {world.GetComponentCount<Patrol>()}");
                debugText.AppendLine($"Health Entities: {world.GetComponentCount<Health>()}");
                debugText.AppendLine($"Physics Entities: {world.GetComponentCount<Velocity>()}");

                // Camera info
                var camera = world.GetEntities().FirstOrDefault(e => world.TryGetComponent<Camera>(e, out _));
                if (camera != null && world.TryGetComponent<Camera>(camera, out var cam))
                {
                    debugText.AppendLine($"Camera Pos: ({cam.Position.X:F1}, {cam.Position.Y:F1})");
                    debugText.AppendLine($"Camera Zoom: {cam.Zoom:F2}");
                }
                debugText.AppendLine();
            }

            // Draw the debug text
            _spriteBatch.DrawString(_font, debugText.ToString(), new Vector2(10, 10), Color.White);

            _spriteBatch.End();
        }

        public void ResetCollisionFlags()
        {
            PlayerTouchingLeft = false;
            PlayerTouchingRight = false;
            PlayerTouchingTop = false;
            PlayerTouchingBottom = false;
        }
    }
}