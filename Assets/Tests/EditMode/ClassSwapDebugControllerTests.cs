#if UNITY_EDITOR
using Game.Classes;
using Game.Core;
using NUnit.Framework;
using UnityEngine;

public class ClassSwapDebugControllerTests
{
    [TearDown]
    public void TearDown()
    {
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include))
            Object.DestroyImmediate(go);
    }

    [Test]
    public void ClassSwapReplacesPlayerWithoutDuplicatesAndUpdatesManager()
    {
        var manager = new GameObject("PlayerManager").AddComponent<PlayerManager>();
        var controller = new GameObject("ClassSwap").AddComponent<ClassSwapDebugController>();
        GameObject fighter = CreatePrefabLikePlayer<Fighter>("FighterPrefab");
        GameObject mage = CreatePrefabLikePlayer<Mage>("MagePrefab");
        controller.Configure(fighter, mage, null, null);

        GameObject first = controller.SwapToClass("Fighter");
        Assert.IsNotNull(first);

        GameObject second = controller.SwapToClass("Mage");

        Assert.IsNotNull(second);
        Assert.AreEqual(second.transform, manager.GetPlayerTransform());
        Assert.AreEqual(1, GameObject.FindGameObjectsWithTag("Player").Length);
        Assert.IsNotNull(second.GetComponent<Mage>());
    }

    private static GameObject CreatePrefabLikePlayer<T>(string name) where T : BaseCharacter
    {
        var go = new GameObject(name);
        go.tag = "Player";
        go.AddComponent<CharacterController>();
        go.AddComponent<T>();
        go.AddComponent<PlayerController>();
        return go;
    }
}
#endif
