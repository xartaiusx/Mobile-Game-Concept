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

When a batch label is set, runs are written to:

`Application.persistentDataPath/Telemetry/<batchLabel>/run_<timestamp>_<batchLabel>.json`

Captured metrics include batch label, optional run notes, Perfect/Good/Miss hit and dodge counts, signed input timing offsets, survival time, enemy kills, average time per kill, max/average combo, total score, and score per minute.

The in-game telemetry overlay is text-only and can be toggled with `F3`. It shows live hit/dodge grade counts, current/max combo, average timing bias, score per minute, and current batch label. Developer hotkeys are `F4` start new run, `F5` end/write run, and `F6` clear current run data.

To review local run data in the Unity Editor, open:

`Game > Telemetry > Analyze Runs`

The analyzer reads local JSON files from the telemetry folder, can analyze the root folder, all batch folders, or a selected batch folder, ignores malformed files safely, marks sessions under 20 seconds as short/smoke-test sessions, and summarizes aggregate Warrior tuning signals:

- Perfect hit rate: target 20%-40%.
- Miss hit rate: target below 25%.
- Perfect dodge rate: target 10%-25%.
- Miss dodge rate: target below 35%.
- Average timing offset: target near 0; earlier than -0.05s or later than +0.05s needs attention.
- Average combo length: target 3-6.
- Early wave survival: target 30-90 seconds for normal runs.

Use the Phase 9.7 tuning loop: set a batch label such as `warrior_batch_001`, run 5-10 Warrior sessions, open the telemetry analyzer, review warnings, tune only 2-3 parameter groups such as rhythm windows, dodge forgiveness, telegraph readability, or feedback clarity, then repeat. Score-per-minute and kill-rate aggregates exclude short sessions because smoke tests can distort those values.

Phase 9.6 first-pass tuning was based on static sanity review and existing smoke/test telemetry plumbing, not human playtest results. Changes were intentionally narrow:

- `DefaultRhythmConfig.goodWindow`: `0.10s` to `0.11s`.
- `DefaultRhythmConfig.earlyInputBiasSeconds`: `0.015s` to `0.018s`.
- `DefaultRhythmConfig.lateInputBiasSeconds`: `0.005s` to `0.007s`.
- `BossLineTelegraph.beatsBeforeImpact`: `3` to `4`.

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

This creates or refreshes the default folders, ScriptableObject assets, prefabs, scene objects, and Build Settings entry with the Warrior-only active path. The generator is expected to be diff-idempotent after the first normalization run: running it twice in a row should not produce meaningful scene, prefab, material, animator, or ScriptableObject changes.

Validate generator determinism with:

`Game > Vertical Slice > Validate Generator Idempotency`

The validator runs the generator twice, hashes generated scene/prefab/material/animation/ScriptableObject files with normalized line endings, checks required scene objects/references, and fails on duplicate generated managers, missing scripts, or duplicate generated assets. It does not replace human review of intentional first-run generator changes.

The intended Play Mode startup scene is always:

`Assets/Scenes/VerticalSlice.unity`

Editor startup validation pins Unity's toolbar Play button to that scene through `EditorSceneManager.playModeStartScene` and keeps it enabled in Build Settings. You can re-check this with:

`Game > Vertical Slice > Validate Startup Scene`

If the editor opens with no active scene, pressing Play should still enter the generated vertical slice. Opening `VerticalSlice.unity` manually is fine, but it is no longer required for toolbar Play validation.

## Free Assets

Imported free assets are documented in `Assets/ThirdParty/FreeAssets/ASSET_CREDITS.md`.

- Kenney Prototype Textures: CC0, commercial use allowed, attribution not required.
- Kenney Particle Pack: CC0, commercial use allowed, attribution not required.

## Visual Asset Imports

Phase 9.8 adds an optional visual scaffold. Gameplay scripts, colliders, health, combat, rhythm, score, and telemetry stay on existing gameplay roots. Third-party models and animations must be mounted as children under `VisualRoot` through visual wrapper prefabs or `VisualAttachmentRoot`; missing visuals must not block startup or tests.

Approved first-pass sources:

