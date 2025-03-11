using UnityEngine;

/// <summary>
/// Game Manager handles character spawning and core game logic.
/// </summary>
public class GameManager : MonoBehaviour
{
    public GameObject fighterPrefab;
    public GameObject magePrefab;
    public GameObject archerPrefab;
    public GameObject healerPrefab;

    private void Start()
    {
        string selectedClass = PlayerPrefs.GetString("SelectedClass", "Fighter");

        Vector3 spawnPosition = new Vector3(0, 1, 0);
        GameObject player;

        switch (selectedClass)
        {
            case "Mage":
                player = Instantiate(magePrefab, spawnPosition, Quaternion.identity);
                break;
            case "Archer":
                player = Instantiate(archerPrefab, spawnPosition, Quaternion.identity);
                break;
            case "Healer":
                player = Instantiate(healerPrefab, spawnPosition, Quaternion.identity);
                break;
            default:
                player = Instantiate(fighterPrefab, spawnPosition, Quaternion.identity);
                break;
        }

        player.AddComponent<PlayerController>();
    }
}
