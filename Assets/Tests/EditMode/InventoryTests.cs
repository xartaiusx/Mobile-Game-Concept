#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;
using Game.Core;

public class InventoryTests
{
    [Test]
    public void AddAndRemoveStacks()
    {
        var go = new GameObject("inv");
        var inv = go.AddComponent<InventorySystem>();

        var item = ScriptableObject.CreateInstance<ItemDefinition>();
        item.maxStack = 10;

        Assert.IsTrue(inv.AddItem(item, 15)); // 10 + 5 in two stacks
        Assert.AreEqual(2, inv.Items.Count);

        Assert.IsTrue(inv.RemoveItem(item, 12)); // leaves 3 total
        int total = 0;
        foreach (var it in inv.Items) total += it.count;
        Assert.AreEqual(3, total);
    }
}
#endif
