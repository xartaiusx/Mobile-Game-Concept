#if UNITY_EDITOR
using Game.Animation;
using Game.Combat;
using NUnit.Framework;
using UnityEngine;

public class AnimationEventRelayTests
{
    [Test]
    public void RelayMethodsDoNotThrowWithoutAnimator()
    {
        var go = new GameObject("animation-relay");
        go.AddComponent<ComboSystem>();
        go.AddComponent<CombatAnimationBridge>();
        var relay = go.AddComponent<AnimationEventRelay>();

        Assert.DoesNotThrow(() => relay.BeginAttackActiveWindow());
        Assert.DoesNotThrow(() => relay.EndAttackActiveWindow());
        Assert.DoesNotThrow(() => relay.FinishRecovery());
        Assert.DoesNotThrow(() => relay.TriggerFootstep());
        Assert.DoesNotThrow(() => relay.TriggerWeaponSwing());
    }

    [Test]
    public void RelayMethodsDoNotThrowWithAnimatorPresent()
    {
        var go = new GameObject("animation-relay");
        go.AddComponent<Animator>();
        go.AddComponent<ComboSystem>();
        go.AddComponent<CombatAnimationBridge>();
        var relay = go.AddComponent<AnimationEventRelay>();

        Assert.DoesNotThrow(() => relay.BeginAttackActiveWindow());
        Assert.DoesNotThrow(() => relay.EndAttackActiveWindow());
        Assert.DoesNotThrow(() => relay.FinishRecovery());
    }

    [Test]
    public void RelayMethodsDoNotThrowWithMissingReceiver()
    {
        var go = new GameObject("animation-relay");
        var relay = go.AddComponent<AnimationEventRelay>();

        Assert.DoesNotThrow(() => relay.BeginAttackActiveWindow());
        Assert.DoesNotThrow(() => relay.EndAttackActiveWindow());
        Assert.DoesNotThrow(() => relay.FinishRecovery());
        Assert.DoesNotThrow(() => relay.TriggerFootstep());
        Assert.DoesNotThrow(() => relay.TriggerWeaponSwing());
    }
}
#endif
