using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles character selection UI and loading the chosen class into the game.
/// </summary>
public class CharacterSelection : MonoBehaviour
{
    public Button fighterButton;
    public Button mageButton;
    public Button archerButton;
    public Button healerButton;

    private string selectedClass;

    void Start()
    {
        fighterButton.onClick.AddListener(() => SelectClass("Fighter"));
        mageButton.onClick.AddListener(() => SelectClass("Mage"));
        archerButton.onClick.AddListener(() => SelectClass("Archer"));
        healerButton.onClick.AddListener(() => SelectClass("Healer"));
    }

    void SelectClass(string className)
    {
        selectedClass = className;
        PlayerPrefs.SetString("SelectedClass", selectedClass);
        SceneManager.LoadScene("World");
    }
}
