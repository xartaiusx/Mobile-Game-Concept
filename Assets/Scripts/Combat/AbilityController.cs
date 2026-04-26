using Game.Core;
using Game.Rhythm;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Resolves player abilities through a dedicated rhythm input buffer.
    /// </summary>
    public class AbilityController : MonoBehaviour
    {
        [SerializeField] private AbilityDefinition[] abilities;
        [SerializeField] private InputBuffer abilityBuffer;
        [SerializeField] private Transform aimOrigin;
        [SerializeField] private LayerMask enemyMask = ~0;
        [SerializeField] private int maxTargets = 12;
        [SerializeField, Range(1f, 180f)] private float forwardConeAngle = 70f;
        [SerializeField] private bool readKeyboardInput = true;
        [SerializeField] private KeyCode primaryAbilityKey = KeyCode.Q;

        private float[] cooldowns;
        private Collider[] hitCache;
        private BaseCharacter owner;
        private int pendingAbilityIndex = -1;

        public AbilityDefinition[] Abilities => abilities;
        public int PendingAbilityIndex => pendingAbilityIndex;

        public void ConfigureAbilities(AbilityDefinition[] definitions)
        {
            abilities = definitions;
            cooldowns = new float[Mathf.Max(1, abilities != null ? abilities.Length : 1)];
        }

        private void Awake()
        {
            owner = GetComponent<BaseCharacter>();
            abilityBuffer = abilityBuffer != null ? abilityBuffer : GetComponent<InputBuffer>();
            aimOrigin = aimOrigin != null ? aimOrigin : transform;
            hitCache = new Collider[Mathf.Max(1, maxTargets)];
            cooldowns = new float[Mathf.Max(1, abilities != null ? abilities.Length : 1)];
        }

        private void OnEnable()
        {
            if (abilityBuffer != null)
                abilityBuffer.OnResolved += ResolvePendingAbility;
        }

        private void OnDisable()
        {
            if (abilityBuffer != null)
                abilityBuffer.OnResolved -= ResolvePendingAbility;
        }

        private void Update()
        {
            TickCooldowns();

            if (readKeyboardInput && Input.GetKeyDown(primaryAbilityKey))
                RequestAbility(0);
        }

        public bool RequestAbility(int index)
        {
            if (!CanRequest(index)) return false;

            pendingAbilityIndex = index;
            if (abilityBuffer != null)
                abilityBuffer.RegisterPress();
            else
                ResolvePendingAbility(RhythmGrade.Miss);

            return true;
        }

        public bool CanRequest(int index)
        {
            return abilities != null
                && index >= 0
                && index < abilities.Length
                && abilities[index] != null
                && cooldowns != null
                && index < cooldowns.Length
                && cooldowns[index] <= 0f;
        }

        public void ResolveAbilityForTests(int index, RhythmGrade grade)
        {
            if (!CanRequest(index)) return;
            pendingAbilityIndex = index;
            ResolvePendingAbility(grade);
        }

        private void ResolvePendingAbility(RhythmGrade grade)
        {
            if (pendingAbilityIndex < 0 || !CanRequest(pendingAbilityIndex))
            {
                pendingAbilityIndex = -1;
                return;
            }

            AbilityDefinition ability = abilities[pendingAbilityIndex];
            ApplyAbility(ability, grade);
            cooldowns[pendingAbilityIndex] = ability.EffectiveCooldown(grade);
            pendingAbilityIndex = -1;
        }

        private void ApplyAbility(AbilityDefinition ability, RhythmGrade grade)
        {
            switch (ability.abilityType)
            {
                case AbilityType.Heal:
                    ApplyHeal(ability, grade);
                    break;
                case AbilityType.Damage:
                case AbilityType.Utility:
                case AbilityType.Buff:
                case AbilityType.Mobility:
                    ApplyDamageOrUtility(ability, grade);
                    break;
            }
        }

        private void ApplyHeal(AbilityDefinition ability, RhythmGrade grade)
        {
            int amount = ability.ScaledHealing(grade);
            if (owner != null)
                owner.Heal(amount);
        }

        private void ApplyDamageOrUtility(AbilityDefinition ability, RhythmGrade grade)
        {
            if (ability.targetMode == AbilityTargetMode.Self)
                return;

            int damage = ability.ScaledDamage(grade);
            if (damage <= 0) return;

            Vector3 origin = aimOrigin.position;
            Vector3 forward = aimOrigin.forward;
            float radius = Mathf.Max(0.1f, ability.radius);
            int count = 0;

            switch (ability.targetMode)
            {
                case AbilityTargetMode.AreaAroundSelf:
                    count = Physics.OverlapSphereNonAlloc(transform.position, radius, hitCache, enemyMask, QueryTriggerInteraction.Ignore);
                    break;
                case AbilityTargetMode.TargetPoint:
                    Vector3 point = origin + forward * Mathf.Max(0f, ability.range);
                    count = Physics.OverlapSphereNonAlloc(point, radius, hitCache, enemyMask, QueryTriggerInteraction.Ignore);
                    break;
                default:
                    count = Physics.OverlapSphereNonAlloc(origin + forward * (ability.range * 0.5f), Mathf.Max(radius, ability.range * 0.5f), hitCache, enemyMask, QueryTriggerInteraction.Ignore);
                    break;
            }

            float coneDot = Mathf.Cos(forwardConeAngle * 0.5f * Mathf.Deg2Rad);
            for (int i = 0; i < count; i++)
            {
                Collider hit = hitCache[i];
                if (hit == null) continue;

                if (ability.targetMode == AbilityTargetMode.ForwardCone)
                {
                    Vector3 toHit = (hit.transform.position - origin).normalized;
                    if (Vector3.Dot(forward, toHit) < coneDot)
                        continue;
                }

                var enemy = hit.GetComponent<BaseEnemy>();
                if (enemy != null)
                    enemy.TakeDamage(new DamageContext(gameObject, damage, DamageType.Rhythm, grade, true));
            }
        }

        private void TickCooldowns()
        {
            int count = Mathf.Min(cooldowns.Length, abilities != null ? abilities.Length : 0);
            for (int i = 0; i < count; i++)
            {
                if (cooldowns[i] > 0f)
                    cooldowns[i] -= Time.deltaTime;
            }
        }
    }
}
