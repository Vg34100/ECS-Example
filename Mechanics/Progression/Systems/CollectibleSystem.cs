using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Collision.Components;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.PlayerController.Components;
using ECS_Base.Mechanics.Progression.Components;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ECS_Base.Mechanics.Progression.Systems
{
    /// <summary>
    /// Handles collectible pickups for player.
    /// </summary>
    public class CollectibleSystem
    {
        public void Update(World world, float deltaTime)
        {
            var player = world.GetEntities().FirstOrDefault(e => world.HasComponent<PlayerComponent>(e));
            if (player == null)
                return;

            if (!world.TryGetComponent<PositionComponent>(player, out var playerPos) ||
                !world.TryGetComponent<ColliderComponent>(player, out var playerCollider))
                return;

            Rectangle playerBounds = new Rectangle(
                (int)playerPos.Value.X + playerCollider.Bounds.X,
                (int)playerPos.Value.Y + playerCollider.Bounds.Y,
                playerCollider.Bounds.Width,
                playerCollider.Bounds.Height
            );

            var toRemove = new List<Entity>();

            foreach (var entity in world.Query<CollectibleComponent, PositionComponent, ColliderComponent>())
            {
                var collectible = world.GetComponent<CollectibleComponent>(entity);
                var pos = world.GetComponent<PositionComponent>(entity);
                var col = world.GetComponent<ColliderComponent>(entity);

                Rectangle bounds = new Rectangle(
                    (int)pos.Value.X + col.Bounds.X,
                    (int)pos.Value.Y + col.Bounds.Y,
                    col.Bounds.Width,
                    col.Bounds.Height
                );

                if (!playerBounds.Intersects(bounds))
                    continue;

                if (world.TryGetComponent<CurrencyComponent>(player, out var currency))
                {
                    switch (collectible.Type)
                    {
                        case CollectibleType.Coin:
                            currency.Coins += collectible.Amount;
                            break;
                        case CollectibleType.Star:
                            currency.Stars += collectible.Amount;
                            break;
                        case CollectibleType.Rupee:
                            currency.Rupees += collectible.Amount;
                            break;
                    }

                    currency.Score += collectible.ScoreValue;
                    world.AddComponent(player, currency);
                }

                toRemove.Add(entity);
            }

            foreach (var entity in toRemove)
            {
                world.RemoveEntity(entity);
            }
        }
    }
}
