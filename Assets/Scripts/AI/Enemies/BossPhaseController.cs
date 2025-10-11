using UnityEngine;
using System.Collections.Generic;

namespace Game.AI.Enemies
{
    /// <summary>
    /// Switches boss behavior based on health thresholds defined by phase data.
    /// Assumes a Game.Core.BossEnemy on the same GameObject.
    /// </summary>
    public class BossPhaseController : MonoBehaviour
    {
        [SerializeField] private List<BossPhaseData> phases = new List<BossPhaseData>();

        private Game.Core.BossEnemy boss;
        private int currentPhaseIndex = -1;

        private void Awake()
        {
            phases.Sort((a, b) => a.healthThreshold.CompareTo(b.healthThreshold)); // ascending
            boss = GetComponent<Game.Core.BossEnemy>();
        }

        private void Update()
        {
            if (boss == null) return;

            float healthFrac = Mathf.Clamp01((float)boss.GetType()
                .GetField("currentHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(boss) /
                (float)boss.GetType()
                .GetField("maxHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(boss));

            int nextIndex = -1;
            for (int i = phases.Count - 1; i >= 0; i--)
            {
                if (healthFrac <= phases[i].healthThreshold)
                {
                    nextIndex = i;
                    break;
                }
            }

            if (nextIndex != -1 && nextIndex != currentPhaseIndex)
            {
                ApplyPhase(phases[nextIndex]);
                currentPhaseIndex = nextIndex;
            }
        }

        private void ApplyPhase(BossPhaseData p)
        {
            // Flip boss mode via serialized fields
            var modeField = typeof(Game.Core.BossEnemy).GetField("attackMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (modeField != null)
            {
                var enumType = modeField.FieldType;
                var rangedValue = System.Enum.Parse(enumType, p.rangedMode ? "Ranged" : "Melee");
                modeField.SetValue(boss, rangedValue);
            }

            // Adjust burst parameters and pacing
            SetPrivate("burstCount", p.burstCount);
            SetPrivate("burstInterval", p.burstInterval);
            SetPrivateInBase("moveSpeed", GetPrivateInBase<float>("moveSpeed") * p.moveSpeedMultiplier);
            SetPrivateInBase("attackCooldown", GetPrivateInBase<float>("attackCooldown") * p.attackCooldownMultiplier);
        }

        private void SetPrivate(string name, object val)
        {
            var f = typeof(Game.Core.BossEnemy).GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (f != null) f.SetValue(boss, val);
        }

        private void SetPrivateInBase(string name, object val)
        {
            var f = typeof(Game.Core.BaseEnemy).GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (f != null) f.SetValue(boss, val);
        }

        private T GetPrivateInBase<T>(string name)
        {
            var f = typeof(Game.Core.BaseEnemy).GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return f != null ? (T)f.GetValue(boss) : default;
        }
    }
}
