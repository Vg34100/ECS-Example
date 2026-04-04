using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Inventory.Components;
using ECS_Base.Mechanics.Inventory.Data;
using System.Linq;

namespace ECS_Base.Mechanics.Inventory.Systems
{
    /// <summary>
    /// System for managing inventories
    /// </summary>
    public class InventorySystem
    {
        private readonly EventSystem _eventSystem;

        public InventorySystem(EventSystem eventSystem = null)
        {
            _eventSystem = eventSystem;
        }

        /// <summary>
        /// Add an item to inventory. Returns true if successful.
        /// </summary>
        public bool AddItem(World world, Entity entity, Item item)
        {
            if (!world.TryGetComponent<InventoryComponent>(entity, out var inventory))
                return false;

            // Try to stack with existing items first
            if (item.IsStackable)
            {
                for (int i = 0; i < inventory.Items.Count; i++)
                {
                    var slot = inventory.Items[i];
                    if (slot.HasValue && slot.Value.Id == item.Id && !slot.Value.IsStackFull)
                    {
                        var stackedItem = slot.Value;
                        int added = stackedItem.AddToStack(item.StackSize);
                        inventory.Items[i] = stackedItem;

                        if (added >= item.StackSize)
                        {
                            // Fully stacked
                            world.AddComponent(entity, inventory);
                            FireItemCollectedEvent(entity, item.Id);
                            return true;
                        }

                        // Partially stacked, reduce remaining
                        item.StackSize -= added;
                    }
                }
            }

            // Find empty slot
            for (int i = 0; i < inventory.Items.Count; i++)
            {
                if (!inventory.Items[i].HasValue || inventory.Items[i].Value.StackSize == 0)
                {
                    inventory.Items[i] = item;
                    world.AddComponent(entity, inventory);
                    FireItemCollectedEvent(entity, item.Id);
                    return true;
                }
            }

            // No space
            return false;
        }

        /// <summary>
        /// Remove an item by ID and amount. Returns amount actually removed.
        /// </summary>
        public int RemoveItem(World world, Entity entity, string itemId, int amount = 1)
        {
            if (!world.TryGetComponent<InventoryComponent>(entity, out var inventory))
                return 0;

            int totalRemoved = 0;
            int remaining = amount;

            for (int i = 0; i < inventory.Items.Count && remaining > 0; i++)
            {
                var slot = inventory.Items[i];
                if (slot.HasValue && slot.Value.Id == itemId)
                {
                    var slotItem = slot.Value;
                    int removed = slotItem.RemoveFromStack(remaining);
                    totalRemoved += removed;
                    remaining -= removed;

                    if (slotItem.StackSize <= 0)
                    {
                        inventory.Items[i] = null;
                    }
                    else
                    {
                        inventory.Items[i] = slotItem;
                    }
                }
            }

            if (totalRemoved > 0)
            {
                world.AddComponent(entity, inventory);
            }

            return totalRemoved;
        }

        /// <summary>
        /// Get total count of an item
        /// </summary>
        public int GetItemCount(World world, Entity entity, string itemId)
        {
            if (!world.TryGetComponent<InventoryComponent>(entity, out var inventory))
                return 0;

            int count = 0;
            foreach (var slot in inventory.Items)
            {
                if (slot.HasValue && slot.Value.Id == itemId)
                {
                    count += slot.Value.StackSize;
                }
            }

            return count;
        }

        /// <summary>
        /// Check if inventory has at least the specified amount of an item
        /// </summary>
        public bool HasItem(World world, Entity entity, string itemId, int amount = 1)
        {
            return GetItemCount(world, entity, itemId) >= amount;
        }

        /// <summary>
        /// Get all items in inventory
        /// </summary>
        public Item[] GetAllItems(World world, Entity entity)
        {
            if (!world.TryGetComponent<InventoryComponent>(entity, out var inventory))
                return new Item[0];

            return inventory.Items
                .Where(slot => slot.HasValue && slot.Value.StackSize > 0)
                .Select(slot => slot.Value)
                .ToArray();
        }

        /// <summary>
        /// Clear all items from inventory
        /// </summary>
        public void ClearInventory(World world, Entity entity)
        {
            if (!world.TryGetComponent<InventoryComponent>(entity, out var inventory))
                return;

            for (int i = 0; i < inventory.Items.Count; i++)
            {
                inventory.Items[i] = null;
            }

            world.AddComponent(entity, inventory);
        }

        /// <summary>
        /// Get first item of a specific type
        /// </summary>
        public Item? GetItemOfType(World world, Entity entity, ItemType type)
        {
            if (!world.TryGetComponent<InventoryComponent>(entity, out var inventory))
                return null;

            foreach (var slot in inventory.Items)
            {
                if (slot.HasValue && slot.Value.Type == type && slot.Value.StackSize > 0)
                {
                    return slot.Value;
                }
            }

            return null;
        }

        private void FireItemCollectedEvent(Entity collector, string itemId)
        {
            _eventSystem?.Publish(new ItemCollectedEvent(itemId, collector));
        }
    }
}
