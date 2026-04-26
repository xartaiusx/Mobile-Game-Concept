# Development Notes

## Current Vertical Slice Setup

Use Unity `6000.4.4f1`. The project uses the built-in render pipeline and the legacy Input Manager.

Open `Assets/Scenes/VerticalSlice.unity` to run the current playable placeholder scene. The scene is enabled in Build Settings.

Unity Play Mode startup is explicitly pinned to `Assets/Scenes/VerticalSlice.unity` by `Game.Editor.VerticalSliceStartup`. This protects the toolbar Play button from running an empty editor scene when Unity's last scene setup is missing or stale. The validator runs on editor load, is also called by vertical slice regeneration, and can be run manually from `Game > Vertical Slice > Validate Startup Scene`.

Phase 9 is a feel calibration and telemetry phase. The goal is to measure the Warrior loop, tune timing/readability, and keep the slice reliable before adding new enemies, classes, narrative systems, or complex UI.

Use `Game > Vertical Slice > Create Or Refresh Vertical Slice` in the Unity Editor to create or refresh the default folders, ScriptableObject assets, prefabs, scene, and Build Settings entry. The menu is expected to be diff-idempotent after the first normalization run: running it twice should not produce meaningful scene, prefab, material, animator, or ScriptableObject churn.

Use `Game > Vertical Slice > Validate Generator Idempotency` after generator changes. The validator runs the generator twice, hashes generated scene/prefab/material/animation/ScriptableObject file contents with normalized line endings, checks required scene objects and references, and fails on duplicate managers, missing scripts, missing generated prefabs, or second-run serialized changes. Hashing is intentionally file-based, so it proves repeatability of committed generated assets but does not explain whether an intentional first-run diff is desirable.

The generated setup includes:

- `PlayerManager`, `GameManager`, `BeatClock`, `RhythmJudgement`, `ScoreSystem`, `ArenaController`, `DifficultyScaler`, and `PickupSpawner`.
- `TelemetryManager` plus a lightweight `TelemetryDebugOverlay`.
- A small primitive arena with boundary walls, follow camera, directional light, melee/ranged enemies, and boss placeholder.
- A scene `Player_Warrior` using the legacy-compatible Fighter component with Warrior stats/name/tuning.
- Default `RhythmConfig`, `ComboProfile`, `AttackTimingData`, Warrior slash ability, boss telegraph data, boss phase data, and item assets under `Assets/ScriptableObjects`.
- Mage, Archer, Healer prefabs/assets remain under `Assets/Prefabs/Player` and `Assets/ScriptableObjects/Abilities` as deferred prototype content.
- `ClassSwapDebugController` remains available as legacy debug code, but it is inactive in `VerticalSlice.unity`.

## Design Pivot

The current prototype is no longer a multi-class test. Do not add Mage, Archer, Healer, class swapping, or multi-class balance work to the active loop unless the direction changes again.

Active gameplay is an endless rhythm-action melee arena:

- Wave starts at 1.
- Warrior survives escalating waves until death.
- Clearing a wave shows a Wave Cleared/Next Wave flow, then starts the next wave after a short delay.
- Every configurable number of waves, default 5, the arena starts a boss/elite encounter.
- Victory is not a terminal state in the active loop.
- Player death is the failure state.
- `R` restarts the run.

## Warrior Combat

Warrior combat is tuned around close-range rhythm mastery:

- `DefaultComboProfile` uses a three-step melee combo with a stronger finisher.
- `DefaultAttackTiming` has quick windup, readable active frames, and short recovery.
- `DefaultRhythmConfig` gives Perfect the highest damage and score, Good moderate output, and Miss weak/no-score output.
- `DefaultRhythmConfig` includes a small early-input bias so mobile and anticipatory inputs feel fair without making late inputs overly loose.
- `DodgeController` rewards Perfect timing with faster movement, shorter cooldown, and better invulnerability.
- `ParryController` rewards Perfect timing by canceling parryable damage and staggering the attacker.
- Fighter remains as the serialized compatibility class; its runtime `CharacterName` is Warrior. `Warrior` exists as an alias class for future prefab migration.

