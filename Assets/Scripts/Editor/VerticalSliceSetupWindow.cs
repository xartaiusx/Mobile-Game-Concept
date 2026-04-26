#if UNITY_EDITOR
using Game.Classes;
using Game.Combat;
using Game.Core;
using Game.Rhythm;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.EditorTools
{
    public class VerticalSliceSetupWindow : EditorWindow
    {
        private const string AssetFolder = "Assets/VerticalSlice";

        [MenuItem("Game/Vertical Slice/Create Test Scene")]
        public static void Open()
        {
            GetWindow<VerticalSliceSetupWindow>("Vertical Slice");
        }

        private void OnGUI()
        {
            GUILayout.Label("Mobile Rhythm RPG Vertical Slice", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Creates placeholder scene objects and default ScriptableObject assets. Replace primitives with real prefabs when art and animation arrive.", MessageType.Info);

            if (GUILayout.Button("Create Basic Test Scene"))
                CreateScene();
        }

        private static void CreateScene()
        {
            EnsureFolder(AssetFolder);
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            scene.name = "VerticalSlice_Test";

            RhythmConfig rhythmConfig = CreateOrLoadAsset<RhythmConfig>(AssetFolder + "/DefaultRhythmConfig.asset");
            rhythmConfig.bpm = 120f;
            rhythmConfig.perfectWindow = 0.05f;
            rhythmConfig.goodWindow = 0.1f;
            EditorUtility.SetDirty(rhythmConfig);

            ComboProfile comboProfile = CreateOrLoadAsset<ComboProfile>(AssetFolder + "/DefaultComboProfile.asset");
            comboProfile.steps = new[]
            {
                new ComboProfile.Step { baseDamage = 8, cooldown = 0.2f, tag = "Light" },
                new ComboProfile.Step { baseDamage = 12, cooldown = 0.25f, tag = "Heavy" }
            };
            comboProfile.comboTimeout = 1.5f;
            EditorUtility.SetDirty(comboProfile);

            AbilityDefinition fighterAbility = CreateOrLoadAsset<AbilityDefinition>(AssetFolder + "/FighterPulseSlash.asset");
            fighterAbility.abilityId = "fighter_pulse_slash";
            fighterAbility.displayName = "Pulse Slash";
            fighterAbility.description = "A forward rhythm slash for the first vertical slice.";
            fighterAbility.baseDamage = 18;
            fighterAbility.cooldown = 2f;
            fighterAbility.range = 4f;
            fighterAbility.radius = 45f;
            fighterAbility.abilityType = AbilityType.Damage;
            fighterAbility.targetMode = AbilityTargetMode.ForwardCone;
            EditorUtility.SetDirty(fighterAbility);

            BossTelegraphData telegraphData = CreateOrLoadAsset<BossTelegraphData>(AssetFolder + "/BossDownbeatSlam.asset");
            telegraphData.telegraphId = "boss_downbeat_slam";
            telegraphData.displayName = "Downbeat Slam";
            telegraphData.beatsBeforeImpact = 2;
            telegraphData.damage = 15;
            telegraphData.radius = 3f;
            telegraphData.attackType = BossTelegraphAttackType.TargetedCircle;
            EditorUtility.SetDirty(telegraphData);

            var playerManager = new GameObject("PlayerManager").AddComponent<PlayerManager>();
            _ = playerManager;

            var gameManager = new GameObject("GameManager").AddComponent<GameManager>();
            _ = gameManager;

            GameObject rhythm = new GameObject("RhythmSystem");
            var beatClock = rhythm.AddComponent<BeatClock>();
            var rhythmJudgement = rhythm.AddComponent<RhythmJudgement>();
            SetObject(beatClock, "config", rhythmConfig);
            SetObject(rhythmJudgement, "config", rhythmConfig);

            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player_Fighter_Placeholder";
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 1f, 0f);
            Object.DestroyImmediate(player.GetComponent<Collider>());
            player.AddComponent<CharacterController>();
            player.AddComponent<Fighter>();
            var playerController = player.AddComponent<PlayerController>();
            var playerJudge = player.AddComponent<RhythmJudgement>();
            SetObject(playerJudge, "config", rhythmConfig);

            var attackBuffer = player.AddComponent<InputBuffer>();
            var abilityBuffer = player.AddComponent<InputBuffer>();
            var dodgeBuffer = player.AddComponent<InputBuffer>();
            var parryBuffer = player.AddComponent<InputBuffer>();
            SetObject(attackBuffer, "judgement", playerJudge);
            SetObject(abilityBuffer, "judgement", playerJudge);
            SetObject(dodgeBuffer, "judgement", playerJudge);
            SetObject(parryBuffer, "judgement", playerJudge);
            SetObject(playerController, "attackBuffer", attackBuffer);

            var combo = player.AddComponent<ComboSystem>();
            SetObject(combo, "profile", comboProfile);
            SetObject(combo, "judgement", playerJudge);

            var abilities = player.AddComponent<AbilityController>();
            SetObject(abilities, "abilityBuffer", abilityBuffer);
            SetObject(abilities, "abilities", new[] { fighterAbility });

            var dodge = player.AddComponent<DodgeController>();
            SetObject(dodge, "dodgeBuffer", dodgeBuffer);

            var parry = player.AddComponent<ParryController>();
            SetObject(parry, "parryBuffer", parryBuffer);

            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "MeleeEnemy_Placeholder";
            enemy.transform.position = new Vector3(4f, 1f, 4f);
            Object.DestroyImmediate(enemy.GetComponent<Collider>());
            enemy.AddComponent<CharacterController>();
            enemy.AddComponent<MeleeEnemy>();

            GameObject boss = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            boss.name = "Boss_Placeholder";
            boss.transform.position = new Vector3(0f, 1f, 8f);
            Object.DestroyImmediate(boss.GetComponent<Collider>());
            boss.AddComponent<CharacterController>();
            boss.AddComponent<BossEnemy>();
            var telegraph = boss.AddComponent<BossTelegraphController>();
            SetObject(telegraph, "defaultTelegraph", telegraphData);

            if (Camera.main != null)
                Camera.main.transform.SetPositionAndRotation(new Vector3(0f, 8f, -8f), Quaternion.Euler(55f, 0f, 0f));

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("Vertical slice placeholder scene created. Save the scene into Assets/Scenes when ready.");
        }

        private static T CreateOrLoadAsset<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;

            asset = CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;

            string[] parts = folder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static void SetObject(Object target, string fieldName, Object value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(fieldName);
            if (property != null)
            {
                property.objectReferenceValue = value;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void SetObject<T>(Object target, string fieldName, T[] values) where T : Object
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(fieldName);
            if (property == null || !property.isArray) return;

            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