- KayKit Adventurers: https://kaylousberg.itch.io/kaykit-adventurers
- KayKit Character Animations: https://kaylousberg.itch.io/kaykit-character-animations
- Kenney Tiny Dungeon: https://kenney.nl/assets/tiny-dungeon
- Mixamo fallback only when needed: https://helpx.adobe.com/creative-cloud/faq/mixamo-faq.html

Import workflow:

1. Download the asset pack manually.
2. Import only needed FBX/GLTF/models/animations/props.
3. Place source assets under the matching `Assets/ThirdParty` folder.
4. Preserve or update `Assets/ThirdParty/Licenses/ASSET_LICENSES.md`.
5. Assign one selected model to the matching visual prefab under `Assets/Art/Prefabs`.
6. Run `Game > Visuals > Validate Visual Asset Setup`.
7. Run the normal validation suite.

Current intended first art pass: one Warrior model, one basic enemy model, one boss placeholder model, idle/run/attack/hit/death animations, and 3-5 dungeon props. Do not commit unused full packs.

Phase 9.9 selected the first visual pass from the manual import:

- Warrior visual: KayKit `Knight.fbx`
- Basic enemy visual: KayKit `Rogue.fbx`
- Boss placeholder visual: KayKit `Barbarian.fbx`
- Animation clips wired where compatible: `Idle_A`, `Running_A`, `Hit_A`, `Death_A`
- Additional available clip documented but not currently wired: `Walking_A`
- Attack and dodge/evade visual controller states remain placeholder-only until compatible clips are selected.
- Kenney decorative tiles in the generated arena: `tile_0000`, `tile_0001`, `tile_0002`, `tile_0016`, `tile_0017`

Imported KayKit screenshots and URL shortcuts are quarantined under `Assets/ThirdParty/_Documentation`. Duplicate source formats remain in ThirdParty for now and are candidates for manual pruning after the art pass is visually approved.

## Validation

```bash
git diff --check
"$HOME/Unity/Hub/Editor/6000.4.4f1/Editor/Unity" -quit -batchmode -projectPath "$PWD" -logFile /tmp/unity_warrior_endless_compile.log
Scripts/run-unity-tests.sh
"$HOME/Unity/Hub/Editor/6000.4.4f1/Editor/Unity" -batchmode -projectPath "$PWD" -executeMethod Game.Editor.ProjectTestRunner.ValidateStartupSceneCommandLine -quit -logFile /tmp/mobile-game-startup-validation.log
"$HOME/Unity/Hub/Editor/6000.4.4f1/Editor/Unity" -batchmode -projectPath "$PWD" -executeMethod Game.Editor.ProjectTestRunner.ValidateTelemetryAnalyzerCommandLine -quit -logFile /tmp/mobile-game-telemetry-validation.log
"$HOME/Unity/Hub/Editor/6000.4.4f1/Editor/Unity" -batchmode -projectPath "$PWD" -executeMethod Game.Editor.ProjectTestRunner.ValidateGeneratorIdempotencyCommandLine -quit -logFile /tmp/mobile-game-generator-idempotency.log
"$HOME/Unity/Hub/Editor/6000.4.4f1/Editor/Unity" -batchmode -projectPath "$PWD" -executeMethod Game.Editor.ProjectTestRunner.ValidateVisualAssetSetupCommandLine -quit -logFile /tmp/mobile-game-visual-validation.log
```

The PlayMode suite includes a startup smoke regression test that validates the same scene path used by Unity Play Mode, waits several frames, checks the player, camera, HUD, rhythm, score, telemetry, arena/enemy roots, verifies `Time.timeScale == 1`, and fails on fatal startup logs such as null references, missing references, missing scripts, or scene load failures.

Telemetry analysis has EditMode coverage for JSON parsing, malformed-file handling, derived metric math, short-session filtering, target-range warnings, and editor/runtime assembly separation.

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
- Phase 9.6 tuning needs real 5-10 run Warrior telemetry before further balance changes.
- Third-party art has not been imported yet; visual wrapper prefabs are safe placeholders only.

## Next

Next phase should be a hands-on Warrior telemetry batch: 5-10 normal runs, analyzer summary capture, then a second narrow pass on rhythm bias, dodge forgiveness, telegraph readability, or early wave pacing based on measured misses and survival.
