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
            phases.RemoveAll(p => p == null);
            phases.Sort((a, b) => a.healthThreshold.CompareTo(b.healthThreshold)); // ascending
            boss = GetComponent<Game.Core.BossEnemy>();
        }

        private void Update()
        {
            if (boss == null) return;

            float healthFrac = boss.HealthFraction;

            int nextIndex = -1;
            for (int i = 0; i < phases.Count; i++)
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
            boss.ApplyPhase(p);
        }
    }
}
