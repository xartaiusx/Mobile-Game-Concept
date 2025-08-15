Mobile Game Concept

A Unity-based rhythm-action RPG prototype for mobile, featuring class-based characters, AI-driven enemies, loot systems, and modular gameplay systems. Built as both a vertical slice prototype and a foundation for full-scale development.


---

📜 Overview

This project is a mobile-friendly 3D action RPG where players choose from multiple unique classes, battle melee and ranged enemies, face boss encounters, and collect loot to enhance their abilities. The architecture is modular, making it easy to expand, optimize, or repurpose for other genres.


---

🎮 Gameplay Features

Class Selection — Fighter, Mage, Archer, and Healer with unique stats and playstyles.

Player Movement & Combat — Third-person camera-aligned movement with melee attacks and optional ranged combat.

Enemy AI — Melee chasers, ranged attackers, and bosses with configurable attack patterns.

Boss Encounters — Supports melee and burst-ranged modes, with cooldowns and special attacks.

Loot & Inventory — Randomized loot drops, stacking items, rarity tiers, and inventory capacity limits.

Spawner System — Configurable enemy spawner with maximum active enemy limits.

Extensible Architecture — Base classes and interfaces encourage modularity and clean expansion.



---

🛠 Technical Details

Engine: Unity (2021 LTS+ recommended)

Language: C#

Target Platform: Mobile (iOS & Android)

Architecture:

Game.Core namespace for all core scripts

Base class hierarchies for characters and enemies

Modular subsystems (Inventory, Loot, Spawning)


Input:

Defaults to Unity's legacy Input Manager (Horizontal, Vertical, Jump, Fire1)

Compatible with Unity’s new Input System with minor changes




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

Skill Trees for each class.

Equipment System with weapon and armor stats.

Quest System with narrative progression.

Procedural Level Generation for replayability.

Co-op Multiplayer boss battles.

Polished Cinemachine Cameras.

Object Pooling for performance.



---

🗺 Development Roadmap

Phase 1 — Vertical Slice (Current)

✅ Playable classes with movement and combat.

✅ Enemy AI for melee, ranged, and bosses.

✅ Loot and inventory prototype.

✅ Enemy spawning system.


Phase 2 — Core Expansion

⏳ Add skill trees and class progression.

⏳ Implement equipment and gear upgrades.

⏳ Improve boss mechanics with phase changes.


Phase 3 — Content & Optimization

⏳ Expand environments and enemy variety.

⏳ Optimize for mobile with object pooling and addressable assets.

⏳ Add co-op multiplayer support.



---

🤝 Contributing Guidelines

We welcome contributions! Here’s how to help:

1. Fork the repository and clone it locally.


2. Create a feature branch:

git checkout -b feature/your-feature-name


3. Make changes that follow Unity C# best practices:

Use [SerializeField] for private fields needing inspector access.

Keep scripts modular and avoid hardcoding values where ScriptableObjects are better.



4. Test your changes in a new scene or with prefabs before committing.


5. Commit with a descriptive message:

git commit -m "Add new boss ranged attack pattern"


6. Push and submit a Pull Request.




---

📄 License

This project is released under the MIT License — you are free to use, modify, and distribute it with attribution.
