# Mobile Game Concept

Unity mobile rhythm-action RPG prototype.

The current vertical slice targets Unity `6000.4.4f1`, the built-in render pipeline, and the legacy Input Manager. The main playable scene is:

`Assets/Scenes/VerticalSlice.unity`

For detailed setup, asset, and test notes, see [DEVELOPMENT.md](DEVELOPMENT.md).

## Current Slice

- Free 3D player movement with jump.
- Rhythm-buffered attacks through `InputBuffer`, `BeatClock`, and `RhythmJudgement`.
- Fighter, Mage, Archer, and Healer placeholder player prefabs.
- Ability, dodge, and parry controllers with rhythm-grade effects.
- Class-default ability assets with distinct Fighter, Mage, Archer, and Healer behavior.
- Animator-ready attack timing states: windup, active, and recovery.
- Melee, ranged, and boss enemy placeholders.
- Pooled projectile prefab for ranged attacks.
- Boss beat telegraph data/controller with beat-pulsed warning and impact placeholders.
- Small primitive arena with boundary walls and a simple follow camera.
- Runtime HUD for beat phase, judgement feedback, combo, cooldowns, dodge/parry state, boss countdown, and debug readout.
- Event-driven placeholder feedback pulses for Perfect/Good/Miss attacks, dodge, parry, and boss attacks.
- Phase 4 tuning values for a readable keyboard/controller combat loop: more survivable Fighter, calmer enemy pressure, compact HUD, and higher follow camera.
- Phase 5 placeholder polish: clearer hit/dodge/parry/boss VFX prefabs, procedural audio cue assets, projectile trails, enemy windup warnings, and animation-event relay hooks.
- Phase 6 gameplay loop: pooled Mage/Archer projectile abilities, class balance defaults, arena wave-to-boss flow, victory/failure state, and a player-like simulation helper.
- Phase 7 rhythm rules and gameplay: truthful centered Beat Bar timing, score scaling, dodge timing tuning, boss phases, hit reactions, and pickup rewards.

## Class Abilities

- Fighter Slash: close cone strike with high Perfect reward and brief enemy stagger.
- Mage Bolt: medium-speed projectile with Perfect splash/stagger and longer cooldown.
- Archer Shot: fast precision projectile; Perfect timing can pierce one target and Miss is sharply reduced.
- Healer Pulse: self heal; Perfect timing adds brief protection.

## Controls

- Move: `Horizontal` / `Vertical`
- Jump: `Jump`
- Attack: `Fire1`
- Ability: `Q`
- Dodge: `Left Shift`
- Parry: `E`

## Bootstrap

Open the project in Unity `6000.4.4f1`, then use:

`Game > Vertical Slice > Create Or Refresh Vertical Slice`

This recreates the default folders, ScriptableObject assets, prefabs, scene objects, and Build Settings entry.

## Phase 5 Feedback

- VFX prefabs live in `Assets/Prefabs/Feedback`.
- Procedural audio cue assets live in `Assets/ScriptableObjects/Audio`.
- Player prefabs include `CombatAnimationBridge` and `AnimationEventRelay` so future clips can call attack active/recovery, footstep, and weapon-swing hooks.
- Boss telegraphs pulse warning rings on beats and show countdown/impact text in the HUD.

## Phase 6 Gameplay

- `AbilityController` launches projectile-style abilities through assigned `ObjectPool` instances.
- `PoolableProjectile` can now damage enemies, apply rhythm grade, splash Mage impacts, and allow Archer Perfect pierce.
- `ArenaController` manages the vertical slice loop: initial wave, boss activation, victory, failure, and `R` restart.
- `PlayerSimulationController` supports cautious automated play behavior for validation without changing mobile input direction.

## Rhythm Rules

- The green center zone on the Beat Bar is the actual Perfect timing window.
- The white slider travels left to right; overlap with the green center zone is Perfect.
- Higher player levels increase effective BPM with a small capped multiplier, making timing tighter without desynchronizing combat.
- Perfect attacks do the most damage and score; Good is moderate; Miss is reduced and scores little only on hit.
- Perfect dodges move faster, cool down sooner, and add a small invulnerability bonus.

## Phase 7 Gameplay

- `ScoreSystem` listens to rhythm damage and feeds HUD score.
- `HitReactionController` adds lightweight flashes and knockback/stagger readability.
- Boss phase assets support slower Phase 1 and faster Phase 2 telegraph behavior.
- Gold and Health Potion pickup prefabs can drop from defeated enemies and update inventory HUD text.
- Combat timing now supports both timed placeholder attacks and future animation-event-driven hit windows.

## Tests

```bash
"$HOME/Unity/Hub/Editor/6000.4.4f1/Editor/Unity" -quit -batchmode -projectPath "$PWD" -logFile -
"$HOME/Unity/Hub/Editor/6000.4.4f1/Editor/Unity" -quit -batchmode -projectPath "$PWD" -runTests -testPlatform EditMode
"$HOME/Unity/Hub/Editor/6000.4.4f1/Editor/Unity" -quit -batchmode -projectPath "$PWD" -runTests -testPlatform PlayMode
```

## Next

Next: human Mage/Archer tuning, placeholder animator controllers/clips, better boss attack variety, inventory/potion UI, then early mobile touch controls and device profiling.
