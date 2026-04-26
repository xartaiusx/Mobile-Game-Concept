using UnityEngine;

namespace Game.Combat
{
    [CreateAssetMenu(menuName = "Game/Combat/AttackTimingData")]
    public class AttackTimingData : ScriptableObject
    {
        [Min(0f)] public float windupSeconds = 0.08f;
        [Min(0.01f)] public float activeSeconds = 0.08f;
        [Min(0f)] public float recoverySeconds = 0.18f;
        public bool canCancelOnPerfect = true;
        public bool beatAlignedImpact = true;
        public float hitFrameBeatOffset;

        public float TotalDuration => windupSeconds + activeSeconds + recoverySeconds;
    }
}
