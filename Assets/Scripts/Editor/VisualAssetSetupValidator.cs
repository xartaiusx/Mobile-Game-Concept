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
        private const string KnightModelPath = "Assets/ThirdParty/KayKit/Adventurers/Characters/fbx/Knight.fbx";
        private const string RogueModelPath = "Assets/ThirdParty/KayKit/Adventurers/Characters/fbx/Rogue.fbx";
        private const string BarbarianModelPath = "Assets/ThirdParty/KayKit/Adventurers/Characters/fbx/Barbarian.fbx";
        private const string AnimationGeneralPath = "Assets/ThirdParty/KayKit/CharacterAnimations/AdventurersAnimations/fbx/Rig_Medium/Rig_Medium_General.fbx";
        private const string AnimationMovementPath = "Assets/ThirdParty/KayKit/CharacterAnimations/AdventurersAnimations/fbx/Rig_Medium/Rig_Medium_MovementBasic.fbx";

        private static readonly string[] RequiredFolders =
        {
            "Assets/ThirdParty",
            "Assets/ThirdParty/KayKit",
            "Assets/ThirdParty/KayKit/Adventurers",
            "Assets/ThirdParty/KayKit/CharacterAnimations",
            "Assets/ThirdParty/Kenney",
            "Assets/ThirdParty/Kenney/TinyDungeon",
            "Assets/ThirdParty/Licenses",
            "Assets/ThirdParty/_Documentation",
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

        public static readonly string[] SelectedModelPaths =
        {
            KnightModelPath,
            RogueModelPath,
            BarbarianModelPath
        };

        public static readonly string[] SelectedAnimationClipNames =
        {
            "Idle_A",
            "Running_A",
            "Walking_A",
            "Hit_A",
            "Death_A"
        };

        public static readonly string[] SelectedKenneyTilePaths =
        {
            "Assets/ThirdParty/Kenney/TinyDungeon/Tiles/tile_0000.png",
            "Assets/ThirdParty/Kenney/TinyDungeon/Tiles/tile_0001.png",
            "Assets/ThirdParty/Kenney/TinyDungeon/Tiles/tile_0002.png",
            "Assets/ThirdParty/Kenney/TinyDungeon/Tiles/tile_0016.png",
            "Assets/ThirdParty/Kenney/TinyDungeon/Tiles/tile_0017.png"
        };

        private static readonly VisualAttachmentPlan[] VisualAttachmentPlans =
        {
            new VisualAttachmentPlan("Assets/Prefabs/Player/FighterPlayer.prefab", WarriorVisualPath, "WarriorVisual"),
            new VisualAttachmentPlan("Assets/Prefabs/Enemies/MeleeEnemy.prefab", BasicEnemyVisualPath, "BasicEnemyVisual"),
            new VisualAttachmentPlan("Assets/Prefabs/Enemies/RangedEnemy.prefab", BasicEnemyVisualPath, "BasicEnemyVisual"),
            new VisualAttachmentPlan("Assets/Prefabs/Enemies/BossEnemy.prefab", BossVisualPath, "BossVisual")
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

        [MenuItem("Game/Visuals/Apply First Art Pass")]
        public static void ApplyFirstArtPassMenu()
        {
            ApplyFirstArtPass();
            Debug.Log("First art pass visual prefabs wired.");
        }

        public static void ApplyFirstArtPass()
        {
            NormalizeImportedAssetLayout();
            CreateVisualScaffold();
            WireVisualPrefab(WarriorVisualPath, KnightModelPath, WarriorControllerPath, Vector3.zero, Quaternion.identity, Vector3.one);
            WireVisualPrefab(BasicEnemyVisualPath, RogueModelPath, BasicEnemyControllerPath, Vector3.zero, Quaternion.identity, Vector3.one * 0.95f);
            WireVisualPrefab(BossVisualPath, BarbarianModelPath, BossControllerPath, Vector3.zero, Quaternion.identity, Vector3.one * 1.55f);
            AssignControllerClips(WarriorControllerPath);
            AssignControllerClips(BasicEnemyControllerPath);
            AssignControllerClips(BossControllerPath);
            EnsureFirstPassVisualAttachments();
            AssetDatabase.SaveAssets();
            NormalizeGeneratedVisualYaml();
            AssetDatabase.Refresh();
        }

        public static void NormalizeImportedAssetLayout()
        {
            EnsureFolder("Assets/ThirdParty/_Documentation");
            EnsureFolder("Assets/ThirdParty/_Documentation/KayKitAdventurers");
            MoveAssetIfPresent("Assets/ThirdParty/KayKit/Adventurers/Animations", "Assets/ThirdParty/KayKit/CharacterAnimations/AdventurersAnimations");
            MoveAssetIfPresent("Assets/ThirdParty/KayKit/Mannequin Character", "Assets/ThirdParty/KayKit/CharacterAnimations/MannequinCharacter");
            MoveAssetIfPresent("Assets/ThirdParty/KayKit/Adventurers/Samples", "Assets/ThirdParty/_Documentation/KayKitAdventurers/Samples");
            MoveAssetIfPresent("Assets/ThirdParty/KayKit/Adventurers/contents.png", "Assets/ThirdParty/_Documentation/KayKitAdventurers/contents.png");
            MoveAssetIfPresent("Assets/ThirdParty/KayKit/Adventurers/More KayKit Assets.url", "Assets/ThirdParty/_Documentation/KayKitAdventurers/More KayKit Assets.url");
            MoveAssetIfPresent("Assets/ThirdParty/KayKit/Adventurers/Patreon.url", "Assets/ThirdParty/_Documentation/KayKitAdventurers/Patreon.url");
            MoveAssetIfPresent("Assets/ThirdParty/KayKit/CharacterAnimations/AdventurersAnimations/Click here for more Free Animations.url", "Assets/ThirdParty/_Documentation/KayKitAdventurers/Click here for more Free Animations.url");
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

            for (int i = 0; i < SelectedModelPaths.Length; i++)
            {
                if (AssetDatabase.LoadAssetAtPath<GameObject>(SelectedModelPaths[i]) == null)
                    failures.Add("Missing selected first-pass model: " + SelectedModelPaths[i]);
            }

            for (int i = 0; i < SelectedKenneyTilePaths.Length; i++)
            {
                if (AssetDatabase.LoadAssetAtPath<Texture2D>(SelectedKenneyTilePaths[i]) == null)
                    failures.Add("Missing selected Kenney Tiny Dungeon tile: " + SelectedKenneyTilePaths[i]);
            }

            if (!VisualPrefabContainsSelectedModel(WarriorVisualPath, "SelectedModel_Knight"))
                failures.Add("WarriorVisual.prefab is not wired to SelectedModel_Knight.");
            if (!VisualPrefabContainsSelectedModel(BasicEnemyVisualPath, "SelectedModel_Rogue"))
                failures.Add("BasicEnemyVisual.prefab is not wired to SelectedModel_Rogue.");
            if (!VisualPrefabContainsSelectedModel(BossVisualPath, "SelectedModel_Barbarian"))
                failures.Add("BossVisual.prefab is not wired to SelectedModel_Barbarian.");

            return failures;
        }

        public static void EnsureGameplayPrefabVisualRoots()
        {
            for (int i = 0; i < GameplayPrefabPaths.Length; i++)
                EnsureGameplayPrefabVisualRoot(GameplayPrefabPaths[i]);

            EnsureFirstPassVisualAttachments();
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

        private static void MoveAssetIfPresent(string sourcePath, string destinationPath)
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(sourcePath) == null && !AssetDatabase.IsValidFolder(sourcePath))
                return;

            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(destinationPath) != null || AssetDatabase.IsValidFolder(destinationPath))
                return;

            string parent = Path.GetDirectoryName(destinationPath).Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent))
                EnsureFolder(parent);

            string error = AssetDatabase.MoveAsset(sourcePath, destinationPath);
            if (!string.IsNullOrEmpty(error))
                Debug.LogWarning("Visual asset move skipped: " + sourcePath + " -> " + destinationPath + " :: " + error);
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

        private static void WireVisualPrefab(string visualPrefabPath, string modelPath, string controllerPath, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (model == null)
                return;

            GameObject root = PrefabUtility.LoadPrefabContents(visualPrefabPath);
            Transform visualRoot = root.transform.Find("VisualRoot");
            if (visualRoot == null)
            {
                GameObject visualRootObject = new GameObject("VisualRoot");
                visualRootObject.transform.SetParent(root.transform, false);
                visualRoot = visualRootObject.transform;
            }

            for (int i = visualRoot.childCount - 1; i >= 0; i--)
                UnityEngine.Object.DestroyImmediate(visualRoot.GetChild(i).gameObject);

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
            instance.name = "SelectedModel_" + Path.GetFileNameWithoutExtension(modelPath);
            instance.transform.SetParent(visualRoot, false);
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = localRotation;
            instance.transform.localScale = localScale;

            Animator animator = visualRoot.GetComponent<Animator>();
            if (animator == null)
                animator = visualRoot.gameObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);

            Material fallbackMaterial = EnsurePlaceholderMaterial();
            Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] materials = renderers[i].sharedMaterials;
                for (int j = 0; j < materials.Length; j++)
                {
                    if (materials[j] == null)
                        materials[j] = fallbackMaterial;
                }
                renderers[i].sharedMaterials = materials;
            }

            var binder = root.GetComponent<VisualAttachmentRoot>();
            if (binder == null)
                binder = root.AddComponent<VisualAttachmentRoot>();
            binder.Configure(visualRoot, animator, renderers);
            PrefabUtility.SaveAsPrefabAsset(root, visualPrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void AssignControllerClips(string controllerPath)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null)
                return;

            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            AssignStateMotion(machine, "Idle", FindClip(AnimationGeneralPath, "Idle_A"));
            AssignStateMotion(machine, "Move", FindClip(AnimationMovementPath, "Running_A"));
            AssignStateMotion(machine, "Hit", FindClip(AnimationGeneralPath, "Hit_A"));
            AssignStateMotion(machine, "Death", FindClip(AnimationGeneralPath, "Death_A"));
            EditorUtility.SetDirty(controller);
        }

        private static void AssignStateMotion(AnimatorStateMachine machine, string stateName, Motion motion)
        {
            if (motion == null)
                return;

            ChildAnimatorState[] states = machine.states;
            for (int i = 0; i < states.Length; i++)
            {
                if (states[i].state != null && states[i].state.name == stateName)
                {
                    states[i].state.motion = motion;
                    return;
                }
            }
        }

        private static AnimationClip FindClip(string assetPath, string clipName)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            for (int i = 0; i < assets.Length; i++)
            {
                var clip = assets[i] as AnimationClip;
                if (clip != null && clip.name == clipName)
                    return clip;
            }

            return null;
        }

        private static bool VisualPrefabContainsSelectedModel(string path, string modelName)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                return false;

            Transform visualRoot = prefab.transform.Find("VisualRoot");
            return visualRoot != null && visualRoot.Find(modelName) != null;
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

        private static void EnsureFirstPassVisualAttachments()
        {
            for (int i = 0; i < VisualAttachmentPlans.Length; i++)
                EnsureVisualAttachment(VisualAttachmentPlans[i]);
        }

        private static void EnsureVisualAttachment(VisualAttachmentPlan plan)
        {
            GameObject gameplayPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(plan.gameplayPrefabPath);
            GameObject visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(plan.visualPrefabPath);
            if (gameplayPrefab == null || visualPrefab == null)
                return;

            GameObject root = PrefabUtility.LoadPrefabContents(plan.gameplayPrefabPath);
            bool changed = false;
            Transform visualRoot = root.transform.Find("VisualRoot");
            if (visualRoot == null)
            {
                GameObject visualRootObject = new GameObject("VisualRoot");
                visualRootObject.transform.SetParent(root.transform, false);
                visualRoot = visualRootObject.transform;
                changed = true;
            }

            if (visualRoot.Find(plan.instanceName) == null && visualRoot.childCount == 0)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(visualPrefab, visualRoot);
                instance.name = plan.instanceName;
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
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
                PrefabUtility.SaveAsPrefabAsset(root, plan.gameplayPrefabPath);

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

            for (int i = 0; i < VisualAttachmentPlans.Length; i++)
            {
                if (!string.Equals(VisualAttachmentPlans[i].gameplayPrefabPath, path, StringComparison.Ordinal))
                    continue;

                Transform visualRoot = prefab.transform.Find("VisualRoot");
                if (visualRoot != null && visualRoot.Find(VisualAttachmentPlans[i].instanceName) == null)
                    failures.Add(path + " is missing first-pass visual attachment " + VisualAttachmentPlans[i].instanceName + ".");
            }
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

        private readonly struct VisualAttachmentPlan
        {
            public readonly string gameplayPrefabPath;
            public readonly string visualPrefabPath;
            public readonly string instanceName;

            public VisualAttachmentPlan(string gameplayPrefabPath, string visualPrefabPath, string instanceName)
            {
                this.gameplayPrefabPath = gameplayPrefabPath;
                this.visualPrefabPath = visualPrefabPath;
                this.instanceName = instanceName;
            }
        }
    }
}
#endif