Warrior should feel durable enough to learn the rhythm system, but not safe when the player ignores timing.

## Active And Deferred Class Systems

Active:

- Warrior/Fighter melee prefab and scene instance.
- Warrior Slash ability.
- Combo, dodge, parry, hit reaction, pickups, score, HUD, camera, and boss systems bound to the Warrior player.

Deferred/legacy:

- Mage, Archer, Healer scripts, prefabs, and ability assets.
- Mage/Archer projectile ability paths.
- `ClassSwapDebugController`.

Deferred assets should keep compiling and prefab validation should keep passing, but they should not be active in `VerticalSlice.unity`.

## Endless Arena And Scaling

`ArenaController` owns the endless wave state:

- `Preparing`
- `Wave`
- `WaveCleared`
- `Boss` / `Elite`
- `Failure`

`Victory` remains in the enum for compatibility with older tests/code, but the active loop does not use it as a terminal wave-clear state.

`DifficultyScaler` evaluates each wave and applies capped scaling:

- Enemy count increases every few waves up to a cap.
- Health and damage grow modestly.
- Move speed rises slightly.
- Attack cooldowns shrink slightly down to a floor.
- Boss/elite waves receive extra health, damage, and cadence pressure.
- Pickup generosity slowly decreases.
- Beat speed gradually increases through `BeatClock.SetLevelSpeedMultiplier`.

Early waves should stay readable. Scaling should make rhythm precision more important before it makes enemy stats oppressive.

## Boss And Elite Rhythm Checks

Boss/elite waves should test the core defensive rhythm loop:

- Clear telegraph read.
- Dodge timing opportunity.
- Parry timing opportunity.
- Melee punish window after a Perfect dodge or Perfect parry.

The existing boss slam behavior should remain the stable baseline. Line and radial scaffolding can remain as data/code support, but they are not the main tuning target yet.

## HUD

`VerticalSliceHud` displays:

- Current wave.
- Enemies remaining.
- Boss/elite warning label.
- Score and local high score.
- Rhythm grade feedback.
- Combo, attack state, ability, dodge, parry, boss, inventory, and optional debug readouts.

`TelemetryDebugOverlay` is a separate developer overlay. Toggle it with `F3`; it displays hit/dodge grade counts, current and max combo, average timing offset, and score per minute. It is intentionally text-only and should remain easy to disable or remove for builds.

## Controls

- Move: legacy `Horizontal` and `Vertical` axes.
- Jump: `Jump`.
- Attack: `Fire1`.
- Ability: `Q`.
- Dodge: `Left Shift`.
- Parry: `E`.
- Restart run: `R`.
- Toggle telemetry overlay: `F3`.

Class swap hotkeys are not part of active validation.

## Telemetry

`TelemetryManager` is a runtime service-style singleton. Existing gameplay systems report lightweight events into it:

- `InputBuffer`: rhythm grade and signed timing offset.
- `ComboSystem`: hit grade and combo length.
- `DodgeController`: dodge grade.
- `ScoreSystem`: aggregate score changes.
- `BaseCharacter`: player death and survival time.
- `BaseEnemy`: enemy kill count and kill pacing through the global defeated event.

Telemetry is local-only. On player death or `WriteRunSummary()`, JSON is written to:

`Application.persistentDataPath/Telemetry/run_<timestamp>.json`

When a batch label is set, JSON is written to:

`Application.persistentDataPath/Telemetry/<batchLabel>/run_<timestamp>_<batchLabel>.json`

Batch labels are sanitized to letters, numbers, `_`, and `-`. Leave the batch label empty to preserve the old root-folder behavior. Optional run notes are stored inside the JSON snapshot and are not added to the file name.

Captured fields include schema version, scene name, player class, batch label, run notes, app/Unity versions, run start/end timestamps, run duration, Perfect/Good/Miss hit counts, Perfect/Good/Miss dodge counts, signed timing offsets, average timing offset, time to player death, total survival time, enemy kill count, average time per kill, max combo, average combo length, total score, and score per minute.

