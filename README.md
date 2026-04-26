# Mobile Game Concept

Unity mobile rhythm-action prototype focused on one playable class: Warrior.

The current vertical slice targets Unity `6000.4.4f1`, the built-in render pipeline, and the legacy Input Manager. The main playable scene is:

`Assets/Scenes/VerticalSlice.unity`

For detailed setup, asset, and test notes, see [DEVELOPMENT.md](DEVELOPMENT.md).

## Current Direction

The prototype has pivoted away from multi-class balance. Warrior is the only active playable class for the current slice, using the old Fighter implementation as a compatibility base while exposing Warrior naming and tuning in gameplay.

Mage, Archer, Healer, projectile ability assets, and class swap code remain in the repository as deferred prototype content. They should compile and remain valid as assets, but they are not part of the active `VerticalSlice.unity` gameplay path.

## Current Slice

- Endless rhythm-action melee arena loop.
- Warrior-only active player path through `GameManager`, `PlayerManager`, HUD, camera, arena, score, dodge, parry, pickups, and boss systems.
- Close-range combo attacks using `ComboSystem`, `DefaultComboProfile`, and `DefaultAttackTiming`.
- Rhythm precision rewards: Perfect hits deal and score the most, Good hits are moderate, Miss hits are weak and score little or nothing.
- Perfect dodge moves faster, cools down sooner, and grants stronger invulnerability.
- Perfect parry cancels parryable damage and staggers enemies for a counter opportunity.
- Scalable endless waves with short next-wave delay, no terminal victory after wave 1, and failure on player death.
- Boss/elite warning waves every 5 waves by default.
- HUD shows wave, score, high score, enemies remaining, boss/elite warning, cooldowns, and rhythm grade feedback.

## Controls

- Move: `Horizontal` / `Vertical`
- Jump: `Jump`
- Attack: `Fire1`
- Ability: `Q` for the active Warrior slash ability
- Dodge: `Left Shift`
- Parry: `E`
- Restart run: `R`

`ClassSwapDebugController` is inactive in `VerticalSlice.unity`. Do not use Mage, Archer, Healer, or class swapping for active prototype validation.

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

`Scripts/run-unity-tests.sh` writes authoritative summaries to `TestResults/editmode-summary.json`, `TestResults/playmode-summary.json`, and `TestResults/summary.txt`.

## Next

Next phase should be a hands-on Warrior feel pass: melee hit readability, boss/elite rhythm telegraphs, arena pacing over waves 1-10, mobile touch controls, and pooled feedback/pickup effects for mobile stress testing.
