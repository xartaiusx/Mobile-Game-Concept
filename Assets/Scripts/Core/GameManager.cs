using UnityEngine;
using System.Collections.Generic;
using Game.Classes;

namespace Game.Core
{
    /// <summary>
    /// Game Manager handles character spawning and core game logic.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public Dictionary<string, GameObject> characterPrefabs = new Dictionary<string, GameObject>();

        public GameObject warriorPrefab;
        public GameObject fighterPrefab;
        public GameObject magePrefab;
        public GameObject archerPrefab;
        public GameObject healerPrefab;
        [SerializeField] private Transform playerSpawnPoint;

        private void Awake()
        {
            GameObject activeWarrior = warriorPrefab != null ? warriorPrefab : fighterPrefab;
            characterPrefabs["Warrior"] = activeWarrior;
            characterPrefabs["Fighter"] = activeWarrior;
            characterPrefabs["Mage"] = null;
            characterPrefabs["Archer"] = null;
            characterPrefabs["Healer"] = null;
        }

        private void Start()
        {
            if (PlayerManager.Instance != null && PlayerManager.Instance.GetPlayerTransform() != null)
                return;

            GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
            if (existingPlayer != null)
            {
                PlayerManager.Instance?.RegisterPlayer(existingPlayer.transform);
                return;
            }

            string selectedClass = "Warrior";

            if (string.IsNullOrEmpty(selectedClass) || !characterPrefabs.ContainsKey(selectedClass) || characterPrefabs[selectedClass] == null)
            {
                Debug.LogWarning("Invalid class selection, defaulting to Warrior.");
                selectedClass = "Warrior";
            }

            if (characterPrefabs[selectedClass] == null)
            {
                Debug.LogError("No player prefab is assigned for " + selectedClass + ".");
                return;
            }

            Vector3 spawnPosition = playerSpawnPoint != null ? playerSpawnPoint.position : new Vector3(0, 1, 0);
            GameObject player = Instantiate(characterPrefabs[selectedClass], spawnPosition, Quaternion.identity);
            if (player.GetComponent<PlayerController>() == null)
                player.AddComponent<PlayerController>();
            PlayerManager.Instance?.RegisterPlayer(player.transform);
        }
    }
}
