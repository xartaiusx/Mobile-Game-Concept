# Development Notes

## Current Vertical Slice Setup

This repository is still script-first: it does not include Unity-generated `.meta` files, scenes, prefabs, `Packages/manifest.json`, or `ProjectSettings`. Open the folder in Unity 2021 LTS or newer and let Unity generate project metadata before saving scenes or assets.

Use `Game > Vertical Slice > Create Test Scene` in the Unity Editor to create a placeholder scene with:

- `PlayerManager`, `GameManager`, and a rhythm system object.
- A default `RhythmConfig`, `ComboProfile`, `AbilityDefinition`, and `BossTelegraphData` under `Assets/VerticalSlice`.
- A placeholder Fighter player with `CharacterController`, `PlayerController`, `InputBuffer`, `ComboSystem`, `AbilityController`, `DodgeController`, and `ParryController`.
- Placeholder melee enemy and boss objects.

Save the generated scene manually into `Assets/Scenes` once the layout is acceptable.

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

## Known Limitations

- Placeholder keyboard input is included for local testing; mobile UI buttons should call `RequestAbility`, `RequestDodge`, and `RequestParry`.
- Ability targeting is intentionally simple and should be expanded with animation events, lock-on, or touch targeting later.
- Boss telegraph visuals are optional prefabs and not authored in this repo.
- Projectile pooling is supported through `ObjectPool`, but non-pooled fallback remains for beginner-friendly setup.
- No equipment UI, skill trees, addressables, Cinemachine, or multiplayer are included yet.

## Recommended Next Sequence

1. Save a generated vertical-slice scene and commit Unity metadata.
2. Replace placeholder primitives with prefabs and animator-driven attacks.
3. Add mobile UI buttons for attack, ability, dodge, and parry.
4. Create one tuned ability asset per class.
5. Add boss telegraph VFX/audio assets and phase-specific telegraph patterns.
6. Profile on Android/iOS and tune pooling, physics masks, and input latency.
