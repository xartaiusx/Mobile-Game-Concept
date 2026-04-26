using System;
using Game.Rhythm;
using UnityEngine;

namespace Game.Combat
{
    public enum AbilityType
    {
        Damage,
        Heal,
        Buff,
        Mobility,
        Utility
    }

    public enum AbilityTargetMode
    {
        Self,
        ForwardCone,
        AreaAroundSelf,
        TargetPoint,
        Ally
    }

    [Serializable]
    public struct AbilityRhythmScaling
    {
        public float perfectMultiplier;
        public float goodMultiplier;
        public float missMultiplier;

        public static AbilityRhythmScaling Default => new AbilityRhythmScaling
        {
            perfectMultiplier = 1.5f,
            goodMultiplier = 1.15f,
            missMultiplier = 0.75f
        };

        public float MultiplierFor(RhythmGrade grade)
        {
            switch (grade)
            {
                case RhythmGrade.Perfect: return perfectMultiplier;
                case RhythmGrade.Good: return goodMultiplier;
                default: return missMultiplier;
            }
        }
    }

    [CreateAssetMenu(menuName = "Game/Combat/AbilityDefinition")]
    public class AbilityDefinition : ScriptableObject
    {
        public string abilityId = "ability";
        public string displayName = "Ability";
        [TextArea] public string description;
        public int baseDamage = 10;
        public int healAmount;
        public float cooldown = 1f;
        public float range = 5f;
        public float radius = 1.5f;
        public int resourceCost;
        public AbilityType abilityType = AbilityType.Damage;
        public AbilityTargetMode targetMode = AbilityTargetMode.ForwardCone;
        public AbilityRhythmScaling rhythmScaling = AbilityRhythmScaling.Default;

        public int ScaledDamage(RhythmGrade grade)
        {
            return Mathf.Max(0, Mathf.RoundToInt(baseDamage * rhythmScaling.MultiplierFor(grade)));
        }

        public int ScaledHealing(RhythmGrade grade)
        {
            return Mathf.Max(0, Mathf.RoundToInt(healAmount * rhythmScaling.MultiplierFor(grade)));
        }

        public float EffectiveCooldown(RhythmGrade grade)
        {
            float refund = grade == RhythmGrade.Perfect ? 0.2f : grade == RhythmGrade.Good ? 0.1f : 0f;
            return Mathf.Max(0f, cooldown * (1f - refund));
        }
    }
}
