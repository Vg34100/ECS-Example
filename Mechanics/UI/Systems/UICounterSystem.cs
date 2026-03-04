using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Progression.Components;
using ECS_Base.Mechanics.UI.Components;
using System.Linq;

namespace ECS_Base.Mechanics.UI.Systems
{
    /// <summary>
    /// Updates UITextComponent from CurrencyComponent values.
    /// </summary>
    public class UICounterSystem
    {
        public void Update(World world)
        {
            foreach (var entity in world.Query<UICounterComponent, UITextComponent>())
            {
                if (!world.TryGetComponent<UICounterComponent>(entity, out var counter))
                    continue;

                if (!world.TryGetComponent<UITextComponent>(entity, out var text))
                    continue;

                if (!TryGetCurrency(world, counter.TargetEntityId, out var currency, out var target))
                    continue;

                int value = counter.Type switch
                {
                    CounterType.Coins => currency.Coins,
                    CounterType.Stars => currency.Stars,
                    CounterType.Rupees => currency.Rupees,
                    CounterType.Score => currency.Score,
                    _ => 0
                };

                if (counter.Type == CounterType.Arrows || counter.Type == CounterType.Bombs)
                {
                    if (world.TryGetComponent(target, out ECS_Base.Mechanics.Progression.Components.AmmoComponent ammo))
                    {
                        value = counter.Type == CounterType.Arrows ? ammo.Arrows : ammo.Bombs;
                    }
                }

                text.Text = $"{counter.Prefix}{value}";
                world.AddComponent(entity, text);
            }
        }

        private bool TryGetCurrency(World world, int entityId, out CurrencyComponent currency, out Entity target)
        {
            currency = default;
            target = world.GetEntities().FirstOrDefault(e => e.Id == entityId);
            if (target == null)
                return false;

            return world.TryGetComponent(target, out currency);
        }
    }
}
