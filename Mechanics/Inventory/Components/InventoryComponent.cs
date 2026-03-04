using ECS_Base.Mechanics.Inventory.Data;
using System.Collections.Generic;

namespace ECS_Base.Mechanics.Inventory.Components
{
    /// <summary>
    /// Inventory component for storing items
    /// </summary>
    public struct InventoryComponent
    {
        /// <summary>Items stored in inventory (can be null for empty slots)</summary>
        public List<Item?> Items;

        /// <summary>Maximum number of inventory slots</summary>
        public int MaxSlots;

        /// <summary>Number of occupied slots</summary>
        public readonly int UsedSlots
        {
            get
            {
                if (Items == null) return 0;
                int count = 0;
                foreach (var item in Items)
                {
                    if (item.HasValue && item.Value.StackSize > 0)
                        count++;
                }
                return count;
            }
        }

        /// <summary>Number of free slots</summary>
        public readonly int FreeSlots => MaxSlots - UsedSlots;

        /// <summary>Is inventory full?</summary>
        public readonly bool IsFull => UsedSlots >= MaxSlots;

        /// <summary>Is inventory empty?</summary>
        public readonly bool IsEmpty => UsedSlots == 0;

        public InventoryComponent(int maxSlots)
        {
            MaxSlots = maxSlots;
            Items = new List<Item?>(maxSlots);

            // Initialize with empty slots
            for (int i = 0; i < maxSlots; i++)
            {
                Items.Add(null);
            }
        }
    }
}
