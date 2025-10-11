using UnityEngine;

namespace Game.Core
{
    [RequireComponent(typeof(Collider))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] float speed = 12f;
        [SerializeField] int damage = 5;
        [SerializeField] float lifetime = 5f;
        private Vector3 targetPoint;
        private bool initialized;

        private void OnEnable()
        {
            CancelInvoke();
            Invoke(nameof(Despawn), lifetime);
        }

        public void Initialize(Vector3 target, float customSpeed = -1f, int customDamage = -1)
        {
            targetPoint = target;
            if (customSpeed > 0f) speed = customSpeed;
            if (customDamage > 0) damage = customDamage;
            initialized = true;
        }

        private void Update()
        {
            if (!initialized) return;
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
            Destroy(gameObject);
        }
    }
}
