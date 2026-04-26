#if UNITY_EDITOR
using Game.AI.Enemies;
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
            RhythmConfig rhythmConfig = CreateRhythmConfig();
            ComboProfile comboProfile = CreateComboProfile();
            AbilityDefinition fighterSlash = CreateAbility("Assets/ScriptableObjects/Abilities/FighterSlash.asset", "fighter_slash", "Fighter Slash", "Close-range rhythm slash.", 18, 0, 1.2f, 3.5f, 45f, AbilityType.Damage, AbilityTargetMode.ForwardCone);
            AbilityDefinition mageBolt = CreateAbility("Assets/ScriptableObjects/Abilities/MageBolt.asset", "mage_bolt", "Mage Bolt", "Medium-range rhythm spell.", 16, 0, 1.6f, 7f, 1.4f, AbilityType.Damage, AbilityTargetMode.TargetPoint);
            AbilityDefinition archerShot = CreateAbility("Assets/ScriptableObjects/Abilities/ArcherShot.asset", "archer_shot", "Archer Shot", "Long-range rhythm shot.", 14, 0, 1.1f, 9f, 1f, AbilityType.Damage, AbilityTargetMode.TargetPoint);
            AbilityDefinition healerPulse = CreateAbility("Assets/ScriptableObjects/Abilities/HealerPulse.asset", "healer_pulse", "Healer Pulse", "Self heal pulse.", 0, 16, 2.5f, 0f, 3f, AbilityType.Heal, AbilityTargetMode.Self);
            BossTelegraphData bossSlam = CreateBossTelegraph();
            CreateItem("Assets/ScriptableObjects/Items/Gold.asset", "gold", "Gold", 999);
            CreateItem("Assets/ScriptableObjects/Items/HealthPotion.asset", "health_potion", "Health Potion", 10);

            GameObject projectilePrefab = CreateProjectilePrefab(materials.projectile);
            GameObject fighterPrefab = CreatePlayerPrefab("Assets/Prefabs/Player/FighterPlayer.prefab", typeof(Fighter), fighterSlash, rhythmConfig, comboProfile, materials.player);
            GameObject magePrefab = CreatePlayerPrefab("Assets/Prefabs/Player/MagePlayer.prefab", typeof(Mage), mageBolt, rhythmConfig, comboProfile, materials.mage);
            GameObject archerPrefab = CreatePlayerPrefab("Assets/Prefabs/Player/ArcherPlayer.prefab", typeof(Archer), archerShot, rhythmConfig, comboProfile, materials.archer);
            GameObject healerPrefab = CreatePlayerPrefab("Assets/Prefabs/Player/HealerPlayer.prefab", typeof(Healer), healerPulse, rhythmConfig, comboProfile, materials.healer);
            GameObject meleePrefab = CreateMeleeEnemyPrefab(materials.enemy);
            GameObject rangedPrefab = CreateRangedEnemyPrefab(projectilePrefab, materials.rangedEnemy);
            GameObject bossPrefab = CreateBossPrefab(projectilePrefab, bossSlam, materials.boss);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            scene.name = "VerticalSlice";
            BuildScene(fighterPrefab, magePrefab, archerPrefab, healerPrefab, meleePrefab, rangedPrefab, bossPrefab, rhythmConfig, bossSlam, materials);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Created playable vertical slice scene at " + ScenePath);
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
                "Assets/ScriptableObjects",
                "Assets/ScriptableObjects/Rhythm",
                "Assets/ScriptableObjects/Abilities",
                "Assets/ScriptableObjects/Combat",
                "Assets/ScriptableObjects/Boss",
                "Assets/ScriptableObjects/Items",
                "Assets/Materials",
                "Assets/Editor"
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
                ground = CreateMaterial("Assets/Materials/Ground_Prototype.mat", new Color(0.28f, 0.32f, 0.28f))
            };
        }

        private static RhythmConfig CreateRhythmConfig()
        {
            RhythmConfig config = CreateOrLoadAsset<RhythmConfig>("Assets/ScriptableObjects/Rhythm/DefaultRhythmConfig.asset");
            config.bpm = 120f;
            config.dspOffsetSeconds = 0d;
            config.perfectWindow = 0.05f;
            config.goodWindow = 0.10f;
            config.perfectDamageMultiplier = 1.5f;
            config.goodDamageMultiplier = 1.15f;
            config.missDamageMultiplier = 0.75f;
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

        private static AbilityDefinition CreateAbility(string path, string id, string displayName, string description, int damage, int heal, float cooldown, float range, float radius, AbilityType type, AbilityTargetMode targetMode)
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
            ability.rhythmScaling = AbilityRhythmScaling.Default;
            EditorUtility.SetDirty(ability);
            return ability;
        }

        private static BossTelegraphData CreateBossTelegraph()
        {
            BossTelegraphData telegraph = CreateOrLoadAsset<BossTelegraphData>("Assets/ScriptableObjects/Boss/BossSlamTelegraph.asset");
            telegraph.telegraphId = "boss_slam";
            telegraph.displayName = "Boss Slam";
            telegraph.beatsBeforeImpact = 3;
            telegraph.damage = 18;
            telegraph.radius = 3f;
            telegraph.range = 6f;
            telegraph.attackType = BossTelegraphAttackType.TargetedCircle;
            EditorUtility.SetDirty(telegraph);
            return telegraph;
        }

        private static ItemDefinition CreateItem(string path, string id, string displayName, int maxStack)
        {
            ItemDefinition item = CreateOrLoadAsset<ItemDefinition>(path);
            item.itemId = id;
            item.displayName = displayName;
            item.maxStack = maxStack;
            EditorUtility.SetDirty(item);
            return item;
        }

        private static GameObject CreatePlayerPrefab(string path, System.Type classType, AbilityDefinition ability, RhythmConfig rhythmConfig, ComboProfile comboProfile, Material material)
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = System.IO.Path.GetFileNameWithoutExtension(path);
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 1f, 0f);
            Object.DestroyImmediate(player.GetComponent<Collider>());
            AssignMaterial(player, material);

            player.AddComponent<CharacterController>();
            player.AddComponent(classType);
            var playerController = player.AddComponent<PlayerController>();
            var judgement = player.AddComponent<RhythmJudgement>();
            SetObject(judgement, "config", rhythmConfig);

            var attackBuffer = player.AddComponent<InputBuffer>();
            var abilityBuffer = player.AddComponent<InputBuffer>();
            var dodgeBuffer = player.AddComponent<InputBuffer>();
            var parryBuffer = player.AddComponent<InputBuffer>();
            SetObject(attackBuffer, "judgement", judgement);
            SetObject(abilityBuffer, "judgement", judgement);
            SetObject(dodgeBuffer, "judgement", judgement);
            SetObject(parryBuffer, "judgement", judgement);
            SetObject(playerController, "attackBuffer", attackBuffer);

            var combo = player.AddComponent<ComboSystem>();
            SetObject(combo, "profile", comboProfile);
            SetObject(combo, "judgement", judgement);

            var abilityController = player.AddComponent<AbilityController>();
            SetObject(abilityController, "abilityBuffer", abilityBuffer);
            SetObject(abilityController, "abilities", new[] { ability });

            var dodge = player.AddComponent<DodgeController>();
            SetObject(dodge, "dodgeBuffer", dodgeBuffer);

            var parry = player.AddComponent<ParryController>();
            SetObject(parry, "parryBuffer", parryBuffer);

            return SavePrefab(path, player);
        }

        private static GameObject CreateMeleeEnemyPrefab(Material material)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "MeleeEnemy";
            Object.DestroyImmediate(enemy.GetComponent<Collider>());
            AssignMaterial(enemy, material);
            enemy.AddComponent<CharacterController>();
            enemy.AddComponent<MeleeEnemy>();
            return SavePrefab("Assets/Prefabs/Enemies/MeleeEnemy.prefab", enemy);
        }

        private static GameObject CreateRangedEnemyPrefab(GameObject projectilePrefab, Material material)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "RangedEnemy";
            Object.DestroyImmediate(enemy.GetComponent<Collider>());
            AssignMaterial(enemy, material);
            enemy.AddComponent<CharacterController>();
            var ranged = enemy.AddComponent<RangedEnemy>();

            GameObject poolObject = new GameObject("ProjectilePool");
            poolObject.transform.SetParent(enemy.transform);
            var pool = poolObject.AddComponent<ObjectPool>();
            SetObject(pool, "prefab", projectilePrefab);
            SetObject(ranged, "projectilePrefab", projectilePrefab);
            SetObject(ranged, "projectilePool", pool);

            return SavePrefab("Assets/Prefabs/Enemies/RangedEnemy.prefab", enemy);
        }

        private static GameObject CreateBossPrefab(GameObject projectilePrefab, BossTelegraphData telegraphData, Material material)
        {
            GameObject boss = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            boss.name = "BossEnemy";
            boss.transform.localScale = new Vector3(1.8f, 1.8f, 1.8f);
            Object.DestroyImmediate(boss.GetComponent<Collider>());
            AssignMaterial(boss, material);
            boss.AddComponent<CharacterController>();
            var bossEnemy = boss.AddComponent<BossEnemy>();
            boss.AddComponent<BossPhaseController>();
            var telegraph = boss.AddComponent<BossTelegraphController>();

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

        private static GameObject CreateProjectilePrefab(Material material)
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
            projectile.AddComponent<PoolableProjectile>();
            return SavePrefab("Assets/Prefabs/Projectiles/BasicProjectile.prefab", projectile);
        }

        private static void BuildScene(GameObject fighterPrefab, GameObject magePrefab, GameObject archerPrefab, GameObject healerPrefab, GameObject meleePrefab, GameObject rangedPrefab, GameObject bossPrefab, RhythmConfig rhythmConfig, BossTelegraphData bossTelegraph, MaterialSet materials)
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(3f, 1f, 3f);
            AssignMaterial(ground, materials.ground);

            GameObject playerManagerObject = new GameObject("PlayerManager");
            playerManagerObject.AddComponent<PlayerManager>();

            GameObject gameManagerObject = new GameObject("GameManager");
            var gameManager = gameManagerObject.AddComponent<GameManager>();
            gameManager.fighterPrefab = fighterPrefab;
            gameManager.magePrefab = magePrefab;
            gameManager.archerPrefab = archerPrefab;
            gameManager.healerPrefab = healerPrefab;

            GameObject rhythm = new GameObject("RhythmSystem");
            var beatClock = rhythm.AddComponent<BeatClock>();
            var rhythmJudgement = rhythm.AddComponent<RhythmJudgement>();
            SetObject(beatClock, "config", rhythmConfig);
            SetObject(rhythmJudgement, "config", rhythmConfig);

            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(fighterPrefab);
            player.name = "Player_Fighter";
            player.transform.position = new Vector3(0f, 1f, -4f);

            GameObject melee = (GameObject)PrefabUtility.InstantiatePrefab(meleePrefab);
            melee.name = "MeleeEnemy";
            melee.transform.position = new Vector3(4f, 1f, 2f);

            GameObject ranged = (GameObject)PrefabUtility.InstantiatePrefab(rangedPrefab);
            ranged.name = "RangedEnemy";
            ranged.transform.position = new Vector3(-5f, 1f, 4f);

            GameObject boss = (GameObject)PrefabUtility.InstantiatePrefab(bossPrefab);
            boss.name = "BossEnemy";
            boss.transform.position = new Vector3(0f, 1.8f, 8f);
            var telegraph = boss.GetComponent<BossTelegraphController>();
            if (telegraph != null)
                SetObject(telegraph, "defaultTelegraph", bossTelegraph);

            GameObject spawnerObject = new GameObject("EnemySpawner");
            var spawner = spawnerObject.AddComponent<EnemySpawner>();
            spawner.enemyPrefabs = new[] { meleePrefab, rangedPrefab };
            spawner.spawnInterval = 10f;
            spawner.maxEnemies = 4;
            spawner.spawnPoints = new[]
            {
                CreateSpawnPoint("EnemySpawn_A", new Vector3(7f, 0f, 4f)),
                CreateSpawnPoint("EnemySpawn_B", new Vector3(-7f, 0f, 4f))
            };

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
                Camera.main.transform.SetPositionAndRotation(new Vector3(0f, 9f, -11f), Quaternion.Euler(55f, 0f, 0f));
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
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(source, path);
            Object.DestroyImmediate(source);
            return prefab;
        }

        private static Material CreateMaterial(string path, Color color)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
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
            public Material ground;
        }
    }
}
#endif
