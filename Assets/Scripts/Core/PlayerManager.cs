using UnityEngine;

namespace Game.Core
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance { get; private set; }
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

        public void RegisterPlayer(Transform t)
        {
            Player = t;
        }

        public Transform GetPlayerTransform() => Player;
    }
}
