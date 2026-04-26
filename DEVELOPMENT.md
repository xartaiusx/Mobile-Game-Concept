# Development Notes

## Current Vertical Slice Setup

Use Unity `6000.4.4f1`. The project uses the built-in render pipeline and the legacy Input Manager.

Open `Assets/Scenes/VerticalSlice.unity` to run the current playable placeholder scene. The scene is enabled in Build Settings.

Use `Game > Vertical Slice > Create Or Refresh Vertical Slice` in the Unity Editor to recreate the default folders, ScriptableObject assets, prefabs, scene, and Build Settings entry. The menu is idempotent and now rebuilds the active path around Warrior-only endless arena play.

The generated setup includes:

- `PlayerManager`, `GameManager`, `BeatClock`, `RhythmJudgement`, `ScoreSystem`, `ArenaController`, `DifficultyScaler`, and `PickupSpawner`.
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

## Controls

- Move: legacy `Horizontal` and `Vertical` axes.
- Jump: `Jump`.
- Attack: `Fire1`.
- Ability: `Q`.
- Dodge: `Left Shift`.
- Parry: `E`.
- Restart run: `R`.

Class swap hotkeys are not part of active validation.

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
```

`Scripts/run-unity-tests.sh` invokes `Game.Editor.ProjectTestRunner.RunEditMode` and `Game.Editor.ProjectTestRunner.RunPlayMode`, writes JSON summaries to `TestResults/editmode-summary.json` and `TestResults/playmode-summary.json`, writes `TestResults/summary.txt`, prints totals, and exits nonzero when summaries are missing, Unity exits nonzero, log scans find compile/null/missing-reference markers, or tests fail.

The JSON summaries are the authoritative test report. `-testResults` XML may still be emitted for compatibility, but CI and local validation should not depend on XML as the only source of truth.

Warrior validation artifacts should go under `Artifacts/WarriorEndless/`. Do not commit `Artifacts/`.

Environment note: Unity licensing handshake/curl messages can appear in batchmode logs without failing validation. Compile errors, `NullReferenceException`, `MissingReferenceException`, missing scripts/references, duplicate singleton warnings, scene load failures, and test failures should still be treated as failures.

## Known Limitations

- Art, animation clips, and audio remain placeholder.
- The Fighter script is still the serialized active component for prefab compatibility, even though gameplay names/treats it as Warrior.
- Mage, Archer, Healer, and class swap are preserved as deferred content and should not be treated as current gameplay.
- Projectile ability code remains for inactive classes and ranged enemies; Warrior gameplay should not depend on it.
- Boss line/radial patterns are scaffolding. Slam is the stable baseline.
- Pickup and feedback effects still use simple `Instantiate` paths in low-frequency cases; pool them before mobile stress testing.
- The HUD uses legacy `UnityEngine.UI` placeholders.

## Recommended Next Sequence

1. Do a hands-on Warrior pass through waves 1-10 and tune enemy count, pickup scarcity, and beat speed.
2. Add real placeholder Warrior attack, dodge, and parry clips that call the existing animation event relay methods.
3. Improve boss/elite telegraph readability and punish-window feedback.
4. Add mobile touch controls for attack, dodge, parry, ability, and restart.
5. Pool feedback and pickup effects before device profiling.
