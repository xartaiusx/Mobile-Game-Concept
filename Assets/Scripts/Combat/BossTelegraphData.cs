using UnityEngine;

namespace Game.Combat
{
    public enum BossTelegraphAttackType
    {
        Area,
        ForwardCone,
        TargetedCircle
    }

    [CreateAssetMenu(menuName = "Game/Combat/BossTelegraphData")]
    public class BossTelegraphData : ScriptableObject
    {
        public string telegraphId = "boss_attack";
        public string displayName = "Boss Attack";
        [Min(1)] public int beatsBeforeImpact = 2;
        public int damage = 10;
        public float radius = 2.5f;
        public float range = 6f;
        public BossTelegraphAttackType attackType = BossTelegraphAttackType.Area;
        public GameObject warningVfxPrefab;
        public GameObject impactVfxPrefab;
        public AudioClip audioCue;
    }
}
