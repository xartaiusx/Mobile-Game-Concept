#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Game.Editor;
using Game.AI.Enemies;
using Game.Combat;
using Game.Core;
using Game.Feedback;
using Game.Audio;
using Game.Animation;
using Game.Rhythm;
using Game.Systems;
using Game.UI;
using Game.Classes;
using Game.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class VerticalSlicePlayModeTests
{
    private const string StartupScenePath = VerticalSliceStartup.ScenePath;

    [TearDown]
    public void TearDown()
    {
        Time.timeScale = 1f;
    }

    [UnityTest]
    public IEnumerator EditorPlayButtonStartupSceneLoadsPlayableVerticalSlice()
    {
        VerticalSliceStartup.EnsureStartupSceneConfigured();
        Assert.IsEmpty(VerticalSliceStartup.GetStartupValidationFailures());
        Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<SceneAsset>(StartupScenePath));

        var fatalLogs = new List<string>();
        Application.LogCallback callback = (condition, stackTrace, type) =>
        {
            if (IsFatalStartupLog(condition, type))
                fatalLogs.Add(type + ": " + condition);
        };
        Application.logMessageReceived += callback;

        yield return LoadVerticalSlice();
        for (int i = 0; i < 5; i++)
            yield return null;

        Application.logMessageReceived -= callback;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Assert.AreEqual(1, players.Length);
        Assert.IsNotNull(players[0].GetComponent<PlayerController>());
        Assert.IsNotNull(players[0].GetComponent<BaseCharacter>());
        Assert.IsNotNull(players[0].transform.Find("VisualRoot"));
        Assert.IsNotNull(players[0].GetComponent<VisualAttachmentRoot>());

        Camera mainCamera = Camera.main;
        Assert.IsNotNull(mainCamera);
        Assert.IsTrue(mainCamera.enabled);
        Assert.IsNotNull(Object.FindAnyObjectByType<Canvas>());
        Assert.IsNotNull(Object.FindAnyObjectByType<BeatClock>());
        Assert.IsNotNull(Object.FindAnyObjectByType<RhythmJudgement>());
        Assert.IsNotNull(Object.FindAnyObjectByType<ScoreSystem>());
        Assert.IsNotNull(Object.FindAnyObjectByType<TelemetryManager>());
        Assert.IsNotNull(Object.FindAnyObjectByType<ArenaController>());
        Assert.Greater(Object.FindObjectsByType<BaseEnemy>(FindObjectsInactive.Exclude).Length, 0);
        foreach (BaseEnemy enemy in Object.FindObjectsByType<BaseEnemy>(FindObjectsInactive.Exclude))
            Assert.IsNotNull(enemy.transform.Find("VisualRoot"), enemy.name + " missing VisualRoot.");
        Assert.AreEqual(1f, Time.timeScale, 0.001f);

        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include))
            Assert.AreEqual(0, GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go), go.name + " has missing scripts.");

        Assert.IsEmpty(fatalLogs);
    }

    [UnityTest]
    public IEnumerator VerticalSliceSceneStartsWithCoreGameplayObjects()
    {
        yield return LoadVerticalSlice();

        Assert.IsNotNull(Object.FindAnyObjectByType<PlayerManager>());
        Assert.IsNotNull(Object.FindAnyObjectByType<GameManager>());
        Assert.IsNotNull(Object.FindAnyObjectByType<BeatClock>());
        Assert.IsNotNull(Object.FindAnyObjectByType<RhythmJudgement>());

        GameObject player = GameObject.FindWithTag("Player");
        Assert.IsNotNull(player);
        Assert.IsNotNull(player.GetComponent<Fighter>());
        Assert.AreEqual("Warrior", player.GetComponent<BaseCharacter>().CharacterName);
        Assert.IsNotNull(player.GetComponent<CharacterController>());
        Assert.IsNotNull(player.GetComponent<PlayerController>());
        Assert.IsNotNull(player.GetComponent<ComboSystem>());
        Assert.AreEqual(AttackTimingState.Ready, player.GetComponent<ComboSystem>().TimingState);
        Assert.IsNotNull(player.GetComponent<AbilityController>());
        Assert.IsNotNull(player.GetComponent<DodgeController>());
        Assert.IsNotNull(player.GetComponent<ParryController>());
        Assert.IsNotNull(player.GetComponent<CombatAnimationBridge>());
        Assert.IsNotNull(player.GetComponent<AnimationEventRelay>());
        Assert.IsNotNull(player.GetComponent<InventorySystem>());
        Assert.IsNotNull(player.GetComponent<HitReactionController>());
        Assert.IsNotNull(player.GetComponent<Animator>());
        Assert.IsNotNull(player.GetComponent<Animator>().runtimeAnimatorController);

        Assert.IsNotNull(Object.FindAnyObjectByType<MeleeEnemy>());
        Assert.IsNotNull(Object.FindAnyObjectByType<RangedEnemy>());
        Assert.IsNotNull(Object.FindAnyObjectByType<BossEnemy>(FindObjectsInactive.Include));
        Assert.IsNotNull(Object.FindAnyObjectByType<BossTelegraphController>(FindObjectsInactive.Include));
        Assert.IsNotNull(Object.FindAnyObjectByType<ArenaController>());
        Assert.IsNotNull(Object.FindAnyObjectByType<ScoreSystem>());
        Assert.IsNotNull(Object.FindAnyObjectByType<TelemetryManager>());
        Assert.IsNotNull(Object.FindAnyObjectByType<PickupSpawner>());
        Assert.IsNull(Object.FindAnyObjectByType<ClassSwapDebugController>());
        Assert.IsNotNull(Object.FindAnyObjectByType<Canvas>());
        Assert.IsNotNull(Object.FindAnyObjectByType<BeatBarUI>());
        Assert.IsNotNull(Object.FindAnyObjectByType<VerticalSliceHud>());
        Assert.IsNotNull(Object.FindAnyObjectByType<TelemetryDebugOverlay>(FindObjectsInactive.Include));
        Assert.IsNotNull(Object.FindAnyObjectByType<RhythmFeedbackController>());
        Assert.IsNotNull(Object.FindAnyObjectByType<AudioCuePlayer>());
    }

    [UnityTest]
    public IEnumerator VerticalSliceHasNoDuplicateCriticalSingletons()
    {
        yield return LoadVerticalSlice();

        Assert.AreEqual(1, Object.FindObjectsByType<PlayerManager>(FindObjectsInactive.Exclude).Length);
        Assert.AreEqual(1, Object.FindObjectsByType<GameManager>(FindObjectsInactive.Exclude).Length);
        Assert.AreEqual(1, Object.FindObjectsByType<BeatClock>(FindObjectsInactive.Exclude).Length);
        Assert.AreEqual(1, Object.FindObjectsByType<RhythmJudgement>(FindObjectsInactive.Exclude).Length);
        Assert.AreEqual(1, Object.FindObjectsByType<ScoreSystem>(FindObjectsInactive.Exclude).Length);
        Assert.AreEqual(1, Object.FindObjectsByType<VerticalSliceHud>(FindObjectsInactive.Exclude).Length);
        Assert.AreEqual(1, GameObject.FindGameObjectsWithTag("Player").Length);
    }

    [UnityTest]
    public IEnumerator VerticalSliceUsesWarriorOnlyAndLegacyClassSwapIsInactive()
    {
        yield return LoadVerticalSlice();

        ClassSwapDebugController classSwap = Object.FindAnyObjectByType<ClassSwapDebugController>(FindObjectsInactive.Include);
        Assert.IsTrue(classSwap == null || !classSwap.gameObject.activeInHierarchy);
        SimpleFollowCamera followCamera = Object.FindAnyObjectByType<SimpleFollowCamera>();
        VerticalSliceHud hud = Object.FindAnyObjectByType<VerticalSliceHud>();
        ArenaController arena = Object.FindAnyObjectByType<ArenaController>();
        Assert.IsNotNull(followCamera);
        Assert.IsNotNull(hud);
        Assert.IsNotNull(arena);

        Directory.CreateDirectory("Artifacts/WarriorEndless");
        GameObject player = GameObject.FindWithTag("Player");
        Assert.IsNotNull(player);
        Assert.IsNotNull(player.GetComponent<Fighter>());
        Assert.IsNull(player.GetComponent<Mage>());
        Assert.IsNull(player.GetComponent<Archer>());
        Assert.IsNull(player.GetComponent<Healer>());
        Assert.AreEqual(player.transform, PlayerManager.Instance.GetPlayerTransform());
        Assert.AreEqual(1, GameObject.FindGameObjectsWithTag("Player").Length);
        Assert.AreEqual(player.transform, SerializedTransform(followCamera, "target"));
        Assert.AreEqual(player.GetComponent<ComboSystem>(), SerializedObjectReference<ComboSystem>(hud, "comboSystem"));
        Assert.AreEqual(player.GetComponent<BaseCharacter>(), SerializedObjectReference<BaseCharacter>(arena, "player"));

        player.GetComponent<PlayerSimulationController>().SetSimulationEnabled(true);
        ScreenCapture.CaptureScreenshot("Artifacts/WarriorEndless/warrior_playmode_test.png");
    }

    [UnityTest]
    public IEnumerator VerticalSliceEndlessArenaCanClearWaveAndIncrement()
    {
        yield return LoadVerticalSlice();

        ArenaController arena = Object.FindAnyObjectByType<ArenaController>();
        ScoreSystem score = Object.FindAnyObjectByType<ScoreSystem>();
        Assert.IsNotNull(arena);
        Assert.IsNotNull(score);

        int startingWave = arena.CurrentWave;
        foreach (BaseEnemy enemy in Object.FindObjectsByType<BaseEnemy>(FindObjectsInactive.Exclude))
        {
            if (enemy != null)
                enemy.TakeDamage(9999);
        }

        Assert.AreEqual(ArenaState.WaveCleared, arena.State);
        Assert.AreNotEqual(ArenaState.Victory, arena.State);
        Assert.Greater(score.Score, 0);

        arena.BeginNextWaveForTests();

        Assert.AreEqual(startingWave + 1, arena.CurrentWave);
        Assert.AreNotEqual(ArenaState.Victory, arena.State);
    }

    [UnityTest]
    public IEnumerator VerticalSliceSmokeTestHasNoMissingScripts()
    {
        yield return LoadVerticalSlice();

        Assert.AreEqual(1, GameObject.FindGameObjectsWithTag("Player").Length);
        Assert.AreEqual(1, Object.FindObjectsByType<BeatClock>(FindObjectsInactive.Exclude).Length);
        Assert.AreEqual(1, Object.FindObjectsByType<ScoreSystem>(FindObjectsInactive.Exclude).Length);
        Assert.IsNotNull(Object.FindAnyObjectByType<VerticalSliceHud>());
        Assert.Greater(Object.FindObjectsByType<BaseEnemy>(FindObjectsInactive.Exclude).Length, 0);

        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include))
            Assert.AreEqual(0, GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go), go.name + " has missing scripts.");
    }

    [UnityTest]
    public IEnumerator TelemetrySmokeTestRecordsCombatAndWritesSummary()
    {
        yield return LoadVerticalSlice();

        TelemetryManager telemetry = Object.FindAnyObjectByType<TelemetryManager>();
        Assert.IsNotNull(telemetry);
        telemetry.ResetRun();

        GameObject player = GameObject.FindWithTag("Player");
        Assert.IsNotNull(player);
        var dodge = player.GetComponent<DodgeController>();
        var combo = player.GetComponent<ComboSystem>();
        var score = Object.FindAnyObjectByType<ScoreSystem>();

        TelemetryManager.ReportInputJudgement(RhythmGrade.Perfect, -0.025f);
        dodge.ResolveDodgeForTests(RhythmGrade.Perfect, Vector3.forward);
        combo.ResolveAttackForTests(RhythmGrade.Perfect);
        score.AddHitScore(RhythmGrade.Perfect, 10, 1);

        float timeout = Time.realtimeSinceStartup + 1f;
        while (telemetry.Snapshot.perfectHitCount == 0 && Time.realtimeSinceStartup < timeout)
            yield return null;

        string path = telemetry.WriteRunSummary();

        Assert.Greater(telemetry.Snapshot.perfectDodgeCount, 0);
        Assert.Greater(telemetry.Snapshot.perfectHitCount, 0);
        Assert.Greater(telemetry.Snapshot.totalScore, 0);
        Assert.IsNotEmpty(path);
        Assert.IsTrue(File.Exists(path));
        Assert.Greater(new FileInfo(path).Length, 0);
        TelemetryParseResult parsed = TelemetryAnalysis.ParseJson(File.ReadAllText(path), path);
        Assert.IsTrue(parsed.IsValid, parsed.errorMessage);
        Assert.AreEqual("Warrior", parsed.snapshot.playerClass);
        AssertRenderableVisual(player, "WarriorVisual", true);
    }

    [UnityTest]
    public IEnumerator RhythmHitAndMissScenariosUpdateScoreComboTelemetryAndVisuals()
    {
        yield return LoadVerticalSlice();

        TelemetryManager telemetry = Object.FindAnyObjectByType<TelemetryManager>();
        GameObject player = GameObject.FindWithTag("Player");
        MeleeEnemy enemy = Object.FindAnyObjectByType<MeleeEnemy>();
        ScoreSystem score = Object.FindAnyObjectByType<ScoreSystem>();
        ComboSystem combo = player.GetComponent<ComboSystem>();
        Assert.IsNotNull(telemetry);
        Assert.IsNotNull(player);
        Assert.IsNotNull(enemy);
        Assert.IsNotNull(score);
        Assert.IsNotNull(combo);

        telemetry.ResetRun();
        score.ResetScore();
        player.transform.position = Vector3.zero;
        player.transform.rotation = Quaternion.identity;
        enemy.transform.position = player.transform.position + player.transform.forward * 1.2f;
        Physics.SyncTransforms();

        int startingScore = score.Score;
        combo.StartAttackWindup(0, 0.2f, 8, RhythmGrade.Perfect);
        combo.OpenHitWindow();
        combo.CloseHitWindow();
        combo.FinishAttackRecovery();

        Assert.Greater(score.Score, startingScore);
        Assert.Greater(telemetry.CurrentComboLength, 0);
        Assert.AreEqual(1, telemetry.Snapshot.perfectHitCount);
        Assert.GreaterOrEqual(telemetry.Snapshot.maxCombo, 1);
        AssertRenderableVisual(player, "WarriorVisual", true);
        AssertRenderableVisual(enemy.gameObject, "BasicEnemyVisual", true);

        telemetry.ResetRun();
        combo.CancelAttackForTests();
        combo.StartAttackWindup(0, 0.2f, 1, RhythmGrade.Miss);
        combo.OpenHitWindow();
        combo.CloseHitWindow();
        combo.FinishAttackRecovery();

        Assert.AreEqual(1, telemetry.Snapshot.missHitCount);
        Assert.GreaterOrEqual(combo.CurrentComboCount, 0);
        AssertRenderableVisual(player, "WarriorVisual", true);
    }

    [UnityTest]
    public IEnumerator DodgePickupBossAndEnemyScenarioPathsRemainSafe()
    {
        yield return LoadVerticalSlice();

        TelemetryManager telemetry = Object.FindAnyObjectByType<TelemetryManager>();
        GameObject player = GameObject.FindWithTag("Player");
        DodgeController dodge = player.GetComponent<DodgeController>();
        InventorySystem inventory = player.GetComponent<InventorySystem>();
        BossEnemy boss = Object.FindAnyObjectByType<BossEnemy>(FindObjectsInactive.Include);
        BossPhaseController bossPhase = boss.GetComponent<BossPhaseController>();
        ScoreSystem score = Object.FindAnyObjectByType<ScoreSystem>();
        Assert.IsNotNull(telemetry);
        Assert.IsNotNull(player);
        Assert.IsNotNull(dodge);
        Assert.IsNotNull(inventory);
        Assert.IsNotNull(boss);
        Assert.IsNotNull(bossPhase);
        Assert.IsNotNull(score);

        telemetry.ResetRun();
        dodge.ResolveDodgeForTests(RhythmGrade.Perfect, Vector3.forward);
        float perfectCooldown = dodge.LastResolvedCooldown;
        float perfectSpeed = dodge.LastResolvedSpeedMultiplier;
        float perfectInvulnerability = dodge.LastResolvedInvulnerability;
        ResetDodgeCooldown(dodge);

        dodge.ResolveDodgeForTests(RhythmGrade.Good, Vector3.forward);
        float goodCooldown = dodge.LastResolvedCooldown;
        float goodSpeed = dodge.LastResolvedSpeedMultiplier;
        float goodInvulnerability = dodge.LastResolvedInvulnerability;
        ResetDodgeCooldown(dodge);

        dodge.ResolveDodgeForTests(RhythmGrade.Miss, Vector3.forward);
        float missCooldown = dodge.LastResolvedCooldown;
        float missSpeed = dodge.LastResolvedSpeedMultiplier;

        Assert.Less(perfectCooldown, goodCooldown);
        Assert.Less(goodCooldown, missCooldown);
        Assert.Greater(perfectSpeed, goodSpeed);
        Assert.Greater(goodSpeed, missSpeed);
        Assert.Greater(perfectInvulnerability, goodInvulnerability);
        Assert.AreEqual(1, telemetry.Snapshot.perfectDodgeCount);
        Assert.AreEqual(1, telemetry.Snapshot.goodDodgeCount);
        Assert.AreEqual(1, telemetry.Snapshot.missDodgeCount);

        GameObject goldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickups/GoldPickup.prefab");
        Assert.IsNotNull(goldPrefab);
        GameObject pickupObject = Object.Instantiate(goldPrefab, player.transform.position, Quaternion.identity);
        Pickup pickup = pickupObject.GetComponent<Pickup>();
        Assert.IsNotNull(pickup);
        pickup.SendMessage("OnTriggerEnter", player.GetComponent<CharacterController>(), SendMessageOptions.RequireReceiver);
        yield return null;
        Assert.IsTrue(pickup == null, "Pickup should destroy itself after collection.");
        Assert.Greater(inventory.Items.Count, 0);

        boss.gameObject.SetActive(true);
        yield return null;
        AssertRenderableVisual(boss.gameObject, "BossVisual", true);
        boss.TakeDamage(new DamageContext(player, Mathf.CeilToInt(boss.MaxHealth * 0.55f), DamageType.Rhythm, RhythmGrade.Good, true));
        yield return null;
        Assert.IsNotNull(bossPhase.CurrentPhase);
        AssertRenderableVisual(boss.gameObject, "BossVisual", true);

        MeleeEnemy enemy = Object.FindAnyObjectByType<MeleeEnemy>();
        Assert.IsNotNull(enemy);
        int scoreBeforeKill = score.Score;
        int killsBefore = telemetry.Snapshot.enemyKillCount;
        enemy.TakeDamage(new DamageContext(player, 9999, DamageType.Rhythm, RhythmGrade.Perfect, true));
        yield return null;
        Assert.Greater(score.Score, scoreBeforeKill);
        Assert.Greater(telemetry.Snapshot.enemyKillCount, killsBefore);
    }

    [UnityTest]
    public IEnumerator VisualAssetsRenderInVerticalSlice()
    {
        var fatalLogs = new List<string>();
        Application.LogCallback callback = (condition, stackTrace, type) =>
        {
            if (IsFatalStartupLog(condition, type)
                || condition.Contains("disabled renderers")
                || condition.Contains("zero-size renderer bounds")
                || condition.Contains("missing materials"))
            {
                fatalLogs.Add(type + ": " + condition);
            }
        };
        Application.logMessageReceived += callback;

        try
        {
            yield return LoadVerticalSlice();
            for (int i = 0; i < 8; i++)
                yield return null;

            GameObject player = GameObject.FindWithTag("Player");
            Assert.IsNotNull(player);
            Bounds playerBounds = AssertRenderableVisual(player, "WarriorVisual", true);

            MeleeEnemy melee = Object.FindAnyObjectByType<MeleeEnemy>();
            Assert.IsNotNull(melee);
            AssertRenderableVisual(melee.gameObject, "BasicEnemyVisual", true);

            BossEnemy boss = Object.FindAnyObjectByType<BossEnemy>(FindObjectsInactive.Include);
            Assert.IsNotNull(boss);
            boss.gameObject.SetActive(true);
            yield return null;
            AssertRenderableVisual(boss.gameObject, "BossVisual", true);

            Camera mainCamera = Camera.main;
            Assert.IsNotNull(mainCamera);
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
            Assert.IsTrue(GeometryUtility.TestPlanesAABB(planes, playerBounds), "Main camera should be able to see the Warrior visual bounds at startup.");

            AssertDungeonDecorRenderableAndNonBlocking();

            TelemetryDebugOverlay overlay = Object.FindAnyObjectByType<TelemetryDebugOverlay>(FindObjectsInactive.Include);
            Assert.IsNotNull(overlay);
            Assert.DoesNotThrow(() => overlay.Toggle());
            Assert.DoesNotThrow(() => overlay.Toggle());

            Directory.CreateDirectory("Artifacts/VisualValidation");
            ScreenCapture.CaptureScreenshot("Artifacts/VisualValidation/vertical_slice_visual_smoke.png");

            Assert.IsEmpty(fatalLogs);
        }
        finally
        {
            Application.logMessageReceived -= callback;
        }
    }

    [UnityTest]
    public IEnumerator HitPauseRestoresTimeScaleWhenInterrupted()
    {
        Time.timeScale = 1f;
        GameObject go = new GameObject("hit-pause-audit");
        try
        {
            var hitPause = go.AddComponent<HitPause>();

            hitPause.TriggerPause();
            yield return null;
            Assert.AreNotEqual(1f, Time.timeScale);

            go.SetActive(false);
            Assert.AreEqual(1f, Time.timeScale, 0.001f);
        }
        finally
        {
            Time.timeScale = 1f;
            if (go != null)
                Object.Destroy(go);
        }

        yield return null;
        Assert.AreEqual(1f, Time.timeScale, 0.001f);
    }

    private static IEnumerator LoadVerticalSlice()
    {
        Time.timeScale = 1f;
        AsyncOperation load = SceneManager.LoadSceneAsync(StartupScenePath, LoadSceneMode.Single);
        Assert.IsNotNull(load);
        while (!load.isDone)
            yield return null;

        yield return null;
    }

    private static bool IsFatalStartupLog(string condition, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert)
            return true;

        return condition.Contains("NullReferenceException")
            || condition.Contains("MissingReferenceException")
            || condition.Contains("Unable to load scene")
            || condition.Contains("Scene couldn't be loaded")
            || condition.Contains("missing scripts")
            || condition.Contains("Missing Script")
            || condition.Contains("Missing script")
            || condition.Contains("Missing reference")
            || condition.Contains("MissingReference");
    }

    private static Bounds AssertRenderableVisual(GameObject gameplayRoot, string expectedVisualInstanceName, bool requireActiveRenderer)
    {
        Assert.IsNotNull(gameplayRoot, expectedVisualInstanceName + " gameplay root missing.");
        VisualAttachmentRoot binder = gameplayRoot.GetComponent<VisualAttachmentRoot>();
        Assert.IsNotNull(binder, gameplayRoot.name + " missing VisualAttachmentRoot.");
        binder.RefreshReferences();

        Transform visualRoot = binder.VisualRoot;
        Assert.IsNotNull(visualRoot, gameplayRoot.name + " missing VisualRoot reference.");
        Assert.IsNotNull(visualRoot.Find(expectedVisualInstanceName), gameplayRoot.name + " missing " + expectedVisualInstanceName + " child.");
        Assert.LessOrEqual(Vector3.Distance(visualRoot.position, gameplayRoot.transform.position), 1.5f, gameplayRoot.name + " visual root is too far from gameplay root.");
        AssertSaneScale(visualRoot.localScale, gameplayRoot.name + " VisualRoot");

        Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
        Assert.Greater(renderers.Length, 0, gameplayRoot.name + " visual has no renderers.");

        bool hasEnabledRenderer = false;
        bool hasVisibleMaterial = false;
        Bounds combined = new Bounds();
        bool hasBounds = false;
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            Assert.IsNotNull(renderer, gameplayRoot.name + " has a missing renderer.");
            if (renderer.enabled)
                hasEnabledRenderer = true;

            Assert.Greater(renderer.bounds.size.sqrMagnitude, 0.0001f, renderer.name + " has zero-size renderer bounds.");
            if (!hasBounds)
            {
                combined = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                combined.Encapsulate(renderer.bounds);
            }

            Material[] materials = renderer.sharedMaterials;
            Assert.Greater(materials.Length, 0, renderer.name + " has no material slots.");
            for (int j = 0; j < materials.Length; j++)
            {
                Material material = materials[j];
                Assert.IsNotNull(material, renderer.name + " has a missing material slot.");
                if (!material.HasProperty("_Color") || material.color.a > 0.05f)
                    hasVisibleMaterial = true;
            }
        }

        if (requireActiveRenderer)
            Assert.IsTrue(hasEnabledRenderer, gameplayRoot.name + " visual has no enabled renderers.");
        Assert.IsTrue(hasVisibleMaterial, gameplayRoot.name + " visual appears fully transparent.");
        AssertSaneScale(combined.size, gameplayRoot.name + " renderer bounds");
        return combined;
    }

    private static void AssertDungeonDecorRenderableAndNonBlocking()
    {
        GameObject decor = GameObject.Find("DungeonDecor");
        Assert.IsNotNull(decor, "DungeonDecor missing.");
        Renderer[] renderers = decor.GetComponentsInChildren<Renderer>(true);
        Assert.GreaterOrEqual(renderers.Length, 5, "DungeonDecor should contain the selected Tiny Dungeon tiles.");
        for (int i = 0; i < renderers.Length; i++)
        {
            Assert.IsTrue(renderers[i].enabled, renderers[i].name + " decor renderer is disabled.");
            Assert.Greater(renderers[i].bounds.size.sqrMagnitude, 0.0001f, renderers[i].name + " decor bounds are zero.");
            Material[] materials = renderers[i].sharedMaterials;
            for (int j = 0; j < materials.Length; j++)
                Assert.IsNotNull(materials[j], renderers[i].name + " decor material slot is missing.");
        }

        Collider[] colliders = decor.GetComponentsInChildren<Collider>(true);
        Assert.AreEqual(0, colliders.Length, "DungeonDecor must remain non-blocking.");
    }

    private static void AssertSaneScale(Vector3 scale, string label)
    {
        Assert.Greater(scale.x, 0.05f, label + " x scale/bounds too small.");
        Assert.Greater(scale.y, 0.05f, label + " y scale/bounds too small.");
        Assert.Greater(scale.z, 0.05f, label + " z scale/bounds too small.");
        Assert.Less(scale.x, 8f, label + " x scale/bounds too large.");
        Assert.Less(scale.y, 8f, label + " y scale/bounds too large.");
        Assert.Less(scale.z, 8f, label + " z scale/bounds too large.");
    }

    private static void ResetDodgeCooldown(DodgeController dodge)
    {
        FieldInfo cooldownField = typeof(DodgeController).GetField("cooldownRemaining", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(cooldownField, "DodgeController cooldown field was not found.");
        cooldownField.SetValue(dodge, 0f);
    }

    private static Transform SerializedTransform(Object target, string propertyName)
    {
        return new SerializedObject(target).FindProperty(propertyName).objectReferenceValue as Transform;
    }

    private static T SerializedObjectReference<T>(Object target, string propertyName) where T : Object
    {
        return new SerializedObject(target).FindProperty(propertyName).objectReferenceValue as T;
    }
}
#endif
