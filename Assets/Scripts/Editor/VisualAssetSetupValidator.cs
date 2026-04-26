#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Game.Visuals;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Game.Editor
{
    public static class VisualAssetSetupValidator
    {
        private const string LicenseManifestPath = "Assets/ThirdParty/Licenses/ASSET_LICENSES.md";
        private const string VisualMaterialPath = "Assets/Art/Materials/VisualPlaceholder.mat";
        private const string WarriorVisualPath = "Assets/Art/Prefabs/WarriorVisual.prefab";
        private const string BasicEnemyVisualPath = "Assets/Art/Prefabs/BasicEnemyVisual.prefab";
        private const string BossVisualPath = "Assets/Art/Prefabs/BossVisual.prefab";
        private const string WarriorControllerPath = "Assets/Art/Animation/Controllers/WarriorVisual.controller";
        private const string BasicEnemyControllerPath = "Assets/Art/Animation/Controllers/BasicEnemyVisual.controller";
        private const string BossControllerPath = "Assets/Art/Animation/Controllers/BossVisual.controller";

        private static readonly string[] RequiredFolders =
        {
            "Assets/ThirdParty",
            "Assets/ThirdParty/KayKit",
            "Assets/ThirdParty/KayKit/Adventurers",
            "Assets/ThirdParty/KayKit/CharacterAnimations",
            "Assets/ThirdParty/Kenney",
            "Assets/ThirdParty/Kenney/TinyDungeon",
            "Assets/ThirdParty/Licenses",
            "Assets/Art",
            "Assets/Art/Characters",
            "Assets/Art/Enemies",
            "Assets/Art/Bosses",
            "Assets/Art/Environment",
            "Assets/Art/Materials",
            "Assets/Art/Animation",
            "Assets/Art/Animation/Controllers",
            "Assets/Art/Prefabs",
            "Assets/Generated",
            "Assets/Generated/Visuals"
        };

        private static readonly string[] GameplayPrefabPaths =
        {
            "Assets/Prefabs/Player/FighterPlayer.prefab",
            "Assets/Prefabs/Enemies/MeleeEnemy.prefab",
            "Assets/Prefabs/Enemies/RangedEnemy.prefab",
            "Assets/Prefabs/Enemies/BossEnemy.prefab"
        };

        private static readonly string[] VisualPrefabPaths =
        {
            WarriorVisualPath,
            BasicEnemyVisualPath,
            BossVisualPath
        };

        [MenuItem("Game/Visuals/Create Visual Folders")]
        public static void CreateVisualFoldersMenu()
        {
            CreateVisualScaffold();
            Debug.Log("Visual asset folders and placeholder assets are ready.");
        }

        [MenuItem("Game/Visuals/Validate Visual Asset Setup")]
        public static bool ValidateVisualAssetSetupMenu()
        {
            return ValidateVisualAssetSetup(true);
        }

        public static void CreateVisualScaffold()
        {
            EnsureFolders();
            EnsurePlaceholderMaterial();
            EnsureController(WarriorControllerPath);
            EnsureController(BasicEnemyControllerPath);
            EnsureController(BossControllerPath);
            EnsureVisualPrefab(WarriorVisualPath, WarriorControllerPath, PrimitiveType.Capsule, new Vector3(0f, 1f, 0f), Vector3.one);
            EnsureVisualPrefab(BasicEnemyVisualPath, BasicEnemyControllerPath, PrimitiveType.Capsule, new Vector3(0f, 1f, 0f), Vector3.one * 0.9f);
            EnsureVisualPrefab(BossVisualPath, BossControllerPath, PrimitiveType.Capsule, new Vector3(0f, 1.5f, 0f), new Vector3(1.6f, 1.6f, 1.6f));
            EnsureGameplayPrefabVisualRoots();
            AssetDatabase.SaveAssets();
            NormalizeGeneratedVisualYaml();
            AssetDatabase.Refresh();
        }

        public static bool ValidateVisualAssetSetup(bool logResult)
        {
            var failures = GetValidationFailures();
            if (failures.Count == 0)
            {
                if (logResult)
                    Debug.Log("Visual asset setup validation passed. " + GetImportedAssetReport());
                return true;
            }

            if (logResult)
                Debug.LogError("Visual asset setup validation failed:\n" + string.Join("\n", failures));
            return false;
        }

        public static List<string> GetValidationFailures()
        {
            var failures = new List<string>();
            for (int i = 0; i < RequiredFolders.Length; i++)
            {
                if (!AssetDatabase.IsValidFolder(RequiredFolders[i]))
                    failures.Add("Missing visual folder: " + RequiredFolders[i]);
            }

            if (!File.Exists(LicenseManifestPath))
                failures.Add("Missing license manifest: " + LicenseManifestPath);

            for (int i = 0; i < VisualPrefabPaths.Length; i++)
                ValidateVisualPrefab(VisualPrefabPaths[i], failures);

            for (int i = 0; i < GameplayPrefabPaths.Length; i++)
                ValidateGameplayPrefabVisualRoot(GameplayPrefabPaths[i], failures);

            return failures;
        }

        public static void EnsureGameplayPrefabVisualRoots()
        {
            for (int i = 0; i < GameplayPrefabPaths.Length; i++)
                EnsureGameplayPrefabVisualRoot(GameplayPrefabPaths[i]);
        }

        public static string GetImportedAssetReport()
        {
            int modelCount = AssetDatabase.FindAssets("t:Model", new[] { "Assets/ThirdParty" }).Length;
            int clipCount = AssetDatabase.FindAssets("t:AnimationClip", new[] { "Assets/ThirdParty" }).Length;
            int materialCount = AssetDatabase.FindAssets("t:Material", new[] { "Assets/ThirdParty" }).Length;
            return "Imported third-party assets: models=" + modelCount + ", clips=" + clipCount + ", materials=" + materialCount + ". Empty recommended source folders are allowed.";
        }

        private static void EnsureFolders()
        {
            for (int i = 0; i < RequiredFolders.Length; i++)
                EnsureFolder(RequiredFolders[i]);
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
                return;

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

        private static Material EnsurePlaceholderMaterial()
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(VisualMaterialPath);
            if (material != null)
                return material;

            material = new Material(Shader.Find("Standard"));
            material.color = new Color(0.55f, 0.68f, 0.78f, 1f);
            AssetDatabase.CreateAsset(material, VisualMaterialPath);
            return material;
        }

        private static void EnsureController(string path)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null)
                controller = AnimatorController.CreateAnimatorControllerAtPath(path);

            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            EnsureState(machine, "Idle", new Vector3(240f, 60f, 0f));
            EnsureState(machine, "Move", new Vector3(240f, 140f, 0f));
            EnsureState(machine, "Attack", new Vector3(240f, 220f, 0f));
            EnsureState(machine, "Evade", new Vector3(240f, 300f, 0f));
            EnsureState(machine, "Hit", new Vector3(520f, 180f, 0f));
            EnsureState(machine, "Death", new Vector3(520f, 300f, 0f));
            EditorUtility.SetDirty(controller);
        }

        private static void EnsureState(AnimatorStateMachine machine, string stateName, Vector3 position)
        {
            ChildAnimatorState[] states = machine.states;
            for (int i = 0; i < states.Length; i++)
            {
                if (states[i].state != null && states[i].state.name == stateName)
                    return;
            }

            AnimatorState state = machine.AddState(stateName, position);
            if (machine.defaultState == null)
                machine.defaultState = state;
        }

        private static void EnsureVisualPrefab(string path, string controllerPath, PrimitiveType primitive, Vector3 localPosition, Vector3 localScale)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                return;

            Material material = EnsurePlaceholderMaterial();
            GameObject root = new GameObject(Path.GetFileNameWithoutExtension(path));
            var binder = root.AddComponent<VisualAttachmentRoot>();
            GameObject visualRoot = new GameObject("VisualRoot");
            visualRoot.transform.SetParent(root.transform, false);

            GameObject placeholder = GameObject.CreatePrimitive(primitive);
            placeholder.name = "PlaceholderModel";
            placeholder.transform.SetParent(visualRoot.transform, false);
            placeholder.transform.localPosition = localPosition;
            placeholder.transform.localScale = localScale;
            Collider collider = placeholder.GetComponent<Collider>();
            if (collider != null)
                UnityEngine.Object.DestroyImmediate(collider);

            Renderer renderer = placeholder.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;

            Animator animator = visualRoot.AddComponent<Animator>();
            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
            binder.Configure(visualRoot.transform, animator, renderer != null ? new[] { renderer } : Array.Empty<Renderer>());

            PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
        }

        private static void EnsureGameplayPrefabVisualRoot(string path)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                return;

            GameObject root = PrefabUtility.LoadPrefabContents(path);
            bool changed = false;
            Transform visualRoot = root.transform.Find("VisualRoot");
            if (visualRoot == null)
            {
                GameObject visualRootObject = new GameObject("VisualRoot");
                visualRootObject.transform.SetParent(root.transform, false);
                visualRoot = visualRootObject.transform;
                changed = true;
            }

            var binder = root.GetComponent<VisualAttachmentRoot>();
            if (binder == null)
            {
                binder = root.AddComponent<VisualAttachmentRoot>();
                changed = true;
            }

            Animator animator = visualRoot.GetComponentInChildren<Animator>(true);
            Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
            binder.Configure(visualRoot, animator, renderers);

            if (changed)
                PrefabUtility.SaveAsPrefabAsset(root, path);

            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void ValidateVisualPrefab(string path, List<string> failures)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                failures.Add("Missing visual wrapper prefab: " + path);
                return;
            }

            ValidateNoMissingScriptsAndMaterials(prefab, path, failures);
            if (prefab.GetComponent<VisualAttachmentRoot>() == null)
                failures.Add(path + " is missing VisualAttachmentRoot.");
        }

        private static void ValidateGameplayPrefabVisualRoot(string path, List<string> failures)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                failures.Add("Missing gameplay prefab: " + path);
                return;
            }

            ValidateNoMissingScriptsAndMaterials(prefab, path, failures);
            if (prefab.transform.Find("VisualRoot") == null)
                failures.Add(path + " is missing VisualRoot child.");
            if (prefab.GetComponent<VisualAttachmentRoot>() == null)
                failures.Add(path + " is missing VisualAttachmentRoot.");
        }

        private static void ValidateNoMissingScriptsAndMaterials(GameObject prefab, string path, List<string> failures)
        {
            if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab) > 0)
                failures.Add(path + " has missing scripts.");

            Component[] components = prefab.GetComponentsInChildren<Component>(true);
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                    failures.Add(path + " has a missing script on a child object.");
            }

            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] materials = renderers[i].sharedMaterials;
                for (int j = 0; j < materials.Length; j++)
                {
                    if (materials[j] == null)
                        failures.Add(path + " has a missing material on " + renderers[i].name + ".");
                }
            }
        }

        private static void NormalizeGeneratedVisualYaml()
        {
            NormalizeYamlFiles("Assets/Art", "*.prefab");
            NormalizeYamlFiles("Assets/Art", "*.controller");
            NormalizeYamlFiles("Assets/Art", "*.mat");
            NormalizeYamlFiles("Assets/Art", "*.meta");
            NormalizeYamlFiles("Assets/Generated", "*.meta");
            NormalizeYamlFiles("Assets/ThirdParty", "*.meta");
            NormalizeYamlFiles("Assets/Prefabs/Player", "*.prefab");
            NormalizeYamlFiles("Assets/Prefabs/Enemies", "*.prefab");
        }

        private static void NormalizeYamlFiles(string folder, string pattern)
        {
            if (!Directory.Exists(folder))
                return;

            string[] files = Directory.GetFiles(folder, pattern, SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                string text = File.ReadAllText(files[i]);
                string normalized = StripTrailingWhitespace(text);
                if (!string.Equals(text, normalized, StringComparison.Ordinal))
                    File.WriteAllText(files[i], normalized);
            }
        }

        private static string StripTrailingWhitespace(string text)
        {
            text = text.Replace("\r\n", "\n").Replace('\r', '\n');
            string[] lines = text.Split('\n');
            var builder = new StringBuilder(text.Length);
            for (int i = 0; i < lines.Length; i++)
            {
                builder.Append(lines[i].TrimEnd(' ', '\t'));
                if (i < lines.Length - 1)
                    builder.Append('\n');
            }

            return builder.ToString();
        }
    }
}
#endif
