using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>
    /// Handles character selection UI and loading the chosen class into the game.
    /// </summary>
    public class CharacterSelection : MonoBehaviour
    {
        private const string GAME_SCENE = "World";
        public List<Button> classButtons;
        private Dictionary<string, Button> classButtonMap;

        private void Start()
        {
            classButtonMap = new Dictionary<string, Button>();

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

                string capturedClassName = className;
                button.onClick.AddListener(() => SelectClass(capturedClassName));
            }
        }

        private void SelectClass(string className)
        {
            PlayerPrefs.SetString("SelectedClass", className);

            if (Application.CanStreamedLevelBeLoaded(GAME_SCENE))
                SceneManager.LoadScene(GAME_SCENE);
            else
                Debug.LogError($"Scene '{GAME_SCENE}' could not be loaded. Ensure it is added to the build settings.");
        }
    }
}
