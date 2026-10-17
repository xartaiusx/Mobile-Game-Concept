# Mobile Game Concept

A Unity rhythm-action RPG prototype exploring beat-timed melee combat, defensive moves, arena encounters, and visual feedback. The current playable slice is a short, Warrior-only session designed for testing combat feel.

## Current scope

- Rhythm-graded attacks and combos, a slash ability, dodging, and parrying
- Enemies, boss telegraphs, pickups, scoring, and a local high score
- A one-wave session with defeat, completion, and retry; longer wave scaling remains available for future tuning
- Local run telemetry and a Unity Editor analyzer
- Selected KayKit character visuals and Kenney environment/effect assets
- Scene-generation helpers, validation tools, and EditMode/PlayMode tests

Mage, Archer, Healer, projectile abilities, and class-swapping code remain as deferred prototype content. They are not active in the committed scene. Touch controls, mobile retry UI, device profiling, and a progression economy are still pending. Keyboard play in the Unity Editor is the current test path.

## Open and play

1. Open the repository in Unity **6000.4.4f1**, the version recorded in [ProjectVersion.txt](ProjectSettings/ProjectVersion.txt).
2. Open [`Assets/Scenes/VerticalSlice.unity`](Assets/Scenes/VerticalSlice.unity).
3. Run **Game > Vertical Slice > Validate Alpha Playtest**, then enter Play Mode.

The project uses the built-in render pipeline and legacy Input Manager. For scene regeneration or startup problems, follow [DEVELOPMENT.md](DEVELOPMENT.md).

### Controls

- Move: `Horizontal` / `Vertical` inputs
- Jump: `Jump`
- Attack: `Fire1`
- Slash ability: **Q**
- Dodge: **Left Shift**
- Parry: **E**
- Retry: **R**
- Telemetry overlay: **F3**

## Playtest and contribute

Run several Warrior sessions, then open **Game > Telemetry > Analyze Runs**. JSON reports are saved under `Application.persistentDataPath/Telemetry`; keep personal run data and generated test output out of Git. Short smoke tests are useful for checking behavior but are not balance evidence.

Read the [development guide](DEVELOPMENT.md) for telemetry batches, visual imports, scene validation, and test commands. Keep changes focused on the playable loop and include relevant EditMode or PlayMode coverage.

Current limitations include placeholder attack/dodge animations, a placeholder boss model, and combat pacing that still needs human playtesting.

## Project layout

- [`Assets/Scripts`](Assets/Scripts): gameplay, rhythm, UI, telemetry, and editor tools
- [`Assets/Scenes`](Assets/Scenes) and [`Assets/Prefabs`](Assets/Prefabs): playable content
- [`Assets/Art`](Assets/Art): visual wrappers, materials, and animation controllers
- [`Assets/ThirdParty`](Assets/ThirdParty): selected source assets and provenance
- [`Assets/Tests`](Assets/Tests): automated tests

## License and credits

Project code is covered by [GPL-3.0](LICENSE). Imported assets retain their own licenses. Preserve the [asset license manifest](Assets/ThirdParty/Licenses/ASSET_LICENSES.md) and [Kenney asset credits](Assets/ThirdParty/FreeAssets/ASSET_CREDITS.md) when using or changing those assets.

