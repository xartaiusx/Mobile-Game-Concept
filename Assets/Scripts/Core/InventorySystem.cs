using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

/// <summary>
/// Manages the player's inventory, including adding, removing, and using items.
/// </summary>
public class InventorySystem : MonoBehaviour
{
    [System.Serializable]
    public class InventoryItem
    {
        public string itemName;
        public GameObject itemPrefab;
        public int quantity;
        public bool isVirtualItem; // Determines if item is a virtual (non-prefab) item
        public string rarity; // Defines item rarity
        public string description; // Provides additional item details
        public DateTime expirationDate; // Expiry date for perishable items
        public int durability; // Durability for breakable items
        public float cooldownTime; // Time in seconds before the item can be used again
        public float lastUsedTime; // Timestamp of last item use
        public Action OnUse;
    }

    private Dictionary<string, InventoryItem> inventory = new Dictionary<string, InventoryItem>();

    public IReadOnlyDictionary<string, InventoryItem> Inventory => inventory;

    public event Action<string> OnItemUsed;

    public void AddItem(string itemName, GameObject itemPrefab, int quantity, bool isVirtualItem = false, string rarity = "Common", string description = "", DateTime? expirationDate = null, int durability = 100, float cooldownTime = 0f, Action onUse = null)
    {
        if (quantity <= 0)
        {
            Debug.LogWarning("Cannot add item: Quantity must be greater than zero.");
            return;
        }
        
        if (string.IsNullOrEmpty(itemName))
        {
            Debug.LogWarning("Cannot add item: Item name cannot be null or empty.");
            return;
        }
        
        if (inventory.TryGetValue(itemName, out InventoryItem existingItem))
        {
            existingItem.quantity += quantity;
        }
        else
        {
            inventory[itemName] = new InventoryItem 
            {
                itemName = itemName, 
                itemPrefab = itemPrefab, 
                quantity = quantity, 
                isVirtualItem = isVirtualItem, 
                rarity = rarity, 
                description = description, 
                expirationDate = expirationDate ?? DateTime.MaxValue,
                durability = durability,
                cooldownTime = cooldownTime,
                lastUsedTime = -cooldownTime, // Ensures item can be used immediately upon acquisition
                OnUse = onUse
            };
        }
    }

    public void RemoveItem(string itemName, int quantity)
    {
        if (quantity <= 0)
        {
            Debug.LogWarning("Cannot remove item: Quantity must be greater than zero.");
            return;
        }
        
        if (inventory.TryGetValue(itemName, out InventoryItem existingItem))
        {
            existingItem.quantity -= quantity;
            if (existingItem.quantity <= 0)
            {
                inventory.Remove(itemName);
                Debug.Log("Item completely removed from inventory: " + itemName);
            }
        }
    }

    public bool HasItem(string itemName, int quantity)
    {
        return inventory.TryGetValue(itemName, out InventoryItem existingItem) 
            && existingItem.quantity >= quantity
            && existingItem.expirationDate > DateTime.Now
            && existingItem.durability > 0;
    }

    public void UseItem(string itemName)
    {
        if (HasItem(itemName, 1))
        {
            InventoryItem item = inventory[itemName];

            if (Time.time - item.lastUsedTime < item.cooldownTime)
            {
                Debug.LogWarning("Item " + itemName + " is on cooldown. Please wait.");
                return;
            }
            
            item.lastUsedTime = Time.time; // Record last used time

            if (item.durability > 0)
            {
                item.durability -= 10; // Reduce durability on use (adjust as needed)
            }
            
            if (item.durability <= 0 || item.expirationDate <= DateTime.Now)
            {
                inventory.Remove(itemName);
                Debug.Log("Item expired or broke: " + itemName);
                return;
            }
            
            RemoveItem(itemName, 1);
            
            if (!item.isVirtualItem && item.itemPrefab != null)
            {
                Instantiate(item.itemPrefab);
            }
            
            item.OnUse?.Invoke();
            OnItemUsed?.Invoke(itemName);
        }
        else
        {
            Debug.LogWarning("Cannot use item: " + itemName + " not found in inventory.");
        }
    }
}
