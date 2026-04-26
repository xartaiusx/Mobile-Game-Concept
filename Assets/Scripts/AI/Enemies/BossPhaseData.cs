using UnityEngine;
using Game.Combat;

namespace Game.AI.Enemies
{
    [CreateAssetMenu(menuName = "Game/AI/BossPhase")]
    public class BossPhaseData : ScriptableObject
    {
        [Range(0f, 1f)]
        [Tooltip("Enter this phase when boss health is at or below this fraction.")]
        public float healthThreshold = 0.7f;

        [Header("Phase Parameters")]
        public string displayName = "Boss Phase";
        public bool rangedMode;
        public int burstCount = 3;
        public float burstInterval = 0.2f;
        public float moveSpeedMultiplier = 1.0f;
        public float attackCooldownMultiplier = 1.0f;
        public float telegraphCooldownMultiplier = 1f;
        public float telegraphWarningScale = 1.15f;
        public BossTelegraphData[] telegraphs;
    }
}
