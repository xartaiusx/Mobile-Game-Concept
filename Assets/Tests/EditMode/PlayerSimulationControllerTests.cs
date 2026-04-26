#if UNITY_EDITOR
using Game.Core;
using NUnit.Framework;
using UnityEngine;

public class PlayerSimulationControllerTests
{
    [Test]
    public void SimulationCanBeEnabledWithoutThrowing()
    {
        var go = new GameObject("sim-player");
        go.AddComponent<CharacterController>();
        var simulation = go.AddComponent<PlayerSimulationController>();

        Assert.DoesNotThrow(() => simulation.SetSimulationEnabled(true));
        Assert.AreEqual(PlayerSimulationState.ApproachEnemy, simulation.State);
    }
}
#endif
