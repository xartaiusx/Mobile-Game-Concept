using UnityEngine;

namespace Game.Core
{
    public class PlayerManager : MonoBehaviour
    {
        private static PlayerManager instance;

        public static PlayerManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindAnyObjectByType<PlayerManager>();
                return instance;
            }
            private set => instance = value;
        }
        public Transform Player { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void RegisterPlayer(Transform t)
        {
            Player = t;
        }

        public void UnregisterPlayer(Transform t)
        {
            if (Player == t)
                Player = null;
        }

        public Transform GetPlayerTransform() => Player;
    }
}
