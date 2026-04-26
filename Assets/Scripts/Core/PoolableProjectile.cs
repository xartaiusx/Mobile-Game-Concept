using UnityEngine;

namespace Game.Core
{
    [RequireComponent(typeof(Collider))]
    public class PoolableProjectile : MonoBehaviour, IPoolable
    {
        [SerializeField] float speed = 12f;
        [SerializeField] int damage = 5;
        [SerializeField] float lifetime = 5f;
        [SerializeField] DamageType damageType = DamageType.Projectile;

        private Vector3 targetPoint;
        private bool initialized;
        private float t;
        private ObjectPool owningPool;
        private GameObject source;

        public void Initialize(Vector3 target, float customSpeed = -1f, int customDamage = -1, ObjectPool pool = null)
        {
            Initialize(target, customSpeed, customDamage, pool, null);
        }

        public void Initialize(Vector3 target, float customSpeed, int customDamage, ObjectPool pool, GameObject damageSource)
        {
            targetPoint = target;
            if (customSpeed > 0f) speed = customSpeed;
            if (customDamage > 0) damage = customDamage;
            initialized = true;
            owningPool = pool;
            source = damageSource;
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
                Despawn();
                return;
            }
            transform.position += dir.normalized * step;
        }

        private void OnTriggerEnter(Collider other)
        {
            var character = other.GetComponent<BaseCharacter>();
            if (character != null)
            {
                character.TakeDamage(new DamageContext(source != null ? source : gameObject, damage, damageType, Game.Rhythm.RhythmGrade.Miss, true));
                Despawn();
            }
        }

        private void Despawn()
        {
            initialized = false;
            t = 0f;
            if (owningPool != null) owningPool.Despawn(gameObject);
            else Destroy(gameObject);
        }

        public void OnSpawned() { t = 0f; initialized = false; source = null; }
        public void OnDespawned() { source = null; }
    }
}
