using Game.Core;
using Game.Rhythm;
using UnityEngine;

namespace Game.Combat
{
    public class ParryController : MonoBehaviour, IDamageResponder
    {
        [SerializeField] private InputBuffer parryBuffer;
        [SerializeField] private float activeWindow = 0.28f;
        [SerializeField] private float cooldown = 0.9f;
        [SerializeField, Range(0f, 1f)] private float goodDamageReduction = 0.5f;
        [SerializeField] private float perfectStaggerDuration = 0.5f;
        [SerializeField] private bool readKeyboardInput = true;
        [SerializeField] private KeyCode parryKey = KeyCode.E;

        private float activeUntil;
        private float cooldownRemaining;
        private RhythmGrade currentGrade = RhythmGrade.Miss;

        public bool IsParrying => Time.time < activeUntil;
        public RhythmGrade CurrentGrade => currentGrade;
        public float CooldownRemaining => cooldownRemaining;

        private void Awake()
        {
            parryBuffer = parryBuffer != null ? parryBuffer : GetComponent<InputBuffer>();
        }

        private void OnEnable()
        {
            if (parryBuffer != null)
                parryBuffer.OnResolved += ResolveParry;
        }

        private void OnDisable()
        {
            if (parryBuffer != null)
                parryBuffer.OnResolved -= ResolveParry;
        }

        private void Update()
        {
            if (cooldownRemaining > 0f)
                cooldownRemaining -= Time.deltaTime;

            if (readKeyboardInput && Input.GetKeyDown(parryKey))
                RequestParry();
        }

        public bool RequestParry()
        {
            if (cooldownRemaining > 0f) return false;

            if (parryBuffer != null)
                parryBuffer.RegisterPress();
            else
                ResolveParry(RhythmGrade.Miss);

            return true;
        }

        public void ResolveParryForTests(RhythmGrade grade)
        {
            ResolveParry(grade);
        }

        public bool TryModifyDamage(ref DamageContext context)
        {
            if (!IsParrying || !context.canBeParried) return false;

            if (currentGrade == RhythmGrade.Perfect)
            {
                context.amount = 0;
                var enemy = context.source != null ? context.source.GetComponent<BaseEnemy>() : null;
                if (enemy != null)
                    enemy.Stagger(perfectStaggerDuration);
                return true;
            }

            if (currentGrade == RhythmGrade.Good)
            {
                context.amount = Mathf.Max(0, Mathf.RoundToInt(context.amount * (1f - goodDamageReduction)));
                return context.amount <= 0;
            }

            return false;
        }

        private void ResolveParry(RhythmGrade grade)
        {
            if (cooldownRemaining > 0f) return;

            currentGrade = grade;
            float gradeWindow = grade == RhythmGrade.Perfect ? activeWindow * 1.25f : grade == RhythmGrade.Good ? activeWindow : activeWindow * 0.5f;
            activeUntil = Time.time + Mathf.Max(0.01f, gradeWindow);
            cooldownRemaining = grade == RhythmGrade.Perfect ? cooldown * 0.75f : cooldown;
        }
    }
}
