#if UNITY_EDITOR
using Game.AI.Enemies;
using Game.Audio;
using Game.Classes;
using Game.Combat;
using Game.Core;
using Game.Feedback;
using Game.Rhythm;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
            RhythmConfig rhythmConfig = CreateRhythmConfig();
            ComboProfile comboProfile = CreateComboProfile();
            AttackTimingData attackTiming = CreateAttackTiming();
            AbilityDefinition fighterSlash = CreateAbility("Assets/ScriptableObjects/Abilities/FighterSlash.asset", "fighter_slash", "Fighter Slash", "Short-range guard-breaking slash. Perfect timing adds a stronger stagger.", 24, 0, 1.15f, 2.8f, 38f, AbilityType.Damage, AbilityTargetMode.ForwardCone, AbilityExecutionStyle.Instant, new AbilityRhythmScaling { perfectMultiplier = 1.75f, goodMultiplier = 1.15f, missMultiplier = 0.6f }, 0.6f, 0f);
            AbilityDefinition mageBolt = CreateAbility("Assets/ScriptableObjects/Abilities/MageBolt.asset", "mage_bolt", "Mage Bolt", "Medium-range focused spell with a longer recovery.", 28, 0, 2.0f, 7f, 1.2f, AbilityType.Damage, AbilityTargetMode.TargetPoint, AbilityExecutionStyle.ProjectileLike, new AbilityRhythmScaling { perfectMultiplier = 1.55f, goodMultiplier = 1.1f, missMultiplier = 0.65f }, 0.25f, 0f);
            AbilityDefinition archerShot = CreateAbility("Assets/ScriptableObjects/Abilities/ArcherShot.asset", "archer_shot", "Archer Shot", "Long-range precision shot that heavily rewards Perfect timing.", 20, 0, 1.35f, 10f, 0.9f, AbilityType.Damage, AbilityTargetMode.TargetPoint, AbilityExecutionStyle.ProjectileLike, new AbilityRhythmScaling { perfectMultiplier = 2.0f, goodMultiplier = 1.2f, missMultiplier = 0.5f }, 0.2f, 0f);
            AbilityDefinition healerPulse = CreateAbility("Assets/ScriptableObjects/Abilities/HealerPulse.asset", "healer_pulse", "Healer Pulse", "Self heal pulse. Perfect timing adds a brief protection window.", 0, 20, 2.6f, 0f, 3.5f, AbilityType.Heal, AbilityTargetMode.Self, AbilityExecutionStyle.Pulse, new AbilityRhythmScaling { perfectMultiplier = 1.6f, goodMultiplier = 1.15f, missMultiplier = 0.7f }, 0f, 0.35f);
            FeedbackPrefabs feedbackPrefabs = CreateFeedbackPrefabs(materials);
            BossTelegraphData bossSlam = CreateBossTelegraph(feedbackPrefabs.bossWarning, feedbackPrefabs.bossImpact);
            CreateItem("Assets/ScriptableObjects/Items/Gold.asset", "gold", "Gold", 999);
            CreateItem("Assets/ScriptableObjects/Items/HealthPotion.asset", "health_potion", "Health Potion", 10);

            GameObject audioPrefab = CreateAudioCuePlayerPrefab();
            GameObject projectilePrefab = CreateProjectilePrefab(materials.projectile, materials.projectileTrail);
            GameObject fighterPrefab = CreatePlayerPrefab("Assets/Prefabs/Player/FighterPlayer.prefab", typeof(Fighter), fighterSlash, rhythmConfig, comboProfile, attackTiming, materials.player, audioCues);
            GameObject magePrefab = CreatePlayerPrefab("Assets/Prefabs/Player/MagePlayer.prefab", typeof(Mage), mageBolt, rhythmConfig, comboProfile, attackTiming, materials.mage, audioCues);
            GameObject archerPrefab = CreatePlayerPrefab("Assets/Prefabs/Player/ArcherPlayer.prefab", typeof(Archer), archerShot, rhythmConfig, comboProfile, attackTiming, materials.archer, audioCues);
            GameObject healerPrefab = CreatePlayerPrefab("Assets/Prefabs/Player/HealerPlayer.prefab", typeof(Healer), healerPulse, rhythmConfig, comboProfile, attackTiming, materials.healer, audioCues);
            GameObject meleePrefab = CreateMeleeEnemyPrefab(materials.enemy, feedbackPrefabs.enemyWindup);
            GameObject rangedPrefab = CreateRangedEnemyPrefab(projectilePrefab, materials.rangedEnemy, feedbackPrefabs.enemyWindup);
            GameObject bossPrefab = CreateBossPrefab(projectilePrefab, bossSlam, materials.boss, feedbackPrefabs.enemyWindup, audioCues);
            GameObject hudPrefab = CreateHudPrefab();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            scene.name = "VerticalSlice";
            BuildScene(fighterPrefab, magePrefab, archerPrefab, healerPrefab, meleePrefab, rangedPrefab, bossPrefab, hudPrefab, rhythmConfig, bossSlam, materials, feedbackPrefabs, audioCues, audioPrefab);
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
                projectileTrail = CreateMaterial("Assets/Materials/Feedback/ProjectileTrail.mat", new Color(0.1f, 0.9f, 1f, 0.45f)),
                ground = CreateMaterial("Assets/Materials/Ground_Prototype.mat", new Color(0.28f, 0.32f, 0.28f)),
                perfect = CreateMaterial("Assets/Materials/Feedback/PerfectFeedback.mat", new Color(0.2f, 1f, 0.75f, 0.85f)),
                good = CreateMaterial("Assets/Materials/Feedback/GoodFeedback.mat", new Color(0.35f, 0.65f, 1f, 0.75f)),
                miss = CreateMaterial("Assets/Materials/Feedback/MissFeedback.mat", new Color(1f, 0.25f, 0.25f, 0.65f)),
                dodge = CreateMaterial("Assets/Materials/Feedback/DodgeFeedback.mat", new Color(1f, 1f, 0.35f, 0.7f)),
                parry = CreateMaterial("Assets/Materials/Feedback/ParryFeedback.mat", new Color(1f, 0.45f, 1f, 0.8f)),
                parrySuccess = CreateMaterial("Assets/Materials/Feedback/ParrySuccess.mat", new Color(1f, 1f, 1f, 0.9f)),
                enemyWindup = CreateMaterial("Assets/Materials/Feedback/EnemyWindup.mat", new Color(1f, 0.85f, 0.1f, 0.85f)),
                bossWarning = CreateMaterial("Assets/Materials/Feedback/BossWarning.mat", new Color(1f, 0.65f, 0.1f, 0.55f)),
                bossImpact = CreateMaterial("Assets/Materials/Feedback/BossImpact.mat", new Color(1f, 0.1f, 0.05f, 0.8f))
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

        private static BossTelegraphData CreateBossTelegraph(GameObject warningPrefab, GameObject impactPrefab)
        {
            BossTelegraphData telegraph = CreateOrLoadAsset<BossTelegraphData>("Assets/ScriptableObjects/Boss/BossSlamTelegraph.asset");
            telegraph.telegraphId = "boss_slam";
            telegraph.displayName = "Boss Slam";
            telegraph.beatsBeforeImpact = 4;
            telegraph.damage = 10;
            telegraph.radius = 3f;
            telegraph.range = 8f;
            telegraph.attackType = BossTelegraphAttackType.TargetedCircle;
            telegraph.warningVfxPrefab = warningPrefab;
            telegraph.impactVfxPrefab = impactPrefab;
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

        private static GameObject CreatePlayerPrefab(string path, System.Type classType, AbilityDefinition ability, RhythmConfig rhythmConfig, ComboProfile comboProfile, AttackTimingData attackTiming, Material material, AudioCueSet audioCues)
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = System.IO.Path.GetFileNameWithoutExtension(path);
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 1f, 0f);
            Object.DestroyImmediate(player.GetComponent<Collider>());
            AssignMaterial(player, material);

            player.AddComponent<CharacterController>();
            player.AddComponent(classType);
            player.AddComponent<AudioSource>();
            var cuePlayer = player.AddComponent<AudioCuePlayer>();
            var animationBridge = player.AddComponent<Game.Animation.CombatAnimationBridge>();
            player.AddComponent<Game.Animation.AnimationEventRelay>();
            SetObject(animationBridge, "audioCuePlayer", cuePlayer);
            SetObject(animationBridge, "footstepCue", audioCues.footstep);
            SetObject(animationBridge, "weaponSwingCue", audioCues.weaponSwing);
            var playerController = player.AddComponent<PlayerController>();
            SetFloat(playerController, "moveSpeed", 5.8f);
            SetFloat(playerController, "jumpHeight", 1.4f);
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
            SetObject(combo, "attackTiming", attackTiming);
            SetFloat(combo, "attackRange", 2.2f);

            var abilityController = player.AddComponent<AbilityController>();
            SetObject(abilityController, "abilityBuffer", abilityBuffer);
            SetObject(abilityController, "abilities", new[] { ability });

            var dodge = player.AddComponent<DodgeController>();
            SetObject(dodge, "dodgeBuffer", dodgeBuffer);
            SetFloat(dodge, "distance", 3.6f);
            SetFloat(dodge, "cooldown", 0.7f);

            var parry = player.AddComponent<ParryController>();
            SetObject(parry, "parryBuffer", parryBuffer);
            SetFloat(parry, "cooldown", 0.75f);

            return SavePrefab(path, player);
        }

        private static GameObject CreateMeleeEnemyPrefab(Material material, GameObject windupPrefab)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "MeleeEnemy";
            Object.DestroyImmediate(enemy.GetComponent<Collider>());
            AssignMaterial(enemy, material);
            enemy.AddComponent<CharacterController>();
            var melee = enemy.AddComponent<MeleeEnemy>();
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

        private static GameObject CreateRangedEnemyPrefab(GameObject projectilePrefab, Material material, GameObject windupPrefab)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "RangedEnemy";
            Object.DestroyImmediate(enemy.GetComponent<Collider>());
            AssignMaterial(enemy, material);
            enemy.AddComponent<CharacterController>();
            var ranged = enemy.AddComponent<RangedEnemy>();
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

        private static GameObject CreateBossPrefab(GameObject projectilePrefab, BossTelegraphData telegraphData, Material material, GameObject windupPrefab, AudioCueSet audioCues)
        {
            GameObject boss = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            boss.name = "BossEnemy";
            boss.transform.localScale = new Vector3(1.8f, 1.8f, 1.8f);
            Object.DestroyImmediate(boss.GetComponent<Collider>());
            AssignMaterial(boss, material);
            boss.AddComponent<CharacterController>();
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
            boss.AddComponent<BossPhaseController>();
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
            panelRect.sizeDelta = new Vector2(-24f, 132f);

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
            Text boss = CreateText("BossText", panel.transform, new Vector2(0f, -116f), new Vector2(500f, 22f), 14, TextAnchor.MiddleCenter);
            Text debug = CreateText("DebugText", root.transform, new Vector2(12f, 12f), new Vector2(230f, 100f), 12, TextAnchor.LowerLeft);
            var debugRect = debug.GetComponent<RectTransform>();
            debugRect.anchorMin = new Vector2(0f, 0f);
            debugRect.anchorMax = new Vector2(0f, 0f);
            debugRect.pivot = new Vector2(0f, 0f);

            var beatBarUi = root.AddComponent<Game.UI.BeatBarUI>();
            SetObject(beatBarUi, "marker", markerRect);
            SetObject(beatBarUi, "bar", beatBarRect);
            SetObject(beatBarUi, "feedbackText", feedback);

            var hud = root.AddComponent<Game.UI.VerticalSliceHud>();
            SetObject(hud, "feedbackText", feedback);
            SetObject(hud, "comboText", combo);
            SetObject(hud, "abilityText", ability);
            SetObject(hud, "dodgeText", dodge);
            SetObject(hud, "parryText", parry);
            SetObject(hud, "attackStateText", attack);
            SetObject(hud, "bossText", boss);
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

        private static void BuildScene(GameObject fighterPrefab, GameObject magePrefab, GameObject archerPrefab, GameObject healerPrefab, GameObject meleePrefab, GameObject rangedPrefab, GameObject bossPrefab, GameObject hudPrefab, RhythmConfig rhythmConfig, BossTelegraphData bossTelegraph, MaterialSet materials, FeedbackPrefabs feedbackPrefabs, AudioCueSet audioCues, GameObject audioPrefab)
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Arena_Ground";
            ground.transform.localScale = new Vector3(4f, 1f, 4f);
            AssignMaterial(ground, materials.ground);
            CreateBoundary("Arena_Wall_North", new Vector3(0f, 1f, 20f), new Vector3(40f, 2f, 0.5f), materials.ground);
            CreateBoundary("Arena_Wall_South", new Vector3(0f, 1f, -20f), new Vector3(40f, 2f, 0.5f), materials.ground);
            CreateBoundary("Arena_Wall_East", new Vector3(20f, 1f, 0f), new Vector3(0.5f, 2f, 40f), materials.ground);
            CreateBoundary("Arena_Wall_West", new Vector3(-20f, 1f, 0f), new Vector3(0.5f, 2f, 40f), materials.ground);

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
            spawner.spawnPoints = new[]
            {
                CreateSpawnPoint("EnemySpawn_A", new Vector3(7f, 0f, 4f)),
                CreateSpawnPoint("EnemySpawn_B", new Vector3(-7f, 0f, 4f))
            };

            GameObject hud = (GameObject)PrefabUtility.InstantiatePrefab(hudPrefab);
            hud.name = "VerticalSliceHUD";
            var hudController = hud.GetComponentInChildren<Game.UI.VerticalSliceHud>();
            if (hudController != null)
            {
                SetObject(hudController, "comboSystem", player.GetComponent<ComboSystem>());
                SetObject(hudController, "abilityController", player.GetComponent<AbilityController>());
                SetObject(hudController, "dodgeController", player.GetComponent<DodgeController>());
                SetObject(hudController, "parryController", player.GetComponent<ParryController>());
                SetObject(hudController, "bossTelegraphController", telegraph);
                SetObject(hudController, "playerCharacter", player.GetComponent<BaseCharacter>());
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
    }
}
#endif
