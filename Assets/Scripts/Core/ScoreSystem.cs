using System;
using Game.Rhythm;
using Game.Systems;
using UnityEngine;

namespace Game.Core
{
    public class ScoreSystem : MonoBehaviour
    {
        [SerializeField] private RhythmConfig rhythmConfig;
        [SerializeField] private int victoryBonus = 250;
        [SerializeField] private int waveClearBaseBonus = 75;
        [SerializeField] private int waveClearPerWaveBonus = 15;
        [SerializeField] private int bossClearBaseBonus = 350;
        [SerializeField] private string highScorePlayerPrefsKey = "WarriorEndlessHighScore";
        [SerializeField] private bool scoreOnlyPlayerDamage = true;

        public static ScoreSystem Instance { get; private set; }

        public event Action<int, int, RhythmGrade> ScoreChanged;

        public int Score { get; private set; }
        public int HighScore { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            HighScore = PlayerPrefs.GetInt(highScorePlayerPrefsKey, 0);
        }

        private void OnEnable()
        {
            BaseEnemy.EnemyDamagedGlobal += HandleEnemyDamaged;
        }

        private void OnDisable()
        {
            BaseEnemy.EnemyDamagedGlobal -= HandleEnemyDamaged;
            if (Instance == this)
                Instance = null;
        }

        public void Configure(RhythmConfig config)
        {
            rhythmConfig = config;
        }

        public void AddVictoryBonus()
        {
            AddBossClearBonus(1);
        }

        public void AddWaveClearBonus(int wave)
        {
            int safeWave = Mathf.Max(1, wave);
            AddScore(waveClearBaseBonus + safeWave * waveClearPerWaveBonus, RhythmGrade.Good);
        }

        public void AddBossClearBonus(int wave)
        {
            int safeWave = Mathf.Max(1, wave);
            AddScore(victoryBonus + bossClearBaseBonus + safeWave * waveClearPerWaveBonus, RhythmGrade.Perfect);
        }

        public int ScoreForGrade(RhythmGrade grade)
        {
            if (rhythmConfig == null)
            {
                switch (grade)
                {
                    case RhythmGrade.Perfect: return 100;
                    case RhythmGrade.Good: return 50;
                    default: return 10;
                }
            }

            switch (grade)
            {
                case RhythmGrade.Perfect: return rhythmConfig.perfectScore;
                case RhythmGrade.Good: return rhythmConfig.goodScore;
                default: return rhythmConfig.missScore;
            }
        }

        public int CalculateHitScore(RhythmGrade grade, int damage, int comboCount = 0)
        {
            if (damage <= 0) return 0;
            int baseScore = ScoreForGrade(grade);
            int comboBonus = rhythmConfig != null ? rhythmConfig.comboScoreBonus : 5;
            return Mathf.Max(0, baseScore + Mathf.Max(0, comboCount - 1) * comboBonus);
        }

        public void AddHitScore(RhythmGrade grade, int damage, int comboCount = 0)
        {
            AddScore(CalculateHitScore(grade, damage, comboCount), grade);
        }

        public void AddScore(int amount, RhythmGrade grade = RhythmGrade.Miss)
        {
            if (amount <= 0) return;
            Score += amount;
            if (Score > HighScore)
            {
                HighScore = Score;
                PlayerPrefs.SetInt(highScorePlayerPrefsKey, HighScore);
            }
            TelemetryManager.ReportScore(Score);
            ScoreChanged?.Invoke(Score, amount, grade);
        }

        public void ResetScore()
        {
            Score = 0;
            HighScore = PlayerPrefs.GetInt(highScorePlayerPrefsKey, HighScore);
            ScoreChanged?.Invoke(Score, 0, RhythmGrade.Miss);
        }

        private void HandleEnemyDamaged(BaseEnemy enemy, DamageContext context)
        {
            if (scoreOnlyPlayerDamage && !IsPlayerSource(context.source))
                return;

            if (context.damageType != DamageType.Rhythm)
                return;

            int comboCount = 0;
            if (context.source != null)
            {
                var combo = context.source.GetComponent<Game.Combat.ComboSystem>();
                if (combo != null)
                    comboCount = combo.CurrentComboCount;
            }

            AddHitScore(context.rhythmGrade, context.amount, comboCount);
        }

        private static bool IsPlayerSource(GameObject source)
        {
            if (source == null) return false;
            if (source.CompareTag("Player")) return true;
            return source.GetComponentInParent<BaseCharacter>() != null && source.GetComponentInParent<PlayerController>() != null;
        }
    }
}
