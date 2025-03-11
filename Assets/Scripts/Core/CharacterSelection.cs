using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Handles character selection UI and loading the chosen class into the game.
/// </summary>
public class CharacterSelection : MonoBehaviour
{
    private const string GAME_SCENE = "World";
    public List<Button> classButtons;
    private Dictionary<string, Button> classButtonMap = new Dictionary<string, Button>();

    void Start()
    {
        if (classButtons == null || classButtons.Count == 0)
        {
            Debug.LogError("No class buttons assigned in the Inspector.");
            return;
        }

        foreach (Button button in classButtons)
        {
            if (button == null)
            {
                Debug.LogError("One or more class buttons are not assigned in the Inspector.");
                continue;
            }

            string className = button.name;
            classButtonMap[className] = button;
            button.onClick.AddListener(() => SelectClass(className));
        }
    }

    void SelectClass(string className)
    {
        PlayerPrefs.SetString("SelectedClass", className);
        SceneManager.LoadScene(GAME_SCENE);
    }
}
