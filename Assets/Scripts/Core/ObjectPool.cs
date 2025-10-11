using UnityEngine;
using System.Collections.Generic;

namespace Game.Core
{
    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }

    public class ObjectPool : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int preloadCount = 8;
        [SerializeField] private bool expandable = true;

        private readonly Queue<GameObject> pool = new Queue<GameObject>();

        private void Awake()
        {
            if (prefab == null) return;
            for (int i = 0; i < preloadCount; i++)
            {
                var go = Instantiate(prefab, transform);
                go.SetActive(false);
                pool.Enqueue(go);
            }
        }

        public GameObject Spawn(Vector3 position, Quaternion rotation)
        {
            GameObject go = null;
            while (pool.Count > 0 && go == null)
            {
                go = pool.Dequeue();
            }

            if (go == null)
            {
                if (!expandable || prefab == null) return null;
                go = Instantiate(prefab, transform);
            }

            go.transform.position = position;
            go.transform.rotation = rotation;
            go.SetActive(true);

            var p = go.GetComponent<IPoolable>();
            p?.OnSpawned();

            return go;
        }

        public void Despawn(GameObject go)
        {
            if (go == null) return;
            var p = go.GetComponent<IPoolable>();
            p?.OnDespawned();
            go.SetActive(false);
            pool.Enqueue(go);
        }
    }
}
