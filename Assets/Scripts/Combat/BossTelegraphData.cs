using UnityEngine;

namespace Game.Combat
{
    public enum BossAttackType
    {
        Slam,
        Line,
        RadialPulse
    }

    public enum TelegraphShape
    {
        Circle,
        Line,
        Cone
    }

    [CreateAssetMenu(menuName = "Game/Combat/BossTelegraphData")]
    public class BossTelegraphData : ScriptableObject
    {
        public string telegraphId = "boss_attack";
        public string displayName = "Boss Attack";
        [Min(1)] public int beatsBeforeImpact = 2;
        [Min(1)] public int repeatCount = 1;
        [Min(1)] public int beatsBetweenRepeats = 1;
        public int damage = 10;
        public float radius = 2.5f;
        public float range = 6f;
        public float width = 1.5f;
        public float length = 6f;
        public BossAttackType attackType = BossAttackType.Slam;
        public TelegraphShape shape = TelegraphShape.Circle;
        public GameObject warningVfxPrefab;
        public GameObject impactVfxPrefab;
        public AudioClip audioCue;
    }
}