Avoid adding per-frame allocations to telemetry. Keep reporting event-driven and write files only at run end or explicit developer request.

## Telemetry Analysis Workflow

Use `Game > Telemetry > Analyze Runs` to open the local editor analyzer. It scans `Application.persistentDataPath/Telemetry`, parses `run_*.json`-style files, reports malformed files without crashing, marks sessions under 20 seconds as short/smoke-test sessions, and produces a copy/paste summary for tuning notes.

The analyzer can:

- Analyze root telemetry only.
- Analyze all batch folders recursively.
- Analyze one selected batch label.
- Choose a custom telemetry or batch folder.
- Set batch label and run notes for the active Play Mode `TelemetryManager`.
- Start a new run, end/write the current run, or clear current run data while in Play Mode.

The in-game telemetry overlay uses `F3` to toggle visibility, `F4` to start a new run, `F5` to end/write a run, and `F6` to clear current run data. Set batch labels and notes from the editor analyzer window before or during Play Mode.

Metric interpretation:

- Perfect hit rate shows how often Warrior attacks land on the tight rhythm window. Initial target: 20%-40%.
- Miss hit rate shows attack timing/readability friction. Initial target: below 25%.
- Perfect dodge rate shows defensive mastery and telegraph clarity. Initial target: 10%-25%.
- Miss dodge rate shows defensive confusion or insufficient input forgiveness. Initial target: below 35%.
- Average timing offset should stay near 0. Earlier than -0.05s means players are anticipating too much; later than +0.05s means feedback/input timing may be lagging.
- Average combo length should sit around 3-6. Consistently low max combo below 3 suggests the loop is breaking before players can build rhythm.
- Early wave survival should usually land around 30-90 seconds for normal-length runs.
- Score-per-minute and enemy-kills-per-minute exclude short sessions from aggregate tuning warnings because smoke tests produce exaggerated rates.

Phase 9.5 tuning loop:

1. Run 5-10 Warrior sessions in `Assets/Scenes/VerticalSlice.unity`.
2. Open `Game > Telemetry > Analyze Runs`.
3. Review warnings and copy the summary into tuning notes if useful.
4. Tune only 2-3 parameters, such as `perfectWindow`, `goodWindow`, early/late bias, dodge timing, telegraph readability, or hit feedback.
5. Repeat the run/analyze/tune cycle.

Warnings include observed metric, target range, and tuning levers to inspect. They are recommendations only; do not auto-apply changes from warnings. High miss rate points to `RhythmConfig.goodWindow`, telegraph duration, and beat alignment; early/late timing bias points to input bias and beat visual alignment; low Perfect dodge points to dodge timing, telegraph readability, and dodge cooldown penalty; low combo points to miss penalty, hit feedback clarity, and enemy interruption timing.

Phase 9.6 5-10 run Warrior batch:

1. Run `Game > Vertical Slice > Create Or Refresh Vertical Slice`.
2. Run `Game > Vertical Slice > Validate Startup Scene`.
3. Play 5-10 normal Warrior runs in `Assets/Scenes/VerticalSlice.unity`; do not mix short smoke sessions into the tuning sample.
4. Open `Game > Telemetry > Analyze Runs`.
5. Copy the analyzer summary into the tuning log.
6. Change no more than 2-3 tuning groups and record old value, new value, rationale, and whether the evidence is real telemetry, smoke telemetry, or static sanity review.
7. Re-run compile/import, tests, startup validation, telemetry analyzer validation, and generator idempotency validation.

## Warrior Telemetry Batch Template

Batch label:
Date:
Build/commit:
Runs:
Target waves:
Changed parameters before batch:
Telemetry summary:
Warnings:
Parameter changes after batch:
Rationale:
Validation:
Next variables to inspect:

Collect 5-10 normal Warrior runs per batch. A valid tuning run should use `Assets/Scenes/VerticalSlice.unity`, player class `Warrior`, accurate scene name, a nonnegative run duration, and enough play time to represent the target wave range. Runs under 20 seconds are short/smoke sessions; keep them for analyzer validation if useful, but exclude them from feel/balance decisions.

