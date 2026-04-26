Mobile Game Concept

A Unity-based rhythm-action RPG prototype for mobile, combining class-based combat, enemy AI, loot systems, and modular gameplay architecture. Players choose from unique classes, face waves of enemies and bosses, and collect loot, all while syncing their attacks, dodges, and abilities to the beat of the soundtrack for enhanced effects and combos. The result is fast-paced, music-driven combat that rewards timing, positioning, and precision.

For current vertical-slice setup instructions and asset creation notes, see [DEVELOPMENT.md](DEVELOPMENT.md).


---

📜 Overview

This project is a mobile-friendly 3D action RPG where rhythm drives the battle flow. Each successful on-beat input increases damage, reduces cooldowns, or triggers bonus effects. The architecture is designed for modularity, enabling easy expansion into a full game or adaptation for other timing-based combat systems.


---

🎮 Gameplay Features

Rhythm-Based Combat — Attack, dodge, and unleash abilities in sync with the music to amplify their effects.

Class Selection — Fighter, Mage, Archer, and Healer with unique stats and combat styles.

Boss Encounters — Melee and ranged bosses with configurable attack phases and patterns.

Enemy AI — Melee chasers, ranged attackers, and rhythm-aware bosses.

Loot & Inventory — Randomized loot drops, item stacking, rarity tiers, and inventory limits.

Spawner System — Configurable spawner with max active enemy caps.

Extensible Architecture — Base classes, clean interfaces, and modular systems for easy iteration.



---

🛠 Technical Details

Engine: Unity (2021 LTS+ recommended)

Language: C#

Target Platform: Mobile (iOS & Android)

Architecture:

Game.Core namespace for core scripts

Base hierarchies for characters and enemies

Modular subsystems for inventory, loot, and spawning


Input:

Defaults to Unity's legacy Input Manager (Horizontal, Vertical, Jump, Fire1)

Adaptable to Unity’s new Input System




---

📂 Project Structure

Assets/
 └── Scripts/
      └── Core/
           BaseCharacter.cs       # Common player & NPC logic
           PlayerController.cs    # Handles movement and combat
           Fighter.cs / Mage.cs / Archer.cs / Healer.cs
           BaseEnemy.cs           # Enemy AI core
           MeleeEnemy.cs
           RangedEnemy.cs
           BossEnemy.cs
           Projectile.cs
           PlayerManager.cs
           GameManager.cs
           CharacterSelection.cs
           EnemySpawner.cs
           InventorySystem.cs
           LootSystem.cs
           ItemDefinition.cs


---

🚀 Getting Started

1. Import the Project

Clone or download this repository.

Open it in Unity 2021 LTS or later.


2. Setup Player Characters

Create prefabs for Fighter, Mage, Archer, and Healer.

Add CharacterController, respective class script, and PlayerController.


3. Setup Game Systems

Add GameManager and PlayerManager to the main scene.

Assign player prefabs to GameManager.


4. Setup Enemies

Create prefabs for MeleeEnemy and RangedEnemy.

Assign projectile prefabs for ranged units.

Create a BossEnemy prefab and choose attack mode.


5. Add Spawner

Drop EnemySpawner into the scene.

Assign enemy prefabs and adjust spawn settings.



---

🔮 Planned Features

Expanded Rhythm Mechanics — Chain combos, parries, and ultimates triggered on-beat.

Skill Trees for each class with rhythm-based buffs.

Equipment System with beat-synergized weapon effects.

Quest System for progression and narrative.

Procedural Levels for replayability.

Cinemachine Camera for dynamic action framing.

Multiplayer Co-op for synchronized boss fights.



---

🗺 Development Roadmap

Phase 1 — Vertical Slice (Current)

✅ Playable classes with movement, combat, and rhythm hooks.

✅ Enemy AI for melee, ranged, and bosses.

✅ Loot and inventory prototype.

✅ Enemy spawning system.


Phase 2 — Core Expansion

⏳ Add skill trees and rhythm combo systems.

⏳ Implement equipment with rhythm effects.

⏳ Enhance boss mechanics with rhythm-triggered phases.


Phase 3 — Content & Optimization

⏳ Expand environments and enemy types.

⏳ Optimize for mobile with object pooling and addressables.

⏳ Add co-op multiplayer support.



---

🤝 Contributing Guidelines

We welcome contributions!

1. Fork the repository and clone it locally.


2. Create a feature branch:

git checkout -b feature/your-feature-name


3. Follow Unity C# best practices:

Use [SerializeField] for private fields needing inspector access.

Keep systems modular and data-driven.



4. Test changes in a separate scene before committing.


5. Commit with a descriptive message:

git commit -m "Add rhythm-based combo attack for Fighter"


6. Push and submit a Pull Request.




---

📄 License

This project is released under the MIT License — free to use, modify, and distribute with attribution.
