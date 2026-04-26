# Mobile Game Concept

Unity mobile rhythm-action prototype focused on one playable class: Warrior.

The current vertical slice targets Unity `6000.4.4f1`, the built-in render pipeline, and the legacy Input Manager. The main playable scene is:

`Assets/Scenes/VerticalSlice.unity`

For detailed setup, asset, and test notes, see [DEVELOPMENT.md](DEVELOPMENT.md).

## Current Direction

The prototype has pivoted away from multi-class balance. Warrior is the only active playable class for the current slice, using the old Fighter implementation as a compatibility base while exposing Warrior naming and tuning in gameplay.

Mage, Archer, Healer, projectile ability assets, and class swap code remain in the repository as deferred prototype content. They should compile and remain valid as assets, but they are not part of the active `VerticalSlice.unity` gameplay path.

## Current Slice

- Phase 9 goal: measure Warrior feel, tune rhythm windows/dodge feedback, and keep the vertical slice stable before adding content.
- Endless rhythm-action melee arena loop.
- Warrior-only active player path through `GameManager`, `PlayerManager`, HUD, camera, arena, score, dodge, parry, pickups, and boss systems.
- Close-range combo attacks using `ComboSystem`, `DefaultComboProfile`, and `DefaultAttackTiming`.
- Rhythm precision rewards: Perfect hits deal and score the most, Good hits are moderate, Miss hits are weak and score little or nothing.
- Perfect dodge moves faster, cools down sooner, and grants stronger invulnerability.
- Perfect parry cancels parryable damage and staggers enemies for a counter opportunity.
- Scalable endless waves with short next-wave delay, no terminal victory after wave 1, and failure on player death.
- Boss/elite warning waves every 5 waves by default.
- HUD shows wave, score, high score, enemies remaining, boss/elite warning, cooldowns, and rhythm grade feedback.
- `TelemetryManager` captures run-level hit, dodge, timing, combo, score, survival, and kill pacing metrics.

## Controls

- Move: `Horizontal` / `Vertical`
- Jump: `Jump`
- Attack: `Fire1`
- Ability: `Q` for the active Warrior slash ability
- Dodge: `Left Shift`
- Parry: `E`
- Restart run: `R`
- Toggle telemetry overlay: `F3`

`ClassSwapDebugController` is inactive in `VerticalSlice.unity`. Do not use Mage, Archer, Healer, or class swapping for active prototype validation.

## Telemetry

Runtime telemetry is local-only and writes human-readable JSON to:

`Application.persistentDataPath/Telemetry/run_<timestamp>.json`

Captured metrics include Perfect/Good/Miss hit and dodge counts, signed input timing offsets, survival time, enemy kills, average time per kill, max/average combo, total score, and score per minute.

The in-game telemetry overlay is text-only and can be toggled with `F3`. It shows live hit/dodge grade counts, current/max combo, average timing bias, and score per minute.

## Endless Scaling

`DifficultyScaler` drives wave-based scaling with conservative caps:

- Enemy count increases every few waves.
- Enemy health and damage increase modestly.
- Enemy move speed rises slightly.
- Enemy attack cooldown tightens slightly.
- Boss/elite waves get extra health, damage, and cadence pressure.
- Pickup generosity decreases gradually.
- Beat speed increases slowly and caps so skill matters more than stat inflation.

## Scoring

`ScoreSystem` awards rhythm-hit score by grade, combo bonus, wave clear bonus, boss/elite bonus, and a local high score through `PlayerPrefs`.

This is not a progression economy yet. There are no skill trees, currencies, upgrades, or long-term unlocks in the active prototype.

## Bootstrap

Open the project in Unity `6000.4.4f1`, then use:

`Game > Vertical Slice > Create Or Refresh Vertical Slice`

This recreates the default folders, ScriptableObject assets, prefabs, scene objects, and Build Settings entry with the Warrior-only active path.

The intended Play Mode startup scene is always:

`Assets/Scenes/VerticalSlice.unity`

Editor startup validation pins Unity's toolbar Play button to that scene through `EditorSceneManager.playModeStartScene` and keeps it enabled in Build Settings. You can re-check this with:

`Game > Vertical Slice > Validate Startup Scene`

If the editor opens with no active scene, pressing Play should still enter the generated vertical slice. Opening `VerticalSlice.unity` manually is fine, but it is no longer required for toolbar Play validation.

## Free Assets

Imported free assets are documented in `Assets/ThirdParty/FreeAssets/ASSET_CREDITS.md`.

- Kenney Prototype Textures: CC0, commercial use allowed, attribution not required.
- Kenney Particle Pack: CC0, commercial use allowed, attribution not required.

## Validation

```bash
git diff --check
"$HOME/Unity/Hub/Editor/6000.4.4f1/Editor/Unity" -quit -batchmode -projectPath "$PWD" -logFile /tmp/unity_warrior_endless_compile.log
Scripts/run-unity-tests.sh
```

The PlayMode suite includes a startup smoke regression test that validates the same scene path used by Unity Play Mode, waits several frames, checks the player, camera, HUD, rhythm, score, telemetry, arena/enemy roots, verifies `Time.timeScale == 1`, and fails on fatal startup logs such as null references, missing references, missing scripts, or scene load failures.

`Scripts/run-unity-tests.sh` writes authoritative summaries to:

- `TestResults/editmode-summary.json`: EditMode totals, failures, and messages.
- `TestResults/playmode-summary.json`: PlayMode totals, failures, and messages.
- `TestResults/summary.txt`: combined human-readable totals.

The custom summaries are the source of truth. Unity XML output may exist for compatibility, but validation should not depend on XML alone.

## Current Risks

- Combat, animation, and UI assets are still placeholder-grade.
- Boss/elite cadence works as a scaffold, but wave 5+ pacing still needs hands-on tuning.
- Touch controls and device profiling are not done.
- Telemetry is local developer instrumentation, not a production analytics service.
- Some deferred Mage, Archer, Healer, projectile, and class-swap assets still exist for compile compatibility only.

## Next

Next phase should be a hands-on Warrior feel pass: melee hit readability, boss/elite rhythm telegraphs, arena pacing over waves 1-10, mobile touch controls, and pooled feedback/pickup effects for mobile stress testing.