Allowed parameter groups per batch:

- Rhythm timing windows and early/late bias.
- Dodge forgiveness, cooldown multipliers, movement, and invulnerability.
- Enemy or boss telegraph readability timing.
- Hit pause or feedback clarity within tiny safe ranges.
- Score/combo rewards only when telemetry clearly shows inconsistency.

Rule: change only 2-3 tuning groups per telemetry batch, record old value, new value, evidence, and rationale, then rerun validation before committing.

## ScriptableObject Assets

Create assets from the Unity create menu:

- `Game/Rhythm/RhythmConfig`: BPM, DSP offset, judgement windows, damage multipliers, score values, and cooldown refund values.
- `Game/Combat/ComboProfile`: ordered melee combo steps and combo timeout.
- `Game/Combat/AttackTimingData`: windup, active, and recovery timing for attacks.
- `Game/Combat/AbilityDefinition`: ability data such as damage, cooldown, range, target mode, and rhythm scaling.
- `Game/Combat/BossTelegraphData`: beat-counted boss warnings and impact settings.
- `Game/AI/BossPhase`: boss phase thresholds and attack pacing modifiers.

## Loot And Pickups

- Defeated enemies can drop `GoldPickup` and `HealthPotionPickup` placeholders through `PickupSpawner`.
- `DifficultyScaler` reduces pickup generosity over time.
- Wave clears grant score; boss/elite clears grant larger score and bonus pickups.
- This is intentionally not a full equipment, economy, or consumable UI system yet.

## Validation

Required commands:

```bash
git diff --check
"$HOME/Unity/Hub/Editor/6000.4.4f1/Editor/Unity" -quit -batchmode -projectPath "$PWD" -logFile /tmp/unity_warrior_endless_compile.log
Scripts/run-unity-tests.sh
"$HOME/Unity/Hub/Editor/6000.4.4f1/Editor/Unity" -batchmode -projectPath "$PWD" -executeMethod Game.Editor.ProjectTestRunner.ValidateStartupSceneCommandLine -quit -logFile /tmp/mobile-game-startup-validation.log
"$HOME/Unity/Hub/Editor/6000.4.4f1/Editor/Unity" -batchmode -projectPath "$PWD" -executeMethod Game.Editor.ProjectTestRunner.ValidateTelemetryAnalyzerCommandLine -quit -logFile /tmp/mobile-game-telemetry-validation.log
"$HOME/Unity/Hub/Editor/6000.4.4f1/Editor/Unity" -batchmode -projectPath "$PWD" -executeMethod Game.Editor.ProjectTestRunner.ValidateGeneratorIdempotencyCommandLine -quit -logFile /tmp/mobile-game-generator-idempotency.log
```

`Scripts/run-unity-tests.sh` invokes `Game.Editor.ProjectTestRunner.RunEditMode` and `Game.Editor.ProjectTestRunner.RunPlayMode`, writes JSON summaries to `TestResults/editmode-summary.json` and `TestResults/playmode-summary.json`, writes `TestResults/summary.txt`, prints totals, and exits nonzero when summaries are missing, Unity exits nonzero, log scans find compile/null/missing-reference markers, or tests fail.

The JSON summaries are the authoritative test report. `-testResults` XML may still be emitted for compatibility, but CI and local validation should not depend on XML as the only source of truth.

Current expected discovery after Phase 9 telemetry:

- EditMode discovers the core system and prefab validation tests.
- EditMode also covers telemetry analysis parsing, malformed-file handling, short-session filtering, warning generation, healthy samples, and editor/runtime separation.
- PlayMode discovers the rhythm judgement test, vertical slice smoke tests, and telemetry smoke test.
- Both modes should remain separated by `Game.Tests.EditMode.asmdef` and `Game.Tests.PlayMode.asmdef`.

