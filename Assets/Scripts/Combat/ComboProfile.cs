using UnityEngine;

namespace Game.Combat
{
    [CreateAssetMenu(menuName = "Game/Combat/ComboProfile")]
    public class ComboProfile : ScriptableObject
    {
        [System.Serializable]
        public struct Step
        {
            [Tooltip("Base damage for this step before rhythm multipliers.")]
            public int baseDamage;

            [Tooltip("Cooldown seconds applied after this step before rhythm refunds.")]
            public float cooldown;

            [Tooltip("Optional VFX or SFX key for this step.")]
            public string tag;
        }

        [Tooltip("Ordered combo steps. Index 0 is the opener.")]
        public Step[] steps;

        [Tooltip("Max time between steps to keep the combo alive.")]
        public float comboTimeout = 1.5f;
    }
}
