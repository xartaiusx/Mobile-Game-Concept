#if UNITY_EDITOR
using Game.AI.Enemies;
using Game.Audio;
using Game.Classes;
using Game.Combat;
using Game.Core;
using Game.Feedback;
using Game.Rhythm;
using Game.Systems;
using Game.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Game.EditorTools
{
    public class VerticalSliceSetupWindow : EditorWindow
    {
        private const string ScenePath = "Assets/Scenes/VerticalSlice.unity";

        [MenuItem("Game/Vertical Slice/Create Test Scene")]
        public static void Open()
        {
            GetWindow<VerticalSliceSetupWindow>("Vertical Slice");
        }

        [MenuItem("Game/Vertical Slice/Create Or Refresh Vertical Slice")]
        public static void CreatePlayableVerticalSlice()
        {
            EnsureProjectFolders();

            var materials = CreateMaterials();
            var audioCues = CreateAudioCues();
            AnimationSet animations = CreateAnimationAssets();
            RhythmConfig rhythmConfig = CreateRhythmConfig();
            ComboProfile comboProfile = CreateComboProfile();
            AttackTimingData attackTiming = CreateAttackTiming();
            AbilityDefinition fighterSlash = CreateAbility("Assets/ScriptableObjects/Abilities/FighterSlash.asset", "fighter_slash", "Fighter Slash", "Short-range guard-breaking slash. Perfect timing adds a stronger stagger.", 23, 0, 1.05f, 2.7f, 38f, AbilityType.Damage, AbilityTargetMode.ForwardCone, AbilityExecutionStyle.Instant, new AbilityRhythmScaling { perfectMultiplier = 1.65f, goodMultiplier = 1.2f, missMultiplier = 0.75f }, 0.75f, 0f);
            AbilityDefinition mageBolt = CreateAbility("Assets/ScriptableObjects/Abilities/MageBolt.asset", "mage_bolt", "Mage Bolt", "Medium-speed projectile with Perfect splash and stagger.", 30, 0, 2.25f, 8f, 1.5f, AbilityType.Damage, AbilityTargetMode.TargetPoint, AbilityExecutionStyle.ProjectileLike, new AbilityRhythmScaling { perfectMultiplier = 1.65f, goodMultiplier = 1.1f, missMultiplier = 0.6f }, 0.35f, 0f);
            AbilityDefinition archerShot = CreateAbility("Assets/ScriptableObjects/Abilities/ArcherShot.asset", "archer_shot", "Archer Shot", "Fast precision projectile. Perfect timing pierces one target.", 18, 0, 1.25f, 12f, 0.35f, AbilityType.Damage, AbilityTargetMode.TargetPoint, AbilityExecutionStyle.ProjectileLike, new AbilityRhythmScaling { perfectMultiplier = 2.25f, goodMultiplier = 1.15f, missMultiplier = 0.45f }, 0.15f, 0f);
            AbilityDefinition healerPulse = CreateAbility("Assets/ScriptableObjects/Abilities/HealerPulse.asset", "healer_pulse", "Healer Pulse", "Self heal pulse. Perfect timing adds a brief protection window.", 0, 22, 2.4f, 0f, 3.5f, AbilityType.Heal, AbilityTargetMode.Self, AbilityExecutionStyle.Pulse, new AbilityRhythmScaling { perfectMultiplier = 1.65f, goodMultiplier = 1.15f, missMultiplier = 0.75f }, 0f, 0.45f);
            FeedbackPrefabs feedbackPrefabs = CreateFeedbackPrefabs(materials);
            BossTelegraphData bossSlam = CreateBossTelegraph("Assets/ScriptableObjects/Boss/BossSlamTelegraph.asset", "boss_slam", "Boss Slam", BossAttackType.Slam, TelegraphShape.Circle, 4, 1, 1, 10, 3f, 8f, 1.5f, 6f, feedbackPrefabs.bossWarning, feedbackPrefabs.bossImpact);
            BossTelegraphData bossLine = CreateBossTelegraph("Assets/ScriptableObjects/Boss/BossLineTelegraph.asset", "boss_line", "Boss Line", BossAttackType.Line, TelegraphShape.Line, 4, 2, 1, 7, 1.5f, 10f, 2.2f, 10f, feedbackPrefabs.bossWarning, feedbackPrefabs.bossImpact);
            BossPhaseData phaseOne = CreateBossPhase("Assets/ScriptableObjects/Boss/BossPhaseOne.asset", "Phase 1", 1f, false, 1, 0.2f, 1f, 1f, 1f, 1.15f, new[] { bossSlam });
            BossPhaseData phaseTwo = CreateBossPhase("Assets/ScriptableObjects/Boss/BossPhaseTwo.asset", "Phase 2", 0.5f, true, 2, 0.25f, 1.08f, 0.78f, 0.72f, 1.35f, new[] { bossLine, bossSlam });
            ItemDefinition goldItem = CreateItem("Assets/ScriptableObjects/Items/Gold.asset", "gold", "Gold", 999, 1, 0);
            ItemDefinition potionItem = CreateItem("Assets/ScriptableObjects/Items/HealthPotion.asset", "health_potion", "Health Potion", 10, 0, 25);

            GameObject audioPrefab = CreateAudioCuePlayerPrefab();
            GameObject projectilePrefab = CreateProjectilePrefab(materials.projectile, materials.projectileTrail);
            GameObject goldPickupPrefab = CreatePickupPrefab("Assets/Prefabs/Pickups/GoldPickup.prefab", goldItem, materials.healer, PrimitiveType.Sphere, 0.32f);
            GameObject potionPickupPrefab = CreatePickupPrefab("Assets/Prefabs/Pickups/HealthPotionPickup.prefab", potionItem, materials.good, PrimitiveType.Cylinder, 0.38f);
            GameObject fighterPrefab = CreatePlayerPrefab("Assets/Prefabs/Player/FighterPlayer.prefab", typeof(Fighter), fighterSlash, rhythmConfig, comboProfile, attackTiming, projectilePrefab, feedbackPrefabs.bossImpact, materials.player, audioCues, animations.playerController);
            GameObject magePrefab = CreatePlayerPrefab("Assets/Prefabs/Player/MagePlayer.prefab", typeof(Mage), mageBolt, rhythmConfig, comboProfile, attackTiming, projectilePrefab, feedbackPrefabs.bossImpact, materials.mage, audioCues, animations.playerController);
            GameObject archerPrefab = CreatePlayerPrefab("Assets/Prefabs/Player/ArcherPlayer.prefab", typeof(Archer), archerShot, rhythmConfig, comboProfile, attackTiming, projectilePrefab, feedbackPrefabs.perfect, materials.archer, audioCues, animations.playerController);
            GameObject healerPrefab = CreatePlayerPrefab("Assets/Prefabs/Player/HealerPlayer.prefab", typeof(Healer), healerPulse, rhythmConfig, comboProfile, attackTiming, projectilePrefab, feedbackPrefabs.parrySuccess, materials.healer, audioCues, animations.playerController);
            GameObject meleePrefab = CreateMeleeEnemyPrefab(materials.enemy, feedbackPrefabs.enemyWindup, animations.enemyController);
            GameObject rangedPrefab = CreateRangedEnemyPrefab(projectilePrefab, materials.rangedEnemy, feedbackPrefabs.enemyWindup, animations.enemyController);
            GameObject bossPrefab = CreateBossPrefab(projectilePrefab, bossSlam, new[] { phaseOne, phaseTwo }, materials.boss, feedbackPrefabs.enemyWindup, audioCues, animations.bossController);
            GameObject hudPrefab = CreateHudPrefab();
            Game.Editor.VisualAssetSetupValidator.CreateVisualScaffold();
            Game.Editor.VisualAssetSetupValidator.NormalizeImportedAssetLayout();

            if (ShouldRebuildScene())
            {
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                scene.name = "VerticalSlice";
                BuildScene(fighterPrefab, magePrefab, archerPrefab, healerPrefab, meleePrefab, rangedPrefab, bossPrefab, hudPrefab, rhythmConfig, bossSlam, goldPickupPrefab, potionPickupPrefab, materials, feedbackPrefabs, audioCues, audioPrefab);
                EditorSceneManager.SaveScene(scene, ScenePath);
            }

            AddSceneToBuildSettings(ScenePath);
            Game.Editor.VerticalSliceStartup.EnsureStartupSceneConfigured();

            AssetDatabase.SaveAssets();
            NormalizeGeneratedSceneYaml();
            NormalizeGeneratedMaterialYaml();
            AssetDatabase.Refresh();
            Debug.Log("Created playable vertical slice scene at " + ScenePath);
        }

        [MenuItem("Game/Vertical Slice/Validate Generator Idempotency")]
        public static bool ValidateGeneratorIdempotency()
        {
            CreatePlayableVerticalSlice();
            Dictionary<string, string> firstSnapshot = CaptureGeneratedFileHashes();
            CreatePlayableVerticalSlice();
            Dictionary<string, string> secondSnapshot = CaptureGeneratedFileHashes();

            var failures = new List<string>();
            foreach (KeyValuePair<string, string> entry in firstSnapshot)
            {
                if (!secondSnapshot.TryGetValue(entry.Key, out string secondHash))
                {
                    failures.Add("Missing after second generation: " + entry.Key);
                    continue;
                }

                if (!string.Equals(entry.Value, secondHash, StringComparison.Ordinal))
                    failures.Add("Changed after second generation: " + entry.Key);
            }

            foreach (string path in secondSnapshot.Keys)
            {
                if (!firstSnapshot.ContainsKey(path))
                    failures.Add("New file after second generation: " + path);
            }

            failures.AddRange(GetGeneratedContentValidationFailures());

            if (failures.Count == 0)
            {
                Debug.Log("Vertical slice generator idempotency validation passed.");
                return true;
            }

            Debug.LogError("Vertical slice generator idempotency validation failed:\n" + string.Join("\n", failures));
            return false;
        }

        private static bool ShouldRebuildScene()
        {
            return !File.Exists(ScenePath) || GetGeneratedSceneValidationFailures().Count > 0;
        }

        private static List<string> GetGeneratedContentValidationFailures()
        {
            var failures = new List<string>();
            failures.AddRange(GetGeneratedSceneValidationFailures());

            string[] prefabPaths =
            {
                "Assets/Prefabs/Player/FighterPlayer.prefab",
                "Assets/Prefabs/Enemies/MeleeEnemy.prefab",
                "Assets/Prefabs/Enemies/RangedEnemy.prefab",
                "Assets/Prefabs/Enemies/BossEnemy.prefab",
                "Assets/Prefabs/UI/VerticalSliceHUD.prefab",
                "Assets/Prefabs/Projectiles/BasicProjectile.prefab",
                "Assets/Prefabs/Pickups/GoldPickup.prefab",
                "Assets/Prefabs/Pickups/HealthPotionPickup.prefab"
            };

            for (int i = 0; i < prefabPaths.Length; i++)
            {
                if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPaths[i]) == null)
                    failures.Add("Missing generated prefab: " + prefabPaths[i]);
            }

            if (AssetDatabase.FindAssets("t:Material", new[] { "Assets/Materials" }).Length < 10)
                failures.Add("Generated materials are missing or incomplete.");

            return failures;
        }

        private static List<string> GetGeneratedSceneValidationFailures()
        {
            var failures = new List<string>();
            if (!File.Exists(ScenePath))
            {
                failures.Add("Missing scene: " + ScenePath);
                return failures;
            }

            Scene previousScene = SceneManager.GetActiveScene();
            string previousPath = previousScene.path;
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            try
            {
                RequireSceneObjectExactlyOnce("GameManager", failures);
                RequireSceneObjectExactlyOnce("PlayerManager", failures);
                RequireSceneObjectExactlyOnce("RhythmSystem", failures);
                RequireSceneObjectExactlyOnce("ScoreSystem", failures);
                RequireSceneObjectExactlyOnce("TelemetryManager", failures);
                RequireSceneObjectExactlyOnce("EnemySpawner", failures);
                RequireSceneObjectExactlyOnce("ArenaController", failures);
                RequireSceneObjectExactlyOnce("DungeonDecor", failures);
                RequireSceneObjectExactlyOnce("Player_Warrior", failures);
                RequireSceneObjectExactlyOnce("VerticalSliceHUD", failures);

                var player = GameObject.Find("Player_Warrior");
                if (player != null)
                {
                    if (player.GetComponent<BaseCharacter>() == null)
                        failures.Add("Player_Warrior missing BaseCharacter.");
                    if (player.GetComponent<DodgeController>() == null)
                        failures.Add("Player_Warrior missing DodgeController.");
                    if (player.GetComponent<ComboSystem>() == null)
                        failures.Add("Player_Warrior missing ComboSystem.");
                }

                if (UnityEngine.Object.FindObjectsByType<RhythmJudgement>(FindObjectsInactive.Include).Length != 1)
                    failures.Add("Expected exactly one RhythmJudgement.");

                Component[] allComponents = UnityEngine.Object.FindObjectsByType<Component>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                for (int i = 0; i < allComponents.Length; i++)
                {
                    if (allComponents[i] == null)
                        failures.Add("Scene contains a missing script reference.");
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(previousPath) && File.Exists(previousPath) && previousPath != ScenePath)
                    EditorSceneManager.OpenScene(previousPath, OpenSceneMode.Single);
            }

            return failures;
        }

        private static void RequireSceneObjectExactlyOnce(string name, List<string> failures)
        {
            GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
            int count = 0;
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i].hideFlags == HideFlags.None && objects[i].scene.IsValid() && objects[i].name == name)
                    count++;
            }

            if (count != 1)
                failures.Add("Expected exactly one scene object named " + name + ", found " + count + ".");
        }

        private static Dictionary<string, string> CaptureGeneratedFileHashes()
        {
            var hashes = new Dictionary<string, string>(StringComparer.Ordinal);
            string[] roots =
            {
                ScenePath,
                "Assets/Prefabs",
                "Assets/Materials",
                "Assets/Art",
                "Assets/Generated",
                "Assets/Animations",
                "Assets/ScriptableObjects"
            };

            for (int i = 0; i < roots.Length; i++)
            {
                if (File.Exists(roots[i]))
                {
                    hashes[roots[i]] = ComputeNormalizedHash(roots[i]);
                    continue;
                }

                if (!Directory.Exists(roots[i]))
                    continue;

                string[] files = Directory.GetFiles(roots[i], "*", SearchOption.AllDirectories);
                Array.Sort(files, StringComparer.Ordinal);
                for (int j = 0; j < files.Length; j++)
                {
                    string path = files[j].Replace('\\', '/');
                    if (path.EndsWith(".meta", StringComparison.Ordinal) || path.Contains("/ThirdParty/", StringComparison.Ordinal))
                        continue;

                    hashes[path] = ComputeNormalizedHash(path);
                }
            }

            return hashes;
        }

        private static string ComputeNormalizedHash(string path)
        {
            string text = File.ReadAllText(path).Replace("\r\n", "\n").Replace('\r', '\n');
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            using (var sha = System.Security.Cryptography.SHA256.Create())
                return Convert.ToBase64String(sha.ComputeHash(bytes));
        }

        private static void NormalizeGeneratedMaterialYaml()
        {
            string[] roots =
            {
                "Assets/Materials",
                "Assets/Art/Materials"
            };

            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                if (!Directory.Exists(roots[rootIndex]))
                    continue;

                string[] files = Directory.GetFiles(roots[rootIndex], "*.mat", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                {
                    string text = File.ReadAllText(files[i]);
                    string normalized = StripTrailingWhitespace(text);
                    if (!string.Equals(text, normalized, StringComparison.Ordinal))
                        File.WriteAllText(files[i], normalized);
                }
            }
        }

        private static void NormalizeGeneratedSceneYaml()
        {
            if (!File.Exists(ScenePath))
                return;

            string text = File.ReadAllText(ScenePath);
            string normalized = StripTrailingWhitespace(text);
            if (!string.Equals(text, normalized, StringComparison.Ordinal))
                File.WriteAllText(ScenePath, normalized);
        }

        private static string StripTrailingWhitespace(string text)
        {
            text = text.Replace("\r\n", "\n").Replace('\r', '\n');
            string[] lines = text.Split('\n');
            var builder = new StringBuilder(text.Length);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].TrimEnd(' ', '\t');
                builder.Append(line);
                if (i < lines.Length - 1)
                    builder.Append('\n');
            }

            return builder.ToString();
        }

        private void OnGUI()
        {
            GUILayout.Label("Mobile Rhythm RPG Vertical Slice", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Creates default folders, assets, prefabs, a playable placeholder scene, and Build Settings wiring.", MessageType.Info);

            if (GUILayout.Button("Create Or Refresh Vertical Slice"))
                CreatePlayableVerticalSlice();
        }

        private static void EnsureProjectFolders()
        {
            string[] folders =
            {
                "Assets/Scenes",
                "Assets/Prefabs",
                "Assets/Prefabs/Player",
                "Assets/Prefabs/Enemies",
                "Assets/Prefabs/Projectiles",
                "Assets/Prefabs/Pickups",
                "Assets/Prefabs/UI",
                "Assets/Prefabs/Feedback",
                "Assets/Prefabs/Audio",
                "Assets/ScriptableObjects",
                "Assets/ScriptableObjects/Rhythm",
                "Assets/ScriptableObjects/Abilities",
                "Assets/ScriptableObjects/Combat",
                "Assets/ScriptableObjects/Boss",
                "Assets/ScriptableObjects/Items",
                "Assets/ScriptableObjects/Audio",
                "Assets/Materials",
                "Assets/Materials/Feedback",
                "Assets/Editor",
                "Assets/Animations",
                "Assets/Animations/Player",
                "Assets/Animations/Enemies",
                "Assets/Animations/Boss"
            };

            for (int i = 0; i < folders.Length; i++)
                EnsureFolder(folders[i]);
        }

        private static MaterialSet CreateMaterials()
        {
            return new MaterialSet
            {
                player = CreateMaterial("Assets/Materials/Player_Fighter.mat", new Color(0.2f, 0.55f, 1f)),
                mage = CreateMaterial("Assets/Materials/Player_Mage.mat", new Color(0.55f, 0.35f, 1f)),
                archer = CreateMaterial("Assets/Materials/Player_Archer.mat", new Color(0.25f, 0.75f, 0.35f)),
                healer = CreateMaterial("Assets/Materials/Player_Healer.mat", new Color(0.95f, 0.85f, 0.35f)),
                enemy = CreateMaterial("Assets/Materials/Enemy_Melee.mat", new Color(0.9f, 0.25f, 0.2f)),
                rangedEnemy = CreateMaterial("Assets/Materials/Enemy_Ranged.mat", new Color(0.95f, 0.45f, 0.2f)),
                boss = CreateMaterial("Assets/Materials/Enemy_Boss.mat", new Color(0.35f, 0.15f, 0.15f)),
                projectile = CreateMaterial("Assets/Materials/Projectile_Basic.mat", new Color(0.1f, 0.9f, 1f)),
                projectileTrail = CreateMaterial("Assets/Materials/Feedback/ProjectileTrail.mat", new Color(0.1f, 0.9f, 1f, 0.45f), "Assets/ThirdParty/FreeAssets/KenneyParticlePack/PNGTransparent/spark_07.png", true),
                ground = CreateMaterial("Assets/Materials/Ground_Prototype.mat", new Color(0.28f, 0.36f, 0.30f), "Assets/ThirdParty/FreeAssets/KenneyPrototypeTextures/PNG/Green/arena_grid_green.png"),
                wall = CreateMaterial("Assets/Materials/Wall_Prototype.mat", new Color(0.25f, 0.27f, 0.27f), "Assets/ThirdParty/FreeAssets/KenneyPrototypeTextures/PNG/Dark/arena_grid_dark.png"),
                perfect = CreateMaterial("Assets/Materials/Feedback/PerfectFeedback.mat", new Color(0.2f, 1f, 0.75f, 0.85f), "Assets/ThirdParty/FreeAssets/KenneyParticlePack/PNGTransparent/magic_05.png", true),
                good = CreateMaterial("Assets/Materials/Feedback/GoodFeedback.mat", new Color(0.35f, 0.65f, 1f, 0.75f), "Assets/ThirdParty/FreeAssets/KenneyParticlePack/PNGTransparent/circle_05.png", true),
                miss = CreateMaterial("Assets/Materials/Feedback/MissFeedback.mat", new Color(1f, 0.25f, 0.25f, 0.65f), "Assets/ThirdParty/FreeAssets/KenneyParticlePack/PNGTransparent/slash_04.png", true),
                dodge = CreateMaterial("Assets/Materials/Feedback/DodgeFeedback.mat", new Color(1f, 1f, 0.35f, 0.7f), "Assets/ThirdParty/FreeAssets/KenneyParticlePack/PNGTransparent/circle_05.png", true),
                parry = CreateMaterial("Assets/Materials/Feedback/ParryFeedback.mat", new Color(1f, 0.45f, 1f, 0.8f), "Assets/ThirdParty/FreeAssets/KenneyParticlePack/PNGTransparent/magic_05.png", true),
                parrySuccess = CreateMaterial("Assets/Materials/Feedback/ParrySuccess.mat", new Color(1f, 1f, 1f, 0.9f), "Assets/ThirdParty/FreeAssets/KenneyParticlePack/PNGTransparent/spark_07.png", true),
                enemyWindup = CreateMaterial("Assets/Materials/Feedback/EnemyWindup.mat", new Color(1f, 0.85f, 0.1f, 0.85f), "Assets/ThirdParty/FreeAssets/KenneyParticlePack/PNGTransparent/spark_07.png", true),
                bossWarning = CreateMaterial("Assets/Materials/Feedback/BossWarning.mat", new Color(1f, 0.65f, 0.1f, 0.55f), "Assets/ThirdParty/FreeAssets/KenneyPrototypeTextures/PNG/Red/danger_grid_red.png", true),
                bossImpact = CreateMaterial("Assets/Materials/Feedback/BossImpact.mat", new Color(1f, 0.1f, 0.05f, 0.8f), "Assets/ThirdParty/FreeAssets/KenneyParticlePack/PNGTransparent/slash_04.png", true)
            };
        }

        private static AudioCueSet CreateAudioCues()
        {
            return new AudioCueSet
            {
                perfect = CreateAudioCue("Assets/ScriptableObjects/Audio/PerfectHit.asset", "perfect_hit", 880f, 0.09f, 0.28f, 1.15f, AudioCueWaveform.Sine),
                good = CreateAudioCue("Assets/ScriptableObjects/Audio/GoodHit.asset", "good_hit", 660f, 0.07f, 0.22f, 1f, AudioCueWaveform.Sine),
                miss = CreateAudioCue("Assets/ScriptableObjects/Audio/Miss.asset", "miss", 180f, 0.08f, 0.16f, 0.85f, AudioCueWaveform.Square),
                dodge = CreateAudioCue("Assets/ScriptableObjects/Audio/Dodge.asset", "dodge", 520f, 0.06f, 0.18f, 1.25f, AudioCueWaveform.Sine),
                parry = CreateAudioCue("Assets/ScriptableObjects/Audio/ParrySuccess.asset", "parry_success", 980f, 0.08f, 0.26f, 1.35f, AudioCueWaveform.Square),
                bossWarning = CreateAudioCue("Assets/ScriptableObjects/Audio/BossWarning.asset", "boss_warning", 260f, 0.10f, 0.2f, 0.9f, AudioCueWaveform.Square),
                bossImpact = CreateAudioCue("Assets/ScriptableObjects/Audio/BossImpact.asset", "boss_impact", 110f, 0.14f, 0.32f, 0.75f, AudioCueWaveform.Square),
                playerDamage = CreateAudioCue("Assets/ScriptableObjects/Audio/PlayerDamage.asset", "player_damage", 150f, 0.10f, 0.20f, 0.8f, AudioCueWaveform.Noise),
                enemyDefeated = CreateAudioCue("Assets/ScriptableObjects/Audio/EnemyDefeated.asset", "enemy_defeated", 740f, 0.12f, 0.22f, 1.1f, AudioCueWaveform.Sine),
                footstep = CreateAudioCue("Assets/ScriptableObjects/Audio/Footstep.asset", "footstep", 220f, 0.04f, 0.10f, 0.8f, AudioCueWaveform.Noise),
                weaponSwing = CreateAudioCue("Assets/ScriptableObjects/Audio/WeaponSwing.asset", "weapon_swing", 420f, 0.06f, 0.14f, 1.1f, AudioCueWaveform.Noise)
            };
        }

        private static AudioCueDefinition CreateAudioCue(string path, string id, float frequency, float duration, float volume, float pitch, AudioCueWaveform waveform)
        {
            AudioCueDefinition cue = CreateOrLoadAsset<AudioCueDefinition>(path);
            cue.cueId = id;
            cue.frequency = frequency;
            cue.duration = duration;
            cue.volume = volume;
            cue.pitch = pitch;
            cue.waveform = waveform;
            EditorUtility.SetDirty(cue);
            return cue;
        }

        private static AnimationSet CreateAnimationAssets()
        {
            AnimationClip idle = CreateClip("Assets/Animations/Player/Idle.anim", true);
            AnimationClip move = CreateClip("Assets/Animations/Player/Move.anim", true);
            AnimationClip attack = CreateClip("Assets/Animations/Player/Attack.anim", false);
            AnimationClip dodge = CreateClip("Assets/Animations/Player/Dodge.anim", false);
            AnimationClip parry = CreateClip("Assets/Animations/Player/Parry.anim", false);
            AnimationClip hit = CreateClip("Assets/Animations/Player/Hit.anim", false);
            AnimationClip death = CreateClip("Assets/Animations/Player/Death.anim", false);

            AnimationClip enemyIdle = CreateClip("Assets/Animations/Enemies/Idle.anim", true);
            AnimationClip enemyMove = CreateClip("Assets/Animations/Enemies/Move.anim", true);
            AnimationClip enemyAttack = CreateClip("Assets/Animations/Enemies/Attack.anim", false);
            AnimationClip enemyHit = CreateClip("Assets/Animations/Enemies/Hit.anim", false);
            AnimationClip enemyDeath = CreateClip("Assets/Animations/Enemies/Death.anim", false);

            AnimationClip bossIdle = CreateClip("Assets/Animations/Boss/Idle.anim", true);
            AnimationClip bossAttack = CreateClip("Assets/Animations/Boss/Attack.anim", false);
            AnimationClip bossHit = CreateClip("Assets/Animations/Boss/Hit.anim", false);
            AnimationClip bossDeath = CreateClip("Assets/Animations/Boss/Death.anim", false);

            return new AnimationSet
            {
                playerController = CreateController("Assets/Animations/Player/PlayerPlaceholder.controller", idle, move, attack, dodge, parry, hit, death),
                enemyController = CreateController("Assets/Animations/Enemies/EnemyPlaceholder.controller", enemyIdle, enemyMove, enemyAttack, null, null, enemyHit, enemyDeath),
                bossController = CreateController("Assets/Animations/Boss/BossPlaceholder.controller", bossIdle, null, bossAttack, null, null, bossHit, bossDeath)
            };
        }

        private static AnimationClip CreateClip(string path, bool loop)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, path);
            }

            clip.frameRate = 30f;
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static RuntimeAnimatorController CreateController(string path, AnimationClip idle, AnimationClip move, AnimationClip attack, AnimationClip dodge, AnimationClip parry, AnimationClip hit, AnimationClip death)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null)
                controller = AnimatorController.CreateAnimatorControllerAtPath(path);

            EnsureAnimatorParameter(controller, "IsMoving", AnimatorControllerParameterType.Bool);
            EnsureAnimatorParameter(controller, "AttackState", AnimatorControllerParameterType.Int);
            EnsureAnimatorParameter(controller, "IsDodging", AnimatorControllerParameterType.Bool);
            EnsureAnimatorParameter(controller, "IsParrying", AnimatorControllerParameterType.Bool);
            EnsureAnimatorParameter(controller, "IsHit", AnimatorControllerParameterType.Bool);
            EnsureAnimatorParameter(controller, "IsDead", AnimatorControllerParameterType.Bool);
            EnsureAnimatorParameter(controller, "RhythmGrade", AnimatorControllerParameterType.Int);

            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            bool changed = false;
            changed |= EnsureState(machine, "Idle", idle, new Vector3(240f, 60f, 0f));
            changed |= EnsureState(machine, "Move", move != null ? move : idle, new Vector3(240f, 140f, 0f));
            changed |= EnsureState(machine, "Attack", attack != null ? attack : idle, new Vector3(240f, 220f, 0f));
            changed |= EnsureState(machine, "Dodge", dodge != null ? dodge : idle, new Vector3(240f, 300f, 0f));
            changed |= EnsureState(machine, "Parry", parry != null ? parry : idle, new Vector3(240f, 380f, 0f));
            changed |= EnsureState(machine, "Hit", hit != null ? hit : idle, new Vector3(520f, 180f, 0f));
            changed |= EnsureState(machine, "Death", death != null ? death : idle, new Vector3(520f, 300f, 0f));
            if (machine.defaultState == null)
            {
                AnimatorState idleState = FindState(machine, "Idle");
                if (idleState != null)
                {
                    machine.defaultState = idleState;
                    changed = true;
                }
            }

            if (changed)
                EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void EnsureAnimatorParameter(AnimatorController controller, string name, AnimatorControllerParameterType type)
        {
            for (int i = 0; i < controller.parameters.Length; i++)
            {
                if (controller.parameters[i].name == name)
                    return;
            }
            controller.AddParameter(name, type);
        }

        private static bool EnsureState(AnimatorStateMachine machine, string name, Motion motion, Vector3 position)
        {
            AnimatorState state = FindState(machine, name);
            bool changed = false;
            if (state == null)
            {
                state = machine.AddState(name, position);
                changed = true;
            }

            if (state.motion != motion)
            {
                state.motion = motion;
                changed = true;
            }

            ChildAnimatorState[] states = machine.states;
            for (int i = 0; i < states.Length; i++)
            {
                if (states[i].state != state)
                    continue;

                if (states[i].position != position)
                {
                    states[i].position = position;
                    machine.states = states;
                    changed = true;
                }

                break;
            }

            if (machine.defaultState == null)
            {
                machine.defaultState = state;
                changed = true;
            }

            return changed;
        }

        private static AnimatorState FindState(AnimatorStateMachine machine, string name)
        {
            ChildAnimatorState[] states = machine.states;
            for (int i = 0; i < states.Length; i++)
            {
                if (states[i].state != null && states[i].state.name == name)
                    return states[i].state;
            }

            return null;
        }

        private static RhythmConfig CreateRhythmConfig()
        {
            RhythmConfig config = CreateOrLoadAsset<RhythmConfig>("Assets/ScriptableObjects/Rhythm/DefaultRhythmConfig.asset");
            config.bpm = 120f;
            config.dspOffsetSeconds = 0d;
            config.perfectWindow = 0.05f;
            config.goodWindow = 0.11f;
            config.earlyInputBiasSeconds = 0.018f;
            config.lateInputBiasSeconds = 0.007f;
            config.perfectDamageMultiplier = 1.5f;
            config.goodDamageMultiplier = 1.15f;
            config.missDamageMultiplier = 0.75f;
            config.speedIncreasePerLevel = 0.025f;
            config.maxSpeedMultiplier = 1.5f;
            config.perfectScore = 100;
            config.goodScore = 50;
            config.missScore = 10;
            config.comboScoreBonus = 5;
            config.perfectCooldownRefund = 0.2f;
            config.goodCooldownRefund = 0.1f;
            EditorUtility.SetDirty(config);
            return config;
        }

        private static ComboProfile CreateComboProfile()
        {
            ComboProfile profile = CreateOrLoadAsset<ComboProfile>("Assets/ScriptableObjects/Combat/DefaultComboProfile.asset");
            profile.steps = new[]
            {
                new ComboProfile.Step { baseDamage = 8, cooldown = 0.15f, tag = "Step 1" },
                new ComboProfile.Step { baseDamage = 11, cooldown = 0.2f, tag = "Step 2" },
                new ComboProfile.Step { baseDamage = 16, cooldown = 0.35f, tag = "Finisher" }
            };
            profile.comboTimeout = 1.5f;
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static AttackTimingData CreateAttackTiming()
        {
            AttackTimingData timing = CreateOrLoadAsset<AttackTimingData>("Assets/ScriptableObjects/Combat/DefaultAttackTiming.asset");
            timing.windupSeconds = 0.08f;
            timing.activeSeconds = 0.10f;
            timing.recoverySeconds = 0.18f;
            timing.canCancelOnPerfect = true;
            timing.beatAlignedImpact = true;
            timing.hitFrameBeatOffset = 0f;
            EditorUtility.SetDirty(timing);
            return timing;
        }

        private static AbilityDefinition CreateAbility(string path, string id, string displayName, string description, int damage, int heal, float cooldown, float range, float radius, AbilityType type, AbilityTargetMode targetMode, AbilityExecutionStyle executionStyle, AbilityRhythmScaling scaling, float perfectStagger, float perfectProtection)
        {
            AbilityDefinition ability = CreateOrLoadAsset<AbilityDefinition>(path);
            ability.abilityId = id;
            ability.displayName = displayName;
            ability.description = description;
            ability.baseDamage = damage;
            ability.healAmount = heal;
            ability.cooldown = cooldown;
            ability.range = range;
            ability.radius = radius;
            ability.resourceCost = 0;
            ability.abilityType = type;
            ability.targetMode = targetMode;
            ability.executionStyle = executionStyle;
            ability.perfectStaggerSeconds = perfectStagger;
            ability.perfectProtectionSeconds = perfectProtection;
            ability.rhythmScaling = scaling;
            EditorUtility.SetDirty(ability);
            return ability;
        }

        private static BossTelegraphData CreateBossTelegraph(string path, string id, string displayName, BossAttackType attackType, TelegraphShape shape, int beatsBeforeImpact, int repeatCount, int beatsBetweenRepeats, int damage, float radius, float range, float width, float length, GameObject warningPrefab, GameObject impactPrefab)
        {
            BossTelegraphData telegraph = CreateOrLoadAsset<BossTelegraphData>(path);
            telegraph.telegraphId = id;
            telegraph.displayName = displayName;
            telegraph.attackType = attackType;
            telegraph.shape = shape;
            telegraph.beatsBeforeImpact = beatsBeforeImpact;
            telegraph.repeatCount = repeatCount;
            telegraph.beatsBetweenRepeats = beatsBetweenRepeats;
            telegraph.damage = damage;
            telegraph.radius = radius;
            telegraph.range = range;
            telegraph.width = width;
            telegraph.length = length;
            telegraph.warningVfxPrefab = warningPrefab;
            telegraph.impactVfxPrefab = impactPrefab;
            EditorUtility.SetDirty(telegraph);
            return telegraph;
        }

        private static BossPhaseData CreateBossPhase(string path, string displayName, float threshold, bool rangedMode, int burstCount, float burstInterval, float moveSpeedMultiplier, float attackCooldownMultiplier, float telegraphCooldownMultiplier, float telegraphWarningScale, BossTelegraphData[] telegraphs)
        {
            BossPhaseData phase = CreateOrLoadAsset<BossPhaseData>(path);
            phase.displayName = displayName;
            phase.healthThreshold = threshold;
            phase.rangedMode = rangedMode;
            phase.burstCount = burstCount;
            phase.burstInterval = burstInterval;
            phase.moveSpeedMultiplier = moveSpeedMultiplier;
            phase.attackCooldownMultiplier = attackCooldownMultiplier;
            phase.telegraphCooldownMultiplier = telegraphCooldownMultiplier;
            phase.telegraphWarningScale = telegraphWarningScale;
            phase.telegraphs = telegraphs;
            EditorUtility.SetDirty(phase);
            return phase;
        }

        private static ItemDefinition CreateItem(string path, string id, string displayName, int maxStack, int goldValue = 0, int healAmount = 0)
        {
            ItemDefinition item = CreateOrLoadAsset<ItemDefinition>(path);
            item.itemId = id;
            item.displayName = displayName;
            item.maxStack = maxStack;
            item.goldValue = goldValue;
            item.healAmount = healAmount;
            EditorUtility.SetDirty(item);
            return item;
        }

        private static GameObject CreatePickupPrefab(string path, ItemDefinition item, Material material, PrimitiveType primitive, float scale)
        {
            GameObject pickup = GameObject.CreatePrimitive(primitive);
            pickup.name = System.IO.Path.GetFileNameWithoutExtension(path);
            pickup.transform.localScale = Vector3.one * scale;
            AssignMaterial(pickup, material);
            Collider collider = pickup.GetComponent<Collider>();
            if (collider != null)
                collider.isTrigger = true;

            var pickupComponent = pickup.AddComponent<Pickup>();
            SetObject(pickupComponent, "item", item);
            SetInt(pickupComponent, "amount", item != null && item.itemId == "gold" ? 2 : 1);
            return SavePrefab(path, pickup);
        }

        private static GameObject CreatePlayerPrefab(string path, System.Type classType, AbilityDefinition ability, RhythmConfig rhythmConfig, ComboProfile comboProfile, AttackTimingData attackTiming, GameObject projectilePrefab, GameObject projectileImpactPrefab, Material material, AudioCueSet audioCues, RuntimeAnimatorController animatorController)
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = System.IO.Path.GetFileNameWithoutExtension(path);
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 1f, 0f);
            Object.DestroyImmediate(player.GetComponent<Collider>());
            AssignMaterial(player, material);

            player.AddComponent<CharacterController>();
            player.AddComponent(classType);
            player.AddComponent<InventorySystem>();
            player.AddComponent<Game.Feedback.HitReactionController>();
            player.AddComponent<HitPause>();
            var animator = player.AddComponent<Animator>();
            animator.runtimeAnimatorController = animatorController;
            player.AddComponent<AudioSource>();
            var cuePlayer = player.AddComponent<AudioCuePlayer>();
            var animationBridge = player.AddComponent<Game.Animation.CombatAnimationBridge>();
            player.AddComponent<Game.Animation.AnimationEventRelay>();
            SetObject(animationBridge, "audioCuePlayer", cuePlayer);
            SetObject(animationBridge, "footstepCue", audioCues.footstep);
            SetObject(animationBridge, "weaponSwingCue", audioCues.weaponSwing);
            var playerController = player.AddComponent<PlayerController>();
            float moveSpeed = classType == typeof(Archer) ? 6.1f : classType == typeof(Mage) ? 5.35f : classType == typeof(Healer) ? 5.45f : 5.8f;
            SetFloat(playerController, "moveSpeed", moveSpeed);
            SetFloat(playerController, "jumpHeight", 1.4f);
            var attackBuffer = player.AddComponent<InputBuffer>();
            var abilityBuffer = player.AddComponent<InputBuffer>();
            var dodgeBuffer = player.AddComponent<InputBuffer>();
            var parryBuffer = player.AddComponent<InputBuffer>();
            SetObject(playerController, "attackBuffer", attackBuffer);

            var combo = player.AddComponent<ComboSystem>();
            SetObject(combo, "profile", comboProfile);
            SetObject(combo, "attackTiming", attackTiming);
            SetFloat(combo, "attackRange", classType == typeof(Fighter) ? 2.35f : 2.05f);

            var abilityController = player.AddComponent<AbilityController>();
            SetObject(abilityController, "abilityBuffer", abilityBuffer);
            SetObject(abilityController, "abilities", new[] { ability });
            SetObject(abilityController, "projectilePrefab", projectilePrefab);
            SetObject(abilityController, "projectileImpactPrefab", projectileImpactPrefab);

            GameObject abilityPoolObject = new GameObject("AbilityProjectilePool");
            abilityPoolObject.transform.SetParent(player.transform);
            var abilityPool = abilityPoolObject.AddComponent<ObjectPool>();
            SetObject(abilityPool, "prefab", projectilePrefab);
            SetObject(abilityController, "projectilePool", abilityPool);
            SetFloat(abilityController, "mageProjectileSpeed", 9f);
            SetFloat(abilityController, "archerProjectileSpeed", 17.5f);

            var dodge = player.AddComponent<DodgeController>();
            SetObject(dodge, "dodgeBuffer", dodgeBuffer);
            SetFloat(dodge, "distance", 3.6f);
            SetFloat(dodge, "cooldown", 0.7f);
            SetFloat(dodge, "perfectSpeedMultiplier", 1.28f);
            SetFloat(dodge, "goodSpeedMultiplier", 1f);
            SetFloat(dodge, "missSpeedMultiplier", 0.9f);
            SetFloat(dodge, "perfectCooldownMultiplier", 0.6f);
            SetFloat(dodge, "goodCooldownMultiplier", 0.9f);
            SetFloat(dodge, "missCooldownMultiplier", 1f);
            SetFloat(dodge, "perfectInvulnerability", 0.38f);
            SetFloat(dodge, "perfectInvulnerabilityBonus", 0.06f);

            var parry = player.AddComponent<ParryController>();
            SetObject(parry, "parryBuffer", parryBuffer);
            SetFloat(parry, "cooldown", 0.75f);

            var simulation = player.AddComponent<PlayerSimulationController>();
            SetFloat(simulation, "preferredRange", classType == typeof(Fighter) ? 2.25f : 2.4f);
            SetFloat(simulation, "rangedPreferredRange", classType == typeof(Archer) ? 8f : 6.2f);

            return SavePrefab(path, player);
        }

        private static GameObject CreateMeleeEnemyPrefab(Material material, GameObject windupPrefab, RuntimeAnimatorController animatorController)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "MeleeEnemy";
            Object.DestroyImmediate(enemy.GetComponent<Collider>());
            AssignMaterial(enemy, material);
            var animator = enemy.AddComponent<Animator>();
            animator.runtimeAnimatorController = animatorController;
            enemy.AddComponent<CharacterController>();
            var melee = enemy.AddComponent<MeleeEnemy>();
            enemy.AddComponent<Game.Feedback.HitReactionController>();
            enemy.AddComponent<HitPause>();
            var flash = enemy.AddComponent<EnemyAttackFlash>();
            SetObject(flash, "warningPrefab", windupPrefab);
            SetInt(melee, "maxHealth", 24);
            SetInt(melee, "health", 24);
            SetInt(melee, "attackDamage", 3);
            SetFloat(melee, "moveSpeed", 1.9f);
            SetFloat(melee, "attackInterval", 2.25f);
            SetFloat(melee, "attackRange", 1.65f);
            SetFloat(melee, "chaseRange", 9f);
            return SavePrefab("Assets/Prefabs/Enemies/MeleeEnemy.prefab", enemy);
        }

        private static GameObject CreateRangedEnemyPrefab(GameObject projectilePrefab, Material material, GameObject windupPrefab, RuntimeAnimatorController animatorController)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "RangedEnemy";
            Object.DestroyImmediate(enemy.GetComponent<Collider>());
            AssignMaterial(enemy, material);
            var animator = enemy.AddComponent<Animator>();
            animator.runtimeAnimatorController = animatorController;
            enemy.AddComponent<CharacterController>();
            var ranged = enemy.AddComponent<RangedEnemy>();
            enemy.AddComponent<Game.Feedback.HitReactionController>();
            enemy.AddComponent<HitPause>();
            var flash = enemy.AddComponent<EnemyAttackFlash>();
            SetObject(flash, "warningPrefab", windupPrefab);
            SetInt(ranged, "maxHealth", 20);
            SetInt(ranged, "health", 20);
            SetInt(ranged, "attackDamage", 2);
            SetFloat(ranged, "moveSpeed", 1.5f);
            SetFloat(ranged, "attackInterval", 2.8f);
            SetFloat(ranged, "attackRange", 8.5f);
            SetFloat(ranged, "chaseRange", 12f);
            SetFloat(ranged, "projectileSpeed", 6.5f);

            GameObject poolObject = new GameObject("ProjectilePool");
            poolObject.transform.SetParent(enemy.transform);
            var pool = poolObject.AddComponent<ObjectPool>();
            SetObject(pool, "prefab", projectilePrefab);
            SetObject(ranged, "projectilePrefab", projectilePrefab);
            SetObject(ranged, "projectilePool", pool);

            return SavePrefab("Assets/Prefabs/Enemies/RangedEnemy.prefab", enemy);
        }

        private static GameObject CreateBossPrefab(GameObject projectilePrefab, BossTelegraphData telegraphData, BossPhaseData[] phases, Material material, GameObject windupPrefab, AudioCueSet audioCues, RuntimeAnimatorController animatorController)
        {
            GameObject boss = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            boss.name = "BossEnemy";
            boss.transform.localScale = new Vector3(1.8f, 1.8f, 1.8f);
            Object.DestroyImmediate(boss.GetComponent<Collider>());
            AssignMaterial(boss, material);
            var animator = boss.AddComponent<Animator>();
            animator.runtimeAnimatorController = animatorController;
            boss.AddComponent<CharacterController>();
            boss.AddComponent<Game.Feedback.HitReactionController>();
            boss.AddComponent<HitPause>();
            boss.AddComponent<AudioSource>();
            var cuePlayer = boss.AddComponent<AudioCuePlayer>();
            var bossEnemy = boss.AddComponent<BossEnemy>();
            var flash = boss.AddComponent<EnemyAttackFlash>();
            SetObject(flash, "warningPrefab", windupPrefab);
            SetInt(bossEnemy, "maxHealth", 90);
            SetInt(bossEnemy, "health", 90);
            SetInt(bossEnemy, "attackDamage", 4);
            SetFloat(bossEnemy, "moveSpeed", 1.4f);
            SetFloat(bossEnemy, "attackInterval", 3.2f);
            SetFloat(bossEnemy, "attackRange", 15f);
            SetFloat(bossEnemy, "chaseRange", 18f);
            SetFloat(bossEnemy, "specialAttackCooldown", 8f);
            var phaseController = boss.AddComponent<BossPhaseController>();
            SetObject(phaseController, "phases", phases);
            var telegraph = boss.AddComponent<BossTelegraphController>();
            SetObject(telegraph, "audioCuePlayer", cuePlayer);
            SetObject(telegraph, "warningAudioCue", audioCues.bossWarning);
            SetObject(telegraph, "impactAudioCue", audioCues.bossImpact);
            SetFloat(telegraph, "telegraphRepeatCooldown", 4.5f);
            SetFloat(telegraph, "warningPulseScale", 1.25f);
            SetFloat(telegraph, "impactDuration", 0.5f);

            GameObject poolObject = new GameObject("ProjectilePool");
            poolObject.transform.SetParent(boss.transform);
            var pool = poolObject.AddComponent<ObjectPool>();
            SetObject(pool, "prefab", projectilePrefab);
            SetObject(bossEnemy, "projectilePrefab", projectilePrefab);
            SetObject(bossEnemy, "projectilePool", pool);
            SetObject(bossEnemy, "telegraphController", telegraph);
            SetObject(bossEnemy, "specialTelegraph", telegraphData);
            SetObject(telegraph, "defaultTelegraph", telegraphData);

            return SavePrefab("Assets/Prefabs/Enemies/BossEnemy.prefab", boss);
        }

        private static GameObject CreateProjectilePrefab(Material material, Material trailMaterial)
        {
            GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.name = "BasicProjectile";
            projectile.transform.localScale = Vector3.one * 0.35f;
            AssignMaterial(projectile, material);
            var collider = projectile.GetComponent<SphereCollider>();
            collider.isTrigger = true;
            var body = projectile.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.isKinematic = true;
            var trail = projectile.AddComponent<TrailRenderer>();
            trail.time = 0.22f;
            trail.startWidth = 0.18f;
            trail.endWidth = 0.02f;
            trail.material = trailMaterial;
            projectile.AddComponent<PoolableProjectile>();
            return SavePrefab("Assets/Prefabs/Projectiles/BasicProjectile.prefab", projectile);
        }

        private static FeedbackPrefabs CreateFeedbackPrefabs(MaterialSet materials)
        {
            return new FeedbackPrefabs
            {
                perfect = CreateFeedbackPrefab("Assets/Prefabs/Feedback/PerfectHitPulse.prefab", PrimitiveType.Sphere, materials.perfect, 0.38f, 2.7f, false),
                good = CreateFeedbackPrefab("Assets/Prefabs/Feedback/GoodHitPulse.prefab", PrimitiveType.Sphere, materials.good, 0.30f, 2.0f, false),
                miss = CreateFeedbackPrefab("Assets/Prefabs/Feedback/MissHitPulse.prefab", PrimitiveType.Sphere, materials.miss, 0.22f, 1.35f, false),
                dodge = CreateFeedbackPrefab("Assets/Prefabs/Feedback/DodgePulse.prefab", PrimitiveType.Cylinder, materials.dodge, 0.32f, 2.7f, true),
                parry = CreateFeedbackPrefab("Assets/Prefabs/Feedback/ParryPulse.prefab", PrimitiveType.Sphere, materials.parry, 0.28f, 1.8f, false),
                parrySuccess = CreateFeedbackPrefab("Assets/Prefabs/Feedback/ParrySuccessBurst.prefab", PrimitiveType.Sphere, materials.parrySuccess, 0.42f, 2.8f, false),
                bossWarning = CreateFeedbackPrefab("Assets/Prefabs/Feedback/BossTelegraphWarningRing.prefab", PrimitiveType.Cylinder, materials.bossWarning, 0.55f, 1.35f, true),
                bossImpact = CreateFeedbackPrefab("Assets/Prefabs/Feedback/BossImpactBurst.prefab", PrimitiveType.Cylinder, materials.bossImpact, 0.52f, 2.1f, true),
                enemyWindup = CreateFeedbackPrefab("Assets/Prefabs/Feedback/EnemyWindupFlash.prefab", PrimitiveType.Sphere, materials.enemyWindup, 0.24f, 1.6f, false),
                projectileTrail = CreateFeedbackPrefab("Assets/Prefabs/Feedback/ProjectileTrailPlaceholder.prefab", PrimitiveType.Cylinder, materials.projectileTrail, 0.22f, 1.2f, true)
            };
        }

        private static GameObject CreateAudioCuePlayerPrefab()
        {
            GameObject audio = new GameObject("AudioCuePlayer");
            audio.AddComponent<AudioSource>();
            audio.AddComponent<AudioCuePlayer>();
            return SavePrefab("Assets/Prefabs/Audio/AudioCuePlayer.prefab", audio);
        }

        private static GameObject CreateFeedbackPrefab(string path, PrimitiveType primitive, Material material, float lifetime, float expansion, bool flatten)
        {
            GameObject feedback = GameObject.CreatePrimitive(primitive);
            feedback.name = System.IO.Path.GetFileNameWithoutExtension(path);
            AssignMaterial(feedback, material);
            Collider collider = feedback.GetComponent<Collider>();
            if (collider != null)
                Object.DestroyImmediate(collider);

            if (flatten)
                feedback.transform.localScale = new Vector3(1f, 0.04f, 1f);

            var pulse = feedback.AddComponent<FeedbackPulse>();
            SetFloat(pulse, "lifetime", lifetime);
            SetFloat(pulse, "expansion", expansion);
            SetBool(pulse, "flattenToRing", flatten);

            return SavePrefab(path, feedback);
        }

        private static GameObject CreateHudPrefab()
        {
            GameObject root = new GameObject("VerticalSliceHUD");
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            root.AddComponent<GraphicRaycaster>();

            GameObject panel = new GameObject("HUDPanel");
            panel.transform.SetParent(root.transform, false);
            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0f, -12f);
            panelRect.sizeDelta = new Vector2(-24f, 176f);

            GameObject beatBar = new GameObject("BeatBar");
            beatBar.transform.SetParent(panel.transform, false);
            var beatBarRect = beatBar.AddComponent<RectTransform>();
            beatBarRect.anchorMin = new Vector2(0.5f, 1f);
            beatBarRect.anchorMax = new Vector2(0.5f, 1f);
            beatBarRect.pivot = new Vector2(0.5f, 1f);
            beatBarRect.anchoredPosition = new Vector2(0f, -6f);
            beatBarRect.sizeDelta = new Vector2(320f, 18f);
            beatBar.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

            GameObject perfectWindow = CreateUiBox("PerfectWindow", beatBar.transform, new Color(0.2f, 1f, 0.8f, 0.55f));
            var perfectRect = perfectWindow.GetComponent<RectTransform>();
            perfectRect.anchorMin = new Vector2(0.45f, 0f);
            perfectRect.anchorMax = new Vector2(0.55f, 1f);
            perfectRect.offsetMin = Vector2.zero;
            perfectRect.offsetMax = Vector2.zero;

            GameObject marker = CreateUiBox("BeatMarker", beatBar.transform, new Color(1f, 1f, 1f, 1f));
            var markerRect = marker.GetComponent<RectTransform>();
            markerRect.anchorMin = new Vector2(0.5f, 0.5f);
            markerRect.anchorMax = new Vector2(0.5f, 0.5f);
            markerRect.sizeDelta = new Vector2(8f, 26f);

            Text feedback = CreateText("FeedbackText", panel.transform, new Vector2(0f, -32f), new Vector2(380f, 26f), 20, TextAnchor.MiddleCenter);
            Text combo = CreateText("ComboText", panel.transform, new Vector2(-280f, -66f), new Vector2(220f, 22f), 14, TextAnchor.MiddleLeft);
            Text ability = CreateText("AbilityText", panel.transform, new Vector2(-280f, -92f), new Vector2(300f, 22f), 14, TextAnchor.MiddleLeft);
            Text dodge = CreateText("DodgeText", panel.transform, new Vector2(-10f, -66f), new Vector2(190f, 22f), 14, TextAnchor.MiddleLeft);
            Text parry = CreateText("ParryText", panel.transform, new Vector2(-10f, -92f), new Vector2(190f, 22f), 14, TextAnchor.MiddleLeft);
            Text attack = CreateText("AttackStateText", panel.transform, new Vector2(220f, -66f), new Vector2(220f, 22f), 14, TextAnchor.MiddleLeft);
            Text score = CreateText("ScoreText", panel.transform, new Vector2(220f, -92f), new Vector2(220f, 22f), 14, TextAnchor.MiddleLeft);
            Text inventory = CreateText("InventoryText", panel.transform, new Vector2(-280f, -116f), new Vector2(300f, 22f), 14, TextAnchor.MiddleLeft);
            Text speed = CreateText("BeatSpeedText", panel.transform, new Vector2(238f, -8f), new Vector2(120f, 18f), 11, TextAnchor.MiddleLeft);
            Text boss = CreateText("BossText", panel.transform, new Vector2(0f, -116f), new Vector2(500f, 22f), 14, TextAnchor.MiddleCenter);
            Text arena = CreateText("ArenaText", panel.transform, new Vector2(0f, -138f), new Vector2(520f, 22f), 14, TextAnchor.MiddleCenter);
            Text debug = CreateText("DebugText", root.transform, new Vector2(12f, 12f), new Vector2(230f, 100f), 12, TextAnchor.LowerLeft);
            var debugRect = debug.GetComponent<RectTransform>();
            debugRect.anchorMin = new Vector2(0f, 0f);
            debugRect.anchorMax = new Vector2(0f, 0f);
            debugRect.pivot = new Vector2(0f, 0f);

            var beatBarUi = root.AddComponent<Game.UI.BeatBarUI>();
            SetObject(beatBarUi, "marker", markerRect);
            SetObject(beatBarUi, "bar", beatBarRect);
            SetObject(beatBarUi, "perfectWindow", perfectRect);
            SetObject(beatBarUi, "feedbackText", feedback);
            SetObject(beatBarUi, "speedText", speed);

            var hud = root.AddComponent<Game.UI.VerticalSliceHud>();
            SetObject(hud, "feedbackText", feedback);
            SetObject(hud, "comboText", combo);
            SetObject(hud, "abilityText", ability);
            SetObject(hud, "dodgeText", dodge);
            SetObject(hud, "parryText", parry);
            SetObject(hud, "attackStateText", attack);
            SetObject(hud, "scoreText", score);
            SetObject(hud, "inventoryText", inventory);
            SetObject(hud, "bossText", boss);
            SetObject(hud, "arenaText", arena);
            SetObject(hud, "debugText", debug);

            return SavePrefab("Assets/Prefabs/UI/VerticalSliceHUD.prefab", root);
        }

        private static GameObject CreateUiBox(string name, Transform parent, Color color)
        {
            GameObject box = new GameObject(name);
            box.transform.SetParent(parent, false);
            box.AddComponent<RectTransform>();
            box.AddComponent<Image>().color = color;
            return box;
        }

        private static Text CreateText(string name, Transform parent, Vector2 position, Vector2 size, int fontSize, TextAnchor anchor)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            var rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Text text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = Color.white;
            text.raycastTarget = false;
            text.text = name;
            return text;
        }

        private static void BuildScene(GameObject fighterPrefab, GameObject magePrefab, GameObject archerPrefab, GameObject healerPrefab, GameObject meleePrefab, GameObject rangedPrefab, GameObject bossPrefab, GameObject hudPrefab, RhythmConfig rhythmConfig, BossTelegraphData bossTelegraph, GameObject goldPickupPrefab, GameObject potionPickupPrefab, MaterialSet materials, FeedbackPrefabs feedbackPrefabs, AudioCueSet audioCues, GameObject audioPrefab)
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Arena_Ground";
            ground.transform.localScale = new Vector3(4f, 1f, 4f);
            AssignMaterial(ground, materials.ground);
            CreateBoundary("Arena_Wall_North", new Vector3(0f, 1f, 20f), new Vector3(40f, 2f, 0.5f), materials.wall);
            CreateBoundary("Arena_Wall_South", new Vector3(0f, 1f, -20f), new Vector3(40f, 2f, 0.5f), materials.wall);
            CreateBoundary("Arena_Wall_East", new Vector3(20f, 1f, 0f), new Vector3(0.5f, 2f, 40f), materials.wall);
            CreateBoundary("Arena_Wall_West", new Vector3(-20f, 1f, 0f), new Vector3(0.5f, 2f, 40f), materials.wall);
            CreateDungeonDecor();

            GameObject playerManagerObject = new GameObject("PlayerManager");
            playerManagerObject.AddComponent<PlayerManager>();

            GameObject gameManagerObject = new GameObject("GameManager");
            var gameManager = gameManagerObject.AddComponent<GameManager>();
            gameManager.warriorPrefab = fighterPrefab;
            gameManager.fighterPrefab = fighterPrefab;
            gameManager.magePrefab = null;
            gameManager.archerPrefab = null;
            gameManager.healerPrefab = null;

            GameObject classSwapObject = new GameObject("ClassSwapDebugController");
            classSwapObject.SetActive(false);
            var classSwap = classSwapObject.AddComponent<ClassSwapDebugController>();
            SetObject(classSwap, "fighterPrefab", fighterPrefab);
            SetObject(classSwap, "magePrefab", magePrefab);
            SetObject(classSwap, "archerPrefab", archerPrefab);
            SetObject(classSwap, "healerPrefab", healerPrefab);

            GameObject rhythm = new GameObject("RhythmSystem");
            var beatClock = rhythm.AddComponent<BeatClock>();
            var rhythmJudgement = rhythm.AddComponent<RhythmJudgement>();
            SetObject(beatClock, "config", rhythmConfig);
            SetObject(rhythmJudgement, "config", rhythmConfig);

            GameObject scoreObject = new GameObject("ScoreSystem");
            var scoreSystem = scoreObject.AddComponent<ScoreSystem>();
            SetObject(scoreSystem, "rhythmConfig", rhythmConfig);

            GameObject telemetryObject = new GameObject("TelemetryManager");
            telemetryObject.AddComponent<TelemetryManager>();

            GameObject pickupSpawnerObject = new GameObject("PickupSpawner");
            var pickupSpawner = pickupSpawnerObject.AddComponent<PickupSpawner>();
            SetObject(pickupSpawner, "goldPickupPrefab", goldPickupPrefab);
            SetObject(pickupSpawner, "healthPotionPickupPrefab", potionPickupPrefab);

            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(fighterPrefab);
            player.name = "Player_Warrior";
            player.transform.position = new Vector3(0f, 1f, -8f);

            GameObject melee = (GameObject)PrefabUtility.InstantiatePrefab(meleePrefab);
            melee.name = "MeleeEnemy";
            melee.transform.position = new Vector3(3f, 1f, -1f);

            GameObject ranged = (GameObject)PrefabUtility.InstantiatePrefab(rangedPrefab);
            ranged.name = "RangedEnemy";
            ranged.transform.position = new Vector3(-6f, 1f, 3f);

            GameObject boss = (GameObject)PrefabUtility.InstantiatePrefab(bossPrefab);
            boss.name = "BossEnemy";
            boss.transform.position = new Vector3(0f, 1.8f, 10f);
            var telegraph = boss.GetComponent<BossTelegraphController>();
            if (telegraph != null)
                SetObject(telegraph, "defaultTelegraph", bossTelegraph);

            GameObject spawnerObject = new GameObject("EnemySpawner");
            var spawner = spawnerObject.AddComponent<EnemySpawner>();
            spawner.enemyPrefabs = new[] { meleePrefab, rangedPrefab };
            spawner.spawnInterval = 18f;
            spawner.maxEnemies = 2;
            Transform[] spawnPoints = new[]
            {
                CreateSpawnPoint("EnemySpawn_A", new Vector3(7f, 0f, 4f)),
                CreateSpawnPoint("EnemySpawn_B", new Vector3(-7f, 0f, 4f))
            };
            spawner.spawnPoints = spawnPoints;
            spawner.enabled = false;

            GameObject arenaObject = new GameObject("ArenaController");
            arenaObject.AddComponent<DifficultyScaler>();
            var arena = arenaObject.AddComponent<ArenaController>();
            SetObject(arena, "player", player.GetComponent<BaseCharacter>());
            SetObject(arena, "bossObject", boss);
            SetObject(arena, "initialWaveEnemies", new[] { melee.GetComponent<BaseEnemy>(), ranged.GetComponent<BaseEnemy>() });
            SetObject(arena, "waveEnemyPrefabs", new[] { meleePrefab, rangedPrefab });
            SetObject(arena, "waveSpawnPoints", spawnPoints);
            SetObject(arena, "scoreSystem", scoreSystem);
            SetObject(arena, "pickupSpawner", pickupSpawner);
            SetInt(arena, "waveCount", 1);
            SetInt(arena, "enemiesPerWave", 2);
            SetFloat(arena, "spawnPacingSeconds", 0.75f);

            GameObject hud = (GameObject)PrefabUtility.InstantiatePrefab(hudPrefab);
            hud.name = "VerticalSliceHUD";
            hud.AddComponent<TelemetryDebugOverlay>();
            var hudController = hud.GetComponentInChildren<Game.UI.VerticalSliceHud>();
            if (hudController != null)
            {
                SetObject(hudController, "comboSystem", player.GetComponent<ComboSystem>());
                SetObject(hudController, "abilityController", player.GetComponent<AbilityController>());
                SetObject(hudController, "dodgeController", player.GetComponent<DodgeController>());
                SetObject(hudController, "parryController", player.GetComponent<ParryController>());
                SetObject(hudController, "bossTelegraphController", telegraph);
                SetObject(hudController, "bossPhaseController", boss.GetComponent<BossPhaseController>());
                SetObject(hudController, "playerCharacter", player.GetComponent<BaseCharacter>());
                SetObject(hudController, "arenaController", arena);
                SetObject(hudController, "scoreSystem", scoreSystem);
                SetObject(hudController, "inventorySystem", player.GetComponent<InventorySystem>());
            }

            GameObject feedbackObject = new GameObject("RhythmFeedback");
            feedbackObject.AddComponent<AudioSource>();
            var cuePlayer = feedbackObject.AddComponent<AudioCuePlayer>();
            var feedback = feedbackObject.AddComponent<RhythmFeedbackController>();
            SetObject(feedback, "comboSystem", player.GetComponent<ComboSystem>());
            SetObject(feedback, "abilityController", player.GetComponent<AbilityController>());
            SetObject(feedback, "dodgeController", player.GetComponent<DodgeController>());
            SetObject(feedback, "parryController", player.GetComponent<ParryController>());
            SetObject(feedback, "bossTelegraphController", telegraph);
            SetObject(feedback, "playerCharacter", player.GetComponent<BaseCharacter>());
            SetObject(feedback, "playerAnchor", player.transform);
            SetObject(feedback, "cuePlayer", cuePlayer);
            SetObject(feedback, "perfectAttackPrefab", feedbackPrefabs.perfect);
            SetObject(feedback, "goodAttackPrefab", feedbackPrefabs.good);
            SetObject(feedback, "missAttackPrefab", feedbackPrefabs.miss);
            SetObject(feedback, "dodgePrefab", feedbackPrefabs.dodge);
            SetObject(feedback, "parryPrefab", feedbackPrefabs.parry);
            SetObject(feedback, "parrySuccessPrefab", feedbackPrefabs.parrySuccess);
            SetObject(feedback, "bossWarningPrefab", feedbackPrefabs.bossWarning);
            SetObject(feedback, "bossImpactPrefab", feedbackPrefabs.bossImpact);
            SetObject(feedback, "perfectCue", audioCues.perfect);
            SetObject(feedback, "goodCue", audioCues.good);
            SetObject(feedback, "missCue", audioCues.miss);
            SetObject(feedback, "dodgeCue", audioCues.dodge);
            SetObject(feedback, "parryCue", audioCues.parry);
            SetObject(feedback, "bossWarningCue", audioCues.bossWarning);
            SetObject(feedback, "bossImpactCue", audioCues.bossImpact);
            SetObject(feedback, "playerDamageCue", audioCues.playerDamage);
            SetObject(feedback, "enemyDefeatedCue", audioCues.enemyDefeated);

            if (audioPrefab != null)
            {
                GameObject audioRig = (GameObject)PrefabUtility.InstantiatePrefab(audioPrefab);
                audioRig.name = "AudioCueRig";
            }

            if (Object.FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
            }

            GameObject directionalLight = GameObject.Find("Directional Light");
            if (directionalLight != null)
            {
                directionalLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                var light = directionalLight.GetComponent<Light>();
                if (light != null) light.intensity = 1.1f;
            }

            if (Camera.main != null)
            {
                Camera.main.name = "Main Camera";
                Camera.main.transform.SetPositionAndRotation(new Vector3(0f, 12f, -15f), Quaternion.Euler(55f, 0f, 0f));
                var follow = Camera.main.GetComponent<SimpleFollowCamera>() ?? Camera.main.gameObject.AddComponent<SimpleFollowCamera>();
                SetObject(follow, "target", player.transform);
                SetVector3(follow, "offset", new Vector3(0f, 12f, -13f));
                SetFloat(follow, "followDamping", 6f);
            }
        }

        private static void CreateBoundary(string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.position = position;
            wall.transform.localScale = scale;
            AssignMaterial(wall, material);
        }

        private static void CreateDungeonDecor()
        {
            GameObject root = new GameObject("DungeonDecor");
            string[] tilePaths =
            {
                "Assets/ThirdParty/Kenney/TinyDungeon/Tiles/tile_0000.png",
                "Assets/ThirdParty/Kenney/TinyDungeon/Tiles/tile_0001.png",
                "Assets/ThirdParty/Kenney/TinyDungeon/Tiles/tile_0002.png",
                "Assets/ThirdParty/Kenney/TinyDungeon/Tiles/tile_0016.png",
                "Assets/ThirdParty/Kenney/TinyDungeon/Tiles/tile_0017.png"
            };
            Vector3[] positions =
            {
                new Vector3(-12f, 0.03f, 12f),
                new Vector3(-9f, 0.03f, 12f),
                new Vector3(12f, 0.03f, 12f),
                new Vector3(-12f, 0.03f, -12f),
                new Vector3(12f, 0.03f, -12f)
            };

            for (int i = 0; i < tilePaths.Length; i++)
            {
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(tilePaths[i]);
                if (texture == null)
                    continue;

                Material material = CreateMaterial("Assets/Art/Materials/TinyDungeon_" + Path.GetFileNameWithoutExtension(tilePaths[i]) + ".mat", Color.white, tilePaths[i], true);
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tile.name = "DungeonDecor_" + Path.GetFileNameWithoutExtension(tilePaths[i]);
                tile.transform.SetParent(root.transform, false);
                tile.transform.SetPositionAndRotation(positions[i], Quaternion.Euler(90f, 0f, 0f));
                tile.transform.localScale = Vector3.one * 2.2f;
                Collider collider = tile.GetComponent<Collider>();
                if (collider != null)
                    Object.DestroyImmediate(collider);
                AssignMaterial(tile, material);
            }
        }

        private static Transform CreateSpawnPoint(string name, Vector3 position)
        {
            GameObject spawn = new GameObject(name);
            spawn.transform.position = position;
            return spawn.transform;
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };
        }

        private static GameObject SavePrefab(string path, GameObject source)
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                Object.DestroyImmediate(source);
                return existing;
            }

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(source, path);
            Object.DestroyImmediate(source);
            return prefab;
        }

        private static Material CreateMaterial(string path, Color color, string texturePath = null, bool transparent = false)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            bool changed = false;
            if (material == null)
            {
                material = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(material, path);
                changed = true;
            }

            Texture2D texture = !string.IsNullOrEmpty(texturePath) ? AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath) : null;
            if (material.color != color)
            {
                material.color = color;
                changed = true;
            }

            if (material.mainTexture != texture)
            {
                material.mainTexture = texture;
                changed = true;
            }

            if (transparent)
                changed |= ConfigureTransparentMaterial(material);
            else
                changed |= ConfigureOpaqueMaterial(material);

            if (changed)
                EditorUtility.SetDirty(material);
            return material;
        }

        private static bool ConfigureTransparentMaterial(Material material)
        {
            bool changed = false;
            changed |= SetMaterialFloat(material, "_Mode", 3f);
            changed |= SetMaterialInt(material, "_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            changed |= SetMaterialInt(material, "_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            changed |= SetMaterialInt(material, "_ZWrite", 0);
            changed |= SetMaterialKeyword(material, "_ALPHATEST_ON", false);
            changed |= SetMaterialKeyword(material, "_ALPHABLEND_ON", true);
            changed |= SetMaterialKeyword(material, "_ALPHAPREMULTIPLY_ON", false);
            if (material.renderQueue != 3000)
            {
                material.renderQueue = 3000;
                changed = true;
            }

            return changed;
        }

        private static bool ConfigureOpaqueMaterial(Material material)
        {
            bool changed = false;
            changed |= SetMaterialFloat(material, "_Mode", 0f);
            changed |= SetMaterialInt(material, "_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            changed |= SetMaterialInt(material, "_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            changed |= SetMaterialInt(material, "_ZWrite", 1);
            changed |= SetMaterialKeyword(material, "_ALPHATEST_ON", false);
            changed |= SetMaterialKeyword(material, "_ALPHABLEND_ON", false);
            changed |= SetMaterialKeyword(material, "_ALPHAPREMULTIPLY_ON", false);
            if (material.renderQueue != -1)
            {
                material.renderQueue = -1;
                changed = true;
            }

            return changed;
        }

        private static bool SetMaterialFloat(Material material, string propertyName, float value)
        {
            if (Mathf.Approximately(material.GetFloat(propertyName), value))
                return false;

            material.SetFloat(propertyName, value);
            return true;
        }

        private static bool SetMaterialInt(Material material, string propertyName, int value)
        {
            if (material.GetInt(propertyName) == value)
                return false;

            material.SetInt(propertyName, value);
            return true;
        }

        private static bool SetMaterialKeyword(Material material, string keyword, bool enabled)
        {
            if (material.IsKeywordEnabled(keyword) == enabled)
                return false;

            if (enabled)
                material.EnableKeyword(keyword);
            else
                material.DisableKeyword(keyword);

            return true;
        }

        private static void AssignMaterial(GameObject go, Material material)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;
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

        private static void SetFloat(Object target, string fieldName, float value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(fieldName);
            if (property != null)
            {
                property.floatValue = value;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void SetInt(Object target, string fieldName, int value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(fieldName);
            if (property != null)
            {
                property.intValue = value;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void SetBool(Object target, string fieldName, bool value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(fieldName);
            if (property != null)
            {
                property.boolValue = value;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void SetVector3(Object target, string fieldName, Vector3 value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(fieldName);
            if (property != null)
            {
                property.vector3Value = value;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private struct MaterialSet
        {
            public Material player;
            public Material mage;
            public Material archer;
            public Material healer;
            public Material enemy;
            public Material rangedEnemy;
            public Material boss;
            public Material projectile;
            public Material projectileTrail;
            public Material ground;
            public Material wall;
            public Material perfect;
            public Material good;
            public Material miss;
            public Material dodge;
            public Material parry;
            public Material parrySuccess;
            public Material enemyWindup;
            public Material bossWarning;
            public Material bossImpact;
        }

        private struct FeedbackPrefabs
        {
            public GameObject perfect;
            public GameObject good;
            public GameObject miss;
            public GameObject dodge;
            public GameObject parry;
            public GameObject parrySuccess;
            public GameObject bossWarning;
            public GameObject bossImpact;
            public GameObject enemyWindup;
            public GameObject projectileTrail;
        }

        private struct AudioCueSet
        {
            public AudioCueDefinition perfect;
            public AudioCueDefinition good;
            public AudioCueDefinition miss;
            public AudioCueDefinition dodge;
            public AudioCueDefinition parry;
            public AudioCueDefinition bossWarning;
            public AudioCueDefinition bossImpact;
            public AudioCueDefinition playerDamage;
            public AudioCueDefinition enemyDefeated;
            public AudioCueDefinition footstep;
            public AudioCueDefinition weaponSwing;
        }

        private struct AnimationSet
        {
            public RuntimeAnimatorController playerController;
            public RuntimeAnimatorController enemyController;
            public RuntimeAnimatorController bossController;
        }
    }
}
#endif
