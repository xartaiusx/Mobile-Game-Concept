#if UNITY_EDITOR
using Game.Classes;
using Game.Core;
using NUnit.Framework;
using UnityEngine;

public class PickupTests
{
    [Test]
    public void InventoryAcceptsPickupItemDefinition()
    {
        var item = ScriptableObject.CreateInstance<ItemDefinition>();
        item.itemId = "gold";
        item.displayName = "Gold";
        item.maxStack = 999;

        var inventory = new GameObject("inventory").AddComponent<InventorySystem>();

        Assert.IsTrue(inventory.AddItem(item, 3));
        Assert.AreEqual(3, inventory.Items[0].count);
    }

    [Test]
    public void HealthPotionDefinitionCarriesHealAmount()
    {
        var potion = ScriptableObject.CreateInstance<ItemDefinition>();
        potion.itemId = "health_potion";
        potion.healAmount = 25;

        Assert.Greater(potion.healAmount, 0);
    }
}
#endif
