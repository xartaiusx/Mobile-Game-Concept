using System;
using Game.Rhythm;
using UnityEngine;

namespace Game.Core
{
    public enum DamageType
    {
        Physical,
        Magical,
        Projectile,
        Rhythm,
        Environmental
    }

    [Serializable]
    public struct DamageContext
    {
        public GameObject source;
        public int amount;
        public DamageType damageType;
        public RhythmGrade rhythmGrade;
        public bool canBeParried;

        public DamageContext(GameObject source, int amount, DamageType damageType = DamageType.Physical, RhythmGrade rhythmGrade = RhythmGrade.Miss, bool canBeParried = true)
        {
            this.source = source;
            this.amount = amount;
            this.damageType = damageType;
            this.rhythmGrade = rhythmGrade;
            this.canBeParried = canBeParried;
        }
    }

    public interface IDamageResponder
    {
        bool TryModifyDamage(ref DamageContext context);
    }
}
