using UnityEngine;

namespace Game.Core
{
    public class SimpleFollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 8f, -8f);
        [SerializeField] private float followDamping = 8f;
        [SerializeField] private float lookHeight = 1.2f;

        private void LateUpdate()
        {
            if (target == null)
                ResolveTarget();
            if (target == null) return;

            Vector3 desired = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-followDamping * Time.deltaTime));
            transform.LookAt(target.position + Vector3.up * lookHeight);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void ResolveTarget()
        {
            if (PlayerManager.Instance != null)
                target = PlayerManager.Instance.GetPlayerTransform();

            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    target = player.transform;
            }
        }
    }
}
