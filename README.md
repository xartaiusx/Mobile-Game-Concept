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

## Class Abilities

- Fighter Slash: close cone strike with high Perfect reward and brief enemy stagger.
- Mage Bolt: medium-range spell with higher damage and longer cooldown.
- Archer Shot: long-range precision hit with the strongest Perfect multiplier.
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

## Tests

```bash
"$HOME/Unity/Hub/Editor/6000.4.4f1/Editor/Unity" -quit -batchmode -projectPath "$PWD" -logFile -
"$HOME/Unity/Hub/Editor/6000.4.4f1/Editor/Unity" -quit -batchmode -projectPath "$PWD" -runTests -testPlatform EditMode
"$HOME/Unity/Hub/Editor/6000.4.4f1/Editor/Unity" -quit -batchmode -projectPath "$PWD" -runTests -testPlatform PlayMode
```

## Next

Do a human keyboard/controller pass, then move into Phase 6: true Mage/Archer projectile abilities, per-class combat balance, a simple pickup loop, and arena win/loss structure. Mobile touch controls come after the keyboard/controller slice feels coherent.
