using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Game Manager handles character spawning and core game logic.
/// </summary>
public class GameManager : MonoBehaviour
{
    public Dictionary<string, GameObject> characterPrefabs = new Dictionary<string, GameObject>();
    
    public GameObject fighterPrefab;
    public GameObject magePrefab;
    public GameObject archerPrefab;
    public GameObject healerPrefab;

    private void Awake()
    {
        characterPrefabs["Fighter"] = fighterPrefab;
        characterPrefabs["Mage"] = magePrefab;
        characterPrefabs["Archer"] = archerPrefab;
        characterPrefabs["Healer"] = healerPrefab;
    }

    private void Start()
    {
        string selectedClass = PlayerPrefs.GetString("SelectedClass", "Fighter").Trim();

        if (string.IsNullOrEmpty(selectedClass) || !characterPrefabs.ContainsKey(selectedClass))
        {
            Debug.LogWarning("Invalid class selection, defaulting to Fighter.");
            selectedClass = "Fighter";
        }

        Vector3 spawnPosition = new Vector3(0, 1, 0);
        GameObject player = Instantiate(characterPrefabs[selectedClass], spawnPosition, Quaternion.identity);
        player.AddComponent<PlayerController>();
    }
}
