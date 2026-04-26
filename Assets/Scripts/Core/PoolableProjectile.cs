using UnityEngine;
using Game.Rhythm;

namespace Game.Core
{
    [RequireComponent(typeof(Collider))]
    public class PoolableProjectile : MonoBehaviour, IPoolable
    {
        [SerializeField] float speed = 12f;
        [SerializeField] int damage = 5;
        [SerializeField] float lifetime = 5f;
        [SerializeField] DamageType damageType = DamageType.Projectile;
        [SerializeField] bool damageEnemies;
        [SerializeField] int pierceCount;
        [SerializeField] float splashRadius;
        [SerializeField] float perfectStaggerSeconds;
        [SerializeField] GameObject impactVfxPrefab;

        private Vector3 targetPoint;
        private Vector3 direction;
        private bool initialized;
        private float t;
        private ObjectPool owningPool;
        private GameObject source;
        private RhythmGrade rhythmGrade = RhythmGrade.Miss;
        private int remainingPierces;
        private readonly Collider[] splashHits = new Collider[12];

        public void Initialize(Vector3 target, float customSpeed = -1f, int customDamage = -1, ObjectPool pool = null)
        {
            Initialize(target, customSpeed, customDamage, pool, null);
        }

        public void Initialize(Vector3 target, float customSpeed, int customDamage, ObjectPool pool, GameObject damageSource)
        {
            targetPoint = target;
            direction = (targetPoint - transform.position).normalized;
            if (customSpeed > 0f) speed = customSpeed;
            if (customDamage > 0) damage = customDamage;
            initialized = true;
            owningPool = pool;
            source = damageSource;
            damageEnemies = false;
            splashRadius = 0f;
            perfectStaggerSeconds = 0f;
            impactVfxPrefab = null;
            rhythmGrade = RhythmGrade.Miss;
            remainingPierces = 0;
        }

        public void InitializeAbilityProjectile(Vector3 target, float customSpeed, int customDamage, ObjectPool pool, GameObject damageSource, RhythmGrade grade, bool canPierce, float splash, float perfectStagger, GameObject impactPrefab)
        {
            targetPoint = target;
            direction = (targetPoint - transform.position).normalized;
            if (direction.sqrMagnitude <= 0.001f)
                direction = transform.forward;
            if (customSpeed > 0f) speed = customSpeed;
            damage = Mathf.Max(0, customDamage);
            initialized = true;
            owningPool = pool;
            source = damageSource;
            damageEnemies = true;
            remainingPierces = canPierce ? 1 : 0;
            splashRadius = Mathf.Max(0f, splash);
            perfectStaggerSeconds = Mathf.Max(0f, perfectStagger);
            impactVfxPrefab = impactPrefab;
            rhythmGrade = grade;
        }

        private void Update()
        {
            if (!initialized) return;
            t += Time.deltaTime;
            if (t >= lifetime) { Despawn(); return; }

            Vector3 dir = targetPoint - transform.position;
            float step = speed * Time.deltaTime;
            if (dir.sqrMagnitude <= step * step)
            {
                transform.position = targetPoint;
                SpawnImpact();
                Despawn();
                return;
            }
            transform.position += direction.sqrMagnitude > 0.001f ? direction * step : dir.normalized * step;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (damageEnemies)
            {
                var enemy = other.GetComponent<BaseEnemy>();
                if (enemy == null) return;

                ApplyEnemyDamage(enemy);
                if (splashRadius > 0.01f)
                    ApplySplash(other.transform.position, enemy);
                SpawnImpact();

                if (remainingPierces > 0)
                {
                    remainingPierces--;
                    return;
                }

                Despawn();
                return;
            }

            var character = other.GetComponent<BaseCharacter>();
            if (character != null)
            {
                character.TakeDamage(new DamageContext(source != null ? source : gameObject, damage, damageType, RhythmGrade.Miss, true));
                SpawnImpact();
                Despawn();
            }
        }

        private void ApplyEnemyDamage(BaseEnemy enemy)
        {
            if (enemy == null || damage <= 0) return;
            enemy.TakeDamage(new DamageContext(source != null ? source : gameObject, damage, DamageType.Rhythm, rhythmGrade, true));
            if (rhythmGrade == RhythmGrade.Perfect && perfectStaggerSeconds > 0f)
                enemy.Stagger(perfectStaggerSeconds);
        }

        private void ApplySplash(Vector3 center, BaseEnemy directHit)
        {
            int count = Physics.OverlapSphereNonAlloc(center, splashRadius, splashHits, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var enemy = splashHits[i] != null ? splashHits[i].GetComponent<BaseEnemy>() : null;
                if (enemy == null || enemy == directHit) continue;
                int splashDamage = Mathf.Max(1, Mathf.RoundToInt(damage * 0.55f));
                enemy.TakeDamage(new DamageContext(source != null ? source : gameObject, splashDamage, DamageType.Rhythm, rhythmGrade, true));
            }
        }

        private void SpawnImpact()
        {
            if (impactVfxPrefab == null) return;
            Instantiate(impactVfxPrefab, transform.position, Quaternion.identity);
        }

        private void Despawn()
        {
            initialized = false;
            t = 0f;
            if (owningPool != null) owningPool.Despawn(gameObject);
            else Destroy(gameObject);
        }

        public void OnSpawned() { t = 0f; initialized = false; source = null; }
        public void OnDespawned() { source = null; impactVfxPrefab = null; }
    }
}
