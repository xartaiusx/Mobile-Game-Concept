using UnityEngine;

namespace Game.Core
{
    public class DifficultyScaler : MonoBehaviour
    {
        [Header("Wave Count")]
        [SerializeField] private int baseEnemyCount = 2;
        [SerializeField] private int enemyCountIncreaseEveryWaves = 3;
        [SerializeField] private int maxEnemyCount = 8;
        [SerializeField] private int bossEveryWaves = 5;

        [Header("Enemy Scaling")]
        [SerializeField] private float healthPerWave = 0.08f;
        [SerializeField] private float maxHealthMultiplier = 2.75f;
        [SerializeField] private float damagePerWave = 0.045f;
        [SerializeField] private float maxDamageMultiplier = 2.1f;
        [SerializeField] private float moveSpeedPerWave = 0.015f;
        [SerializeField] private float maxMoveSpeedMultiplier = 1.35f;
        [SerializeField] private float attackCooldownReductionPerWave = 0.012f;
        [SerializeField] private float minAttackCooldownMultiplier = 0.72f;

        [Header("Boss Scaling")]
        [SerializeField] private float bossHealthBonus = 1.85f;
        [SerializeField] private float bossDamageBonus = 1.3f;
        [SerializeField] private float bossCooldownBonus = 0.88f;

        [Header("Rhythm And Pickups")]
        [SerializeField] private float beatSpeedPerWave = 0.015f;
        [SerializeField] private float maxBeatSpeedMultiplier = 1.35f;
        [SerializeField] private float pickupGenerosityLossPerWave = 0.015f;
        [SerializeField] private float minPickupGenerosityMultiplier = 0.55f;

        public int BossEveryWaves => Mathf.Max(1, bossEveryWaves);

        public bool IsBossWave(int wave)
        {
            return Mathf.Max(1, wave) % BossEveryWaves == 0;
        }

        public int GetEnemyCount(int wave)
        {
            int safeWave = Mathf.Max(1, wave);
            int added = (safeWave - 1) / Mathf.Max(1, enemyCountIncreaseEveryWaves);
            return Mathf.Clamp(baseEnemyCount + added, 1, Mathf.Max(1, maxEnemyCount));
        }

        public DifficultySnapshot Evaluate(int wave, bool bossOrElite)
        {
            int safeWave = Mathf.Max(1, wave);
            float steps = safeWave - 1;
            var snapshot = new DifficultySnapshot
            {
                enemyCount = GetEnemyCount(safeWave),
                healthMultiplier = Mathf.Min(maxHealthMultiplier, 1f + steps * Mathf.Max(0f, healthPerWave)),
                damageMultiplier = Mathf.Min(maxDamageMultiplier, 1f + steps * Mathf.Max(0f, damagePerWave)),
                moveSpeedMultiplier = Mathf.Min(maxMoveSpeedMultiplier, 1f + steps * Mathf.Max(0f, moveSpeedPerWave)),
                attackCooldownMultiplier = Mathf.Max(minAttackCooldownMultiplier, 1f - steps * Mathf.Max(0f, attackCooldownReductionPerWave)),
                beatSpeedMultiplier = Mathf.Min(maxBeatSpeedMultiplier, 1f + steps * Mathf.Max(0f, beatSpeedPerWave)),
                pickupGenerosityMultiplier = Mathf.Max(minPickupGenerosityMultiplier, 1f - steps * Mathf.Max(0f, pickupGenerosityLossPerWave))
            };

            if (bossOrElite)
            {
                snapshot.healthMultiplier = Mathf.Min(maxHealthMultiplier * bossHealthBonus, snapshot.healthMultiplier * bossHealthBonus);
                snapshot.damageMultiplier = Mathf.Min(maxDamageMultiplier * bossDamageBonus, snapshot.damageMultiplier * bossDamageBonus);
                snapshot.attackCooldownMultiplier = Mathf.Max(minAttackCooldownMultiplier * bossCooldownBonus, snapshot.attackCooldownMultiplier * bossCooldownBonus);
            }

            return snapshot;
        }
    }

    public struct DifficultySnapshot
    {
        public int enemyCount;
        public float healthMultiplier;
        public float damageMultiplier;
        public float moveSpeedMultiplier;
        public float attackCooldownMultiplier;
        public float beatSpeedMultiplier;
        public float pickupGenerosityMultiplier;
    }
}
