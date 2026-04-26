# Development Notes

## Current Vertical Slice Setup

Use Unity `6000.4.4f1`. The project uses the built-in render pipeline and the legacy Input Manager.

Open `Assets/Scenes/VerticalSlice.unity` to run the current playable placeholder scene. The scene is enabled in Build Settings.

Use `Game > Vertical Slice > Create Or Refresh Vertical Slice` in the Unity Editor to recreate the default folders, ScriptableObject assets, prefabs, scene, and Build Settings entry. The menu is idempotent: it refreshes the known default assets and saves the scene at `Assets/Scenes/VerticalSlice.unity`.

The generated setup includes:

- `PlayerManager`, `GameManager`, and a rhythm system object.
- A small primitive arena with boundary walls, follow camera, and directional light.
- Default `RhythmConfig`, `ComboProfile`, `AttackTimingData`, class ability assets, boss telegraph data, and item assets under `Assets/ScriptableObjects`.
- Player prefabs for Fighter, Mage, Archer, and Healer under `Assets/Prefabs/Player`.
- Melee, ranged, and boss prefabs under `Assets/Prefabs/Enemies`.
- A pooled projectile prefab under `Assets/Prefabs/Projectiles`.
- A scene Fighter player, melee enemy, ranged enemy, boss, basic enemy spawner, and readable rhythm-combat HUD.
- Placeholder feedback prefabs and materials under `Assets/Prefabs/Feedback` and `Assets/Materials/Feedback`.

## Vertical Slice UI

The scene includes `Assets/Prefabs/UI/VerticalSliceHUD.prefab`, backed by `BeatBarUI` and `VerticalSliceHud`.

- Beat bar: shows beat phase with a center timing window.
- Feedback text: reports `Perfect`, `Good`, or `Miss` results from attacks, abilities, dodges, and parries.
- Combo text: shows the current combo step count.
- Ability text: shows the first equipped ability and cooldown.
- Dodge/parry text: shows cooldown and active/invulnerable state.
- Boss text: shows boss telegraph countdown and impact status.
- Attack text: shows `Ready`, `Windup`, `Active`, or `Recovery`.
- Debug text: optional runtime readout for beat, last grade, attack state, player health, and enemy count.

## Phase 4 Tuning Notes

The current values were tuned with an automated desktop play loop on Linux using Unity GUI Play Mode, `xdotool` input, and screenshot capture under `Artifacts/Phase4Frames`.

- Fighter health is 96 HP in the default slice, giving enough time to test dodge/parry and combo flow.
- Enemy pressure is intentionally moderate: melee and ranged enemies hit lightly, attack more slowly, and the scene spawner is capped for readability.
- The follow camera sits higher and farther back so movement near arena edges remains visible.
- The HUD is compact enough for the default Game view and keeps rhythm, combo, cooldown, and debug feedback readable.
- Boss slam damage/radius are reduced for the tuning slice; authored VFX/audio and stronger phase tuning belong in the next polish phase.

## Phase 5 Feedback And Animation Hooks

Phase 5 keeps the same combat architecture and adds readable placeholder polish:

- `RhythmFeedbackController` now drives clearer grade, dodge, parry, boss warning, boss impact, player damage, and enemy defeat audio/VFX events.
- `AudioCueDefinition` and `AudioCuePlayer` provide optional generated tone cues with safe null handling. Replace these assets with authored clips later without changing combat events.
- `CombatAnimationBridge` and `AnimationEventRelay` let future animation clips call `BeginAttackActiveWindow`, `EndAttackActiveWindow`, `FinishRecovery`, `TriggerFootstep`, and `TriggerWeaponSwing`.
- Player prefabs include the animation bridge/relay even without Animator controllers; if an Animator is added, the bridge sets `IsMoving`, `AttackState`, `RhythmGrade`, `IsDodging`, and `IsParrying`.
- Enemy attack starts spawn `EnemyWindupFlash`, and ranged projectiles use a lightweight `TrailRenderer` for screenshot readability.
- Boss slam warnings use `BossTelegraphWarningRing`, pulse by beat, play optional warning/impact cues, and keep HUD countdown text visible.

Generated Phase 5 assets:

- VFX: `PerfectHitPulse`, `GoodHitPulse`, `MissHitPulse`, `DodgePulse`, `ParryPulse`, `ParrySuccessBurst`, `BossTelegraphWarningRing`, `BossImpactBurst`, `EnemyWindupFlash`, `ProjectileTrailPlaceholder`.
- Audio: `PerfectHit`, `GoodHit`, `Miss`, `Dodge`, `ParrySuccess`, `BossWarning`, `BossImpact`, `PlayerDamage`, `EnemyDefeated`, `Footstep`, `WeaponSwing`.

## Class Ability Defaults

- Fighter Slash: short-range forward cone damage. Perfect timing has the strongest damage scaling and briefly staggers enemies.
- Mage Bolt: medium-range target-point spell. It hits harder than the basic combo but has a longer cooldown.
- Archer Shot: long-range precision hit. Perfect timing has the highest damage multiplier, while misses are heavily reduced.
- Healer Pulse: self heal. Perfect timing adds bonus healing and a brief protection window.

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
- `Game/Combat/AttackTimingData`: windup, active, and recovery timing for animator-ready attacks.
- `Game/Combat/AbilityDefinition`: class ability data such as damage, healing, cost, range, target mode, and rhythm scaling.
- `Game/Combat/BossTelegraphData`: beat-counted boss warnings and impact settings.
- `Game/AI/BossPhase`: boss phase thresholds and attack pacing modifiers.

## Rhythm Combat Rules

- Attacks are buffered through `InputBuffer` and resolved on `BeatClock.OnBeat`.
- `RhythmJudgement` maps timing deltas to `Perfect`, `Good`, or `Miss`.
- Combos use grade multipliers for damage and cooldown refunds.
- Combo attacks now move through windup, active, and recovery states before returning to ready.
- Abilities use per-ability rhythm scaling for damage or healing.
- Dodges use the rhythm grade to adjust invulnerability and recovery.
- Parries use the rhythm grade to cancel or reduce incoming parryable damage.
- Boss telegraphs count down by beats before applying `DamageContext`.
- `RhythmFeedbackController` listens to combat events and spawns placeholder pulses for attack grades, dodge/parry events, boss warnings, and boss impacts.

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
- Boss telegraph and combat feedback use authored placeholder primitives and generated tones; final VFX/audio are still needed.
- No equipment UI, skill trees, addressables, Cinemachine, or multiplayer are included yet.
- The batchmode Test Runner exits successfully in this environment but does not currently emit XML result files; use the Unity Test Runner window for detailed per-test reporting if needed.
- The HUD uses legacy `UnityEngine.UI` placeholders; replace with final UI art/layout after combat feel is stable.

## Recommended Next Sequence

1. Do a human keyboard/controller pass in `Assets/Scenes/VerticalSlice.unity` to confirm feel beyond automated input.
2. Implement true projectile-style Mage/Archer abilities using the existing projectile/pool path.
3. Tune one ability per class against the default rhythm windows.
4. Add a simple pickup/loot loop and arena win/loss condition.
5. Connect real animation clips/events to `AttackTimingData` and combo timing states.
6. Add mobile UI buttons for attack, ability, dodge, and parry after keyboard/controller feel is stable.
7. Profile on Android/iOS and tune pooling, physics masks, and input latency.