The startup smoke test is `VerticalSlicePlayModeTests.EditorPlayButtonStartupSceneLoadsPlayableVerticalSlice`. It calls the startup validator, checks the configured Play Mode start scene path, loads the actual vertical slice scene by path, waits several frames, asserts one player/camera/HUD/rhythm/score/telemetry/arena path exists, confirms `Time.timeScale` is restored to `1`, and fails on fatal startup log markers.

`TestResults/summary.txt` is the quickest human-readable status. The two JSON files are better for CI parsing because they include totals and failure messages per mode.

Warrior validation artifacts should go under `Artifacts/WarriorEndless/`. Do not commit `Artifacts/`.

Environment note: Unity licensing handshake/curl messages can appear in batchmode logs without failing validation. Compile errors, `NullReferenceException`, `MissingReferenceException`, missing scripts/references, duplicate singleton warnings, scene load failures, and test failures should still be treated as failures.

## Tuning Log

### Phase 9.6 - 2026-04-26

Evidence source: static sanity review of existing tuning assets, tests, and telemetry analyzer targets. No human playtest telemetry was generated or claimed for this pass.

Changed parameters:

- `DefaultRhythmConfig.goodWindow`: `0.10s` to `0.11s`. Rationale: small miss-rate forgiveness increase while leaving the Perfect target unchanged.
- `DefaultRhythmConfig.earlyInputBiasSeconds`: `0.015s` to `0.018s`. Rationale: slight mobile/anticipatory input support without heavily favoring early presses.
- `DefaultRhythmConfig.lateInputBiasSeconds`: `0.005s` to `0.007s`. Rationale: tiny late-input tolerance increase to avoid over-penalizing input/display latency.
- `BossLineTelegraph.beatsBeforeImpact`: `3` to `4`. Rationale: the line pattern is faster and more spatially specific than slam, so the first readability pass gives one additional beat before impact.

Validation result: generator idempotency validation passes after the determinism patch. Full Phase 9.6 validation should include compile/import, EditMode, PlayMode, startup scene validation, telemetry analyzer validation, generator idempotency validation, and log scans.

Next variables to inspect after real playtest telemetry: Perfect hit rate, miss hit rate, average signed timing offset, Perfect/Miss dodge rates during boss/elite warnings, average combo length, and early wave survival time.

## Known Limitations

- Art, animation clips, and audio remain placeholder.
- The Fighter script is still the serialized active component for prefab compatibility, even though gameplay names/treats it as Warrior.
- The active slice uses one scene-level `RhythmJudgement`; player input buffers and combo systems fall back to it when they do not have a local judgement reference.
- Toolbar Play is expected to start `Assets/Scenes/VerticalSlice.unity`; if it does not, run `Game > Vertical Slice > Validate Startup Scene` and then the startup PlayMode smoke test.
- Telemetry writes local developer JSON only. It is not network analytics, privacy tooling, or production reporting.
- Short automated telemetry smoke tests can produce exaggerated score-per-minute values because the run duration is intentionally tiny.
- Generator idempotency validation proves the second run is stable, but intentional first-run generator changes still need normal code review.
- Phase 9.6 tuning is not human-validated yet; avoid a second balance pass until a real 5-10 run Warrior batch exists.
- Mage, Archer, Healer, and class swap are preserved as deferred content and should not be treated as current gameplay.
- Projectile ability code remains for inactive classes and ranged enemies; Warrior gameplay should not depend on it.
- Boss line/radial patterns are scaffolding. Slam is the stable baseline.
- Pickup and feedback effects still use simple `Instantiate` paths in low-frequency cases; pool them before mobile stress testing.
- The HUD uses legacy `UnityEngine.UI` placeholders.

## Recommended Next Sequence

1. Run a real 5-10 session Warrior telemetry batch and compare analyzer output against the Phase 9.6 tuning log before changing more balance.
2. Add real placeholder Warrior attack, dodge, and parry clips that call the existing animation event relay methods.
3. Improve boss/elite telegraph readability and punish-window feedback.
4. Add mobile touch controls for attack, dodge, parry, ability, restart, and optional developer overlay access.
5. Pool feedback and pickup effects before device profiling.
