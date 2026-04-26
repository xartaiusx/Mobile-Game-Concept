using Game.Combat;
using Game.Feedback;
using Game.UI;
using UnityEngine;

namespace Game.Core
{
    public class ClassSwapDebugController : MonoBehaviour
    {
        [SerializeField] private bool enableDebugSwap = true;
        [SerializeField] private GameObject fighterPrefab;
        [SerializeField] private GameObject magePrefab;
        [SerializeField] private GameObject archerPrefab;
        [SerializeField] private GameObject healerPrefab;

        public GameObject CurrentPlayer { get; private set; }

        private void Start()
        {
            ResolveCurrentPlayer();
        }

        private void Update()
        {
            if (!enableDebugSwap) return;

            if (Input.GetKeyDown(KeyCode.F1)) SwapToClass("Fighter");
            else if (Input.GetKeyDown(KeyCode.F2)) SwapToClass("Mage");
            else if (Input.GetKeyDown(KeyCode.F3)) SwapToClass("Archer");
            else if (Input.GetKeyDown(KeyCode.F4)) SwapToClass("Healer");
        }

        public GameObject SwapToClass(string className)
        {
            GameObject prefab = PrefabFor(className);
            if (prefab == null)
            {
                Debug.LogWarning("No debug class prefab configured for " + className + ".");
                return CurrentPlayer;
            }

            ResolveCurrentPlayer();
            Vector3 position = CurrentPlayer != null ? CurrentPlayer.transform.position : new Vector3(0f, 1f, -8f);
            Quaternion rotation = CurrentPlayer != null ? CurrentPlayer.transform.rotation : Quaternion.identity;

            if (CurrentPlayer != null)
            {
                PlayerManager.Instance?.UnregisterPlayer(CurrentPlayer.transform);
                if (Application.isPlaying)
                    Destroy(CurrentPlayer);
                else
                    DestroyImmediate(CurrentPlayer);
            }

            CurrentPlayer = Instantiate(prefab, position, rotation);
            CurrentPlayer.name = "Player_" + className;
            CurrentPlayer.tag = "Player";
            CurrentPlayer.SetActive(true);
            PlayerManager.Instance?.RegisterPlayer(CurrentPlayer.transform);
            RebindSceneReferences(CurrentPlayer);
            return CurrentPlayer;
        }

        public void Configure(GameObject fighter, GameObject mage, GameObject archer, GameObject healer)
        {
            fighterPrefab = fighter;
            magePrefab = mage;
            archerPrefab = archer;
            healerPrefab = healer;
            PrepareSceneTemplate(fighterPrefab);
            PrepareSceneTemplate(magePrefab);
            PrepareSceneTemplate(archerPrefab);
            PrepareSceneTemplate(healerPrefab);
        }

        private void ResolveCurrentPlayer()
        {
            Transform managed = PlayerManager.Instance != null ? PlayerManager.Instance.GetPlayerTransform() : null;
            if (managed != null)
            {
                CurrentPlayer = managed.gameObject;
                return;
            }

            GameObject tagged = GameObject.FindGameObjectWithTag("Player");
            if (tagged != null)
            {
                CurrentPlayer = tagged;
                PlayerManager.Instance?.RegisterPlayer(tagged.transform);
            }
        }

        private GameObject PrefabFor(string className)
        {
            switch (className)
            {
                case "Fighter": return fighterPrefab;
                case "Mage": return magePrefab;
                case "Archer": return archerPrefab;
                case "Healer": return healerPrefab;
                default: return null;
            }
        }

        private static void RebindSceneReferences(GameObject player)
        {
            if (player == null) return;

            SimpleFollowCamera camera = FindAnyObjectByType<SimpleFollowCamera>();
            if (camera != null)
                camera.SetTarget(player.transform);

            ArenaController arena = FindAnyObjectByType<ArenaController>();
            if (arena != null)
                arena.ReplacePlayer(player.GetComponent<BaseCharacter>());

            foreach (VerticalSliceHud hud in FindObjectsByType<VerticalSliceHud>(FindObjectsInactive.Exclude))
                hud.BindPlayer(player);

            foreach (RhythmFeedbackController feedback in FindObjectsByType<RhythmFeedbackController>(FindObjectsInactive.Exclude))
                feedback.BindPlayer(player);
        }

        private static void PrepareSceneTemplate(GameObject template)
        {
            if (template == null || !template.scene.IsValid())
                return;

            template.SetActive(false);
            if (template.CompareTag("Player"))
                template.tag = "Untagged";
        }
    }
}
