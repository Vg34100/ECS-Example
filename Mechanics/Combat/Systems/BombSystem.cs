using ECS_Base.Mechanics.Combat.Components;
using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Input.Components;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.PlayerController.Components;
using ECS_Base.Mechanics.Progression.Components;
using ECS_Base.Mechanics.Rendering.Components;
using ECS_Base.Mechanics.Stats.Systems;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ECS_Base.Mechanics.Combat.Systems
{
    /// <summary>
    /// Spawns bombs and handles explosion damage.
    /// </summary>
    public class BombSystem
    {
        private readonly StatsSystem _statsSystem;

        public BombSystem(StatsSystem statsSystem)
        {
            _statsSystem = statsSystem;
        }

        public void Update(World world, float deltaTime)
        {
            var player = world.GetEntities().FirstOrDefault(e => world.HasComponent<PlayerComponent>(e));
            if (player != null && world.TryGetComponent<InputComponent>(player, out var input) &&
                input.Bomb)
            {
                if (world.TryGetComponent<AmmoComponent>(player, out var ammo) && ammo.Bombs > 0 &&
                    world.TryGetComponent<PositionComponent>(player, out var pos))
                {
                    ammo.Bombs -= 1;
                    world.AddComponent(player, ammo);
                    var spawnCenter = pos.Value;
                    if (world.TryGetComponent<Collision.Components.ColliderComponent>(player, out var col))
                    {
                        spawnCenter += new Vector2(col.Bounds.Width * 0.5f, col.Bounds.Height * 0.5f);
                    }
                    SpawnBomb(world, spawnCenter);
                }
            }

            var toRemove = new List<Entity>();
            foreach (var entity in world.Query<BombComponent, PositionComponent>())
            {
                var bomb = world.GetComponent<BombComponent>(entity);
                if (bomb.Exploded)
                {
                    toRemove.Add(entity);
                    continue;
                }
                bomb.TimeToExplode -= deltaTime;

                if (bomb.TimeToExplode <= 0f)
                {
                    Explode(world, entity, bomb);
                    bomb.Exploded = true;
                    world.AddComponent(entity, bomb);
                    toRemove.Add(entity);
                }
                else
                {
                    world.AddComponent(entity, bomb);
                }
            }

            foreach (var e in toRemove)
                world.RemoveEntity(e);
        }

        private void SpawnBomb(World world, Vector2 center)
        {
            const int pixelSize = 2;
            const int maskSize = 8;
            float iconSize = pixelSize * maskSize;
            var topLeft = center - new Vector2(iconSize * 0.5f, iconSize * 0.5f);

            var bomb = world.CreateEntity();
            world.AddComponent(bomb, new PositionComponent(topLeft.X, topLeft.Y));
            world.AddComponent(bomb, new BombComponent(timeToExplode: 1.2f, radius: 50f, damage: 3f));
            world.AddComponent(bomb, new WorldIconComponent(UI.Components.UIIconType.Bomb, Color.LightGray, pixelSize));
        }

        private void Explode(World world, Entity bombEntity, BombComponent bomb)
        {
            if (!world.TryGetComponent<PositionComponent>(bombEntity, out var pos))
                return;

            var explosionCenter = pos.Value;
            if (world.TryGetComponent<WorldIconComponent>(bombEntity, out var icon))
            {
                float iconSize = icon.PixelSize * 8f;
                explosionCenter = pos.Value + new Vector2(iconSize * 0.5f, iconSize * 0.5f);
            }

            var explosion = world.CreateEntity();
            world.AddComponent(explosion, new PositionComponent(explosionCenter.X, explosionCenter.Y));
            world.AddComponent(explosion, new ExplosionComponent(center: explosionCenter, duration: 0.4f, maxRadius: bomb.Radius, color: Color.Orange));
            world.AddComponent(explosion, new ShapeComponent(ShapeComponent.ShapeType.Circle, Color.Orange, new Vector2(8, 8)));

            foreach (var target in world.Query<Stats.Components.StatsComponent, PositionComponent>())
            {
                if (!world.TryGetComponent<PositionComponent>(target, out var targetPos))
                    continue;

                float dist = Vector2.Distance(explosionCenter, targetPos.Value);
                if (dist <= bomb.Radius)
                {
                    _statsSystem.ApplyDamage(world, target, bomb.Damage, bombEntity);
                }
            }
        }
    }
}
