using UnityEngine;

namespace Game.Core
{
    [RequireComponent(typeof(Collider))]
    public class PoolableProjectile : MonoBehaviour, IPoolable
    {
        [SerializeField] float speed = 12f;
        [SerializeField] int damage = 5;
        [SerializeField] float lifetime = 5f;

        private Vector3 targetPoint;
        private bool initialized;
        private float t;
        private ObjectPool owningPool;

        public void Initialize(Vector3 target, float customSpeed = -1f, int customDamage = -1, ObjectPool pool = null)
        {
            targetPoint = target;
            if (customSpeed > 0f) speed = customSpeed;
            if (customDamage > 0) damage = customDamage;
            initialized = true;
            owningPool = pool;
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
                character.TakeDamage(damage);
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

        public void OnSpawned() { t = 0f; initialized = false; }
        public void OnDespawned() { }
    }
}
