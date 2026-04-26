using Game.Core;
using Game.Rhythm;
using System;
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
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private ObjectPool projectilePool;
        [SerializeField] private GameObject projectileImpactPrefab;
        [SerializeField] private float mageProjectileSpeed = 9f;
        [SerializeField] private float archerProjectileSpeed = 16f;
        [SerializeField] private float projectileSpawnHeight = 1.1f;
        [SerializeField] private bool readKeyboardInput = true;
        [SerializeField] private KeyCode primaryAbilityKey = KeyCode.Q;

        private float[] cooldowns;
        private Collider[] hitCache;
        private BaseCharacter owner;
        private DodgeController dodgeController;
        private int pendingAbilityIndex = -1;

        public event Action<AbilityDefinition, RhythmGrade, float> AbilityResolved;
        public event Action<AbilityDefinition, RhythmGrade, int> AbilityEffectApplied;

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
            dodgeController = GetComponent<DodgeController>();
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

        public float GetCooldownRemaining(int index)
        {
            if (cooldowns == null || index < 0 || index >= cooldowns.Length)
                return 0f;
            return Mathf.Max(0f, cooldowns[index]);
        }

        public float GetCooldownDuration(int index)
        {
            if (abilities == null || index < 0 || index >= abilities.Length || abilities[index] == null)
                return 0f;
            return Mathf.Max(0f, abilities[index].cooldown);
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
            AbilityResolved?.Invoke(ability, grade, cooldowns[pendingAbilityIndex]);
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
            {
                owner.Heal(amount);
                if (grade == RhythmGrade.Perfect && ability.perfectProtectionSeconds > 0f)
                    dodgeController?.GrantInvulnerability(ability.perfectProtectionSeconds);
                AbilityEffectApplied?.Invoke(ability, grade, amount);
            }
        }

        private void ApplyDamageOrUtility(AbilityDefinition ability, RhythmGrade grade)
        {
            if (ability.targetMode == AbilityTargetMode.Self)
                return;

            int damage = ability.ScaledDamage(grade);
            if (damage <= 0) return;

            if (ability.executionStyle == AbilityExecutionStyle.ProjectileLike && TryLaunchProjectileAbility(ability, grade, damage))
            {
                AbilityEffectApplied?.Invoke(ability, grade, damage);
                return;
            }

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
                {
                    enemy.TakeDamage(new DamageContext(gameObject, damage, DamageType.Rhythm, grade, true));
                    if (grade == RhythmGrade.Perfect && ability.perfectStaggerSeconds > 0f)
                        enemy.Stagger(ability.perfectStaggerSeconds);
                    AbilityEffectApplied?.Invoke(ability, grade, damage);
                }
            }
        }

        private bool TryLaunchProjectileAbility(AbilityDefinition ability, RhythmGrade grade, int damage)
        {
            if (projectilePrefab == null && projectilePool == null)
                return false;

            Vector3 origin = aimOrigin.position + Vector3.up * projectileSpawnHeight;
            Vector3 forward = aimOrigin.forward.sqrMagnitude > 0.001f ? aimOrigin.forward : transform.forward;
            Vector3 target = ResolveProjectileTarget(origin, forward, ability.range);
            Quaternion rotation = Quaternion.LookRotation((target - origin).sqrMagnitude > 0.001f ? (target - origin).normalized : forward);

            GameObject projectile = projectilePool != null
                ? projectilePool.Spawn(origin, rotation)
                : Instantiate(projectilePrefab, origin, rotation);

            if (projectile == null) return false;

            var poolable = projectile.GetComponent<PoolableProjectile>();
            if (poolable == null) return false;

            bool isArcher = ability.abilityId != null && ability.abilityId.Contains("archer");
            bool perfect = grade == RhythmGrade.Perfect;
            float speed = isArcher ? archerProjectileSpeed : mageProjectileSpeed;
            float splash = isArcher ? 0f : (perfect ? Mathf.Max(ability.radius, 1.8f) : Mathf.Max(0f, ability.radius * 0.65f));
            bool pierce = isArcher && perfect;
            float stagger = perfect ? ability.perfectStaggerSeconds : 0f;

            poolable.InitializeAbilityProjectile(target, speed, damage, projectilePool, gameObject, grade, pierce, splash, stagger, projectileImpactPrefab);
            return true;
        }

        private Vector3 ResolveProjectileTarget(Vector3 origin, Vector3 forward, float range)
        {
            BaseEnemy nearest = FindNearestEnemyInArc(origin, forward, range);
            if (nearest != null)
                return nearest.transform.position + Vector3.up * 0.9f;

            return origin + forward.normalized * Mathf.Max(1f, range);
        }

        private BaseEnemy FindNearestEnemyInArc(Vector3 origin, Vector3 forward, float range)
        {
            BaseEnemy[] enemies = FindObjectsByType<BaseEnemy>(FindObjectsInactive.Exclude);
            float bestDistance = float.MaxValue;
            BaseEnemy best = null;
            float coneDot = Mathf.Cos(forwardConeAngle * 0.5f * Mathf.Deg2Rad);
            for (int i = 0; i < enemies.Length; i++)
            {
                BaseEnemy enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive) continue;

                Vector3 toEnemy = enemy.transform.position - origin;
                float distance = toEnemy.magnitude;
                if (distance > range || distance < 0.01f) continue;
                if (Vector3.Dot(forward.normalized, toEnemy.normalized) < coneDot) continue;

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = enemy;
                }
            }

            return best;
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
