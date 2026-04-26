using UnityEngine;
using System;
using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>
    /// Stack-based inventory for item definitions, with a legacy string overload for prototypes.
    /// </summary>
    public class InventorySystem : MonoBehaviour
    {
        [Serializable]
        public class InventoryItem
        {
            public ItemDefinition definition;
            public string itemName;
            public GameObject itemPrefab;
            public int count;
            public bool isVirtualItem;
            public string rarity;
            public string description;
            public DateTime expirationDate = DateTime.MaxValue;
            public int durability = 100;
            public float cooldownTime;
            public float lastUsedTime;
            public Action OnUse;

            public int quantity
            {
                get => count;
                set => count = value;
            }
        }

        [SerializeField] private int capacity = 24;
        private readonly List<InventoryItem> items = new List<InventoryItem>();
        private readonly Dictionary<string, InventoryItem> legacyLookup = new Dictionary<string, InventoryItem>();

        public IReadOnlyList<InventoryItem> Items => items;
        public IReadOnlyDictionary<string, InventoryItem> Inventory => legacyLookup;
        public int Capacity => capacity;

        public event Action<string> OnItemUsed;
        public event Action OnInventoryChanged;

        public bool AddItem(ItemDefinition definition, int count = 1)
        {
            if (definition == null || count <= 0) return false;

            int remaining = count;
            int maxStack = Mathf.Max(1, definition.maxStack);
            string key = GetKey(definition);

            for (int i = 0; i < items.Count && remaining > 0; i++)
            {
                InventoryItem item = items[i];
                if (item.definition != definition || item.count >= maxStack) continue;

                int add = Mathf.Min(maxStack - item.count, remaining);
                item.count += add;
                remaining -= add;
            }

            while (remaining > 0)
            {
                if (items.Count >= capacity)
                {
                    OnInventoryChanged?.Invoke();
                    return false;
                }

                int add = Mathf.Min(maxStack, remaining);
                var item = new InventoryItem
                {
                    definition = definition,
                    itemName = string.IsNullOrEmpty(definition.displayName) ? key : definition.displayName,
                    count = add
                };
                items.Add(item);
                legacyLookup[key] = item;
                remaining -= add;
            }

            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool RemoveItem(ItemDefinition definition, int count = 1)
        {
            if (definition == null || count <= 0) return false;
            if (GetTotalCount(definition) < count) return false;

            int remaining = count;
            for (int i = items.Count - 1; i >= 0 && remaining > 0; i--)
            {
                InventoryItem item = items[i];
                if (item.definition != definition) continue;

                int remove = Mathf.Min(item.count, remaining);
                item.count -= remove;
                remaining -= remove;

                if (item.count <= 0)
                    items.RemoveAt(i);
            }

            RefreshLegacyLookup();
            OnInventoryChanged?.Invoke();
            return true;
        }

        public void AddItem(string itemName, GameObject itemPrefab, int quantity, bool isVirtualItem = false, string rarity = "Common", string description = "", DateTime? expirationDate = null, int durability = 100, float cooldownTime = 0f, Action onUse = null)
        {
            if (quantity <= 0 || string.IsNullOrEmpty(itemName)) return;

            if (legacyLookup.TryGetValue(itemName, out InventoryItem existingItem) && existingItem.definition == null)
            {
                existingItem.count += quantity;
            }
            else
            {
                if (items.Count >= capacity) return;

                var item = new InventoryItem
                {
                    itemName = itemName,
                    itemPrefab = itemPrefab,
                    count = quantity,
                    isVirtualItem = isVirtualItem,
                    rarity = rarity,
                    description = description,
                    expirationDate = expirationDate ?? DateTime.MaxValue,
                    durability = durability,
                    cooldownTime = cooldownTime,
                    lastUsedTime = -cooldownTime,
                    OnUse = onUse
                };
                items.Add(item);
                legacyLookup[itemName] = item;
            }

            OnInventoryChanged?.Invoke();
        }

        public bool RemoveItem(string itemName, int quantity)
        {
            if (quantity <= 0 || string.IsNullOrEmpty(itemName)) return false;
            if (!legacyLookup.TryGetValue(itemName, out InventoryItem existingItem)) return false;
            if (existingItem.count < quantity) return false;

            existingItem.count -= quantity;
            if (existingItem.count <= 0)
            {
                items.Remove(existingItem);
                legacyLookup.Remove(itemName);
            }

            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool HasItem(string itemName, int quantity)
        {
            return legacyLookup.TryGetValue(itemName, out InventoryItem item)
                && item.count >= quantity
                && item.expirationDate > DateTime.Now
                && item.durability > 0;
        }

        public void UseItem(string itemName)
        {
            if (!HasItem(itemName, 1))
            {
                Debug.LogWarning("Cannot use item: " + itemName + " not found in inventory.");
                return;
            }

            InventoryItem item = legacyLookup[itemName];
            if (Time.time - item.lastUsedTime < item.cooldownTime)
            {
                Debug.LogWarning("Item " + itemName + " is on cooldown. Please wait.");
                return;
            }

            item.lastUsedTime = Time.time;
            if (item.durability > 0)
                item.durability -= 10;

            item.OnUse?.Invoke();
            OnItemUsed?.Invoke(itemName);

            if (!item.isVirtualItem && item.itemPrefab != null)
                Instantiate(item.itemPrefab);

            RemoveItem(itemName, 1);
        }

        private int GetTotalCount(ItemDefinition definition)
        {
            int total = 0;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].definition == definition)
                    total += items[i].count;
            }
            return total;
        }

        private void RefreshLegacyLookup()
        {
            legacyLookup.Clear();
            for (int i = 0; i < items.Count; i++)
            {
                InventoryItem item = items[i];
                string key = item.definition != null ? GetKey(item.definition) : item.itemName;
                if (!string.IsNullOrEmpty(key))
                    legacyLookup[key] = item;
            }
        }

        private static string GetKey(ItemDefinition definition)
        {
            return !string.IsNullOrEmpty(definition.itemId) ? definition.itemId : definition.name;
        }
    }
}
