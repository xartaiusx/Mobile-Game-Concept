# Development Notes

## Current Vertical Slice Setup

Use Unity `6000.4.4f1`. The project uses the built-in render pipeline and the legacy Input Manager.

Open `Assets/Scenes/VerticalSlice.unity` to run the current playable placeholder scene. The scene is enabled in Build Settings.

Use `Game > Vertical Slice > Create Or Refresh Vertical Slice` in the Unity Editor to recreate the default folders, ScriptableObject assets, prefabs, scene, and Build Settings entry. The menu is idempotent: it refreshes the known default assets and saves the scene at `Assets/Scenes/VerticalSlice.unity`.

The generated setup includes:

- `PlayerManager`, `GameManager`, and a rhythm system object.
- A small primitive arena with boundary walls, follow camera, and directional light.
- Default `RhythmConfig`, `ComboProfile`, class ability assets, boss telegraph data, and item assets under `Assets/ScriptableObjects`.
- Player prefabs for Fighter, Mage, Archer, and Healer under `Assets/Prefabs/Player`.
- Melee, ranged, and boss prefabs under `Assets/Prefabs/Enemies`.
- A pooled projectile prefab under `Assets/Prefabs/Projectiles`.
- A scene Fighter player, melee enemy, ranged enemy, boss, basic enemy spawner, and readable rhythm-combat HUD.

## Vertical Slice UI

The scene includes `Assets/Prefabs/UI/VerticalSliceHUD.prefab`, backed by `BeatBarUI` and `VerticalSliceHud`.

- Beat bar: shows beat phase with a center timing window.
- Feedback text: reports `Perfect`, `Good`, or `Miss` results from attacks, abilities, dodges, and parries.
- Combo text: shows the current combo step count.
- Ability text: shows the first equipped ability and cooldown.
- Dodge/parry text: shows cooldown and active/invulnerable state.
- Boss text: shows boss telegraph countdown and impact status.
- Debug text: optional runtime readout for beat, last grade, player health, and enemy count.

## Controls

- Move: legacy `Horizontal` and `Vertical` axes.
- Jump: `Jump`.
- Attack: `Fire1`.
- Ability: `Q`.
- Dodge: `Left Shift`.
- Parry: `E`.

## ScriptableObject Assets

Create assets from the Unity create menu:

- `Game/Rhythm/RhythmConfig`: BPM, DSP offset, judgement windows, damage multipliers, and cooldown refund values.
- `Game/Combat/ComboProfile`: ordered melee combo steps and combo timeout.
- `Game/Combat/AbilityDefinition`: class ability data such as damage, healing, cost, range, target mode, and rhythm scaling.
- `Game/Combat/BossTelegraphData`: beat-counted boss warnings and impact settings.
- `Game/AI/BossPhase`: boss phase thresholds and attack pacing modifiers.

## Rhythm Combat Rules

- Attacks are buffered through `InputBuffer` and resolved on `BeatClock.OnBeat`.
- `RhythmJudgement` maps timing deltas to `Perfect`, `Good`, or `Miss`.
- Combos use grade multipliers for damage and cooldown refunds.
- Abilities use per-ability rhythm scaling for damage or healing.
- Dodges use the rhythm grade to adjust invulnerability and recovery.
- Parries use the rhythm grade to cancel or reduce incoming parryable damage.
- Boss telegraphs count down by beats before applying `DamageContext`.

## Projectile Path

Use `PoolableProjectile` with `ObjectPool` for gameplay prefabs on mobile. `Projectile` remains as a non-pooled fallback for quick prototypes. Ranged enemies and bosses both prefer an assigned `ObjectPool` and fall back to `Instantiate` only when no pool is configured.

## Tests

Batchmode compile:

```bash
"$HOME/Unity/Hub/Editor/6000.4.4f1/Editor/Unity" -quit -batchmode -projectPath "$PWD" -logFile -
```

EditMode tests:

```bash
"$HOME/Unity/Hub/Editor/6000.4.4f1/Editor/Unity" -quit -batchmode -projectPath "$PWD" -runTests -testPlatform EditMode
```

PlayMode tests:

```bash
"$HOME/Unity/Hub/Editor/6000.4.4f1/Editor/Unity" -quit -batchmode -projectPath "$PWD" -runTests -testPlatform PlayMode
```

## Known Limitations

- Placeholder keyboard input is included for local testing; mobile UI buttons should call `RequestAbility`, `RequestDodge`, and `RequestParry`.
- Ability targeting is intentionally simple and should be expanded with animation events, lock-on, or touch targeting later.
- Boss telegraph visuals and audio cues are optional and still need authored assets.
- No equipment UI, skill trees, addressables, Cinemachine, or multiplayer are included yet.
- The batchmode Test Runner exits successfully in this environment but does not currently emit XML result files; use the Unity Test Runner window for detailed per-test reporting if needed.
- The HUD uses legacy `UnityEngine.UI` placeholders; replace with final UI art/layout after combat feel is stable.

## Recommended Next Sequence

1. Manually play `Assets/Scenes/VerticalSlice.unity` and tune hit ranges, enemy spacing, and boss telegraph cadence.
2. Add simple VFX/audio cues for beat hits, dodge invulnerability, parry success, and boss impact.
3. Replace placeholder primitives with real prefabs and animator-driven attacks.
4. Tune one ability per class against the default rhythm windows.
5. Add mobile UI buttons for attack, ability, dodge, and parry after keyboard/controller feel is stable.
6. Profile on Android/iOS and tune pooling, physics masks, and input latency.
