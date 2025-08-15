Mobile-Game-Concept: Updated minimal compile-ready core scripts
What changed:
- Replaced truncated/placeholder scripts with compile-ready versions under Game.Core namespace.
- Added PlayerManager and Projectile scripts.
- Standardized CharacterController-based movement for player and enemies.
- Introduced simple InventorySystem and LootSystem stubs.

Next steps in Unity:
1) Create prefabs for Fighter, Mage, Archer, Healer:
   - Add respective component to each and also add PlayerController + CharacterController.
2) Create enemy prefabs:
   - MeleeEnemy, RangedEnemy (assign Projectile prefab), BossEnemy (optional).
3) Add a GameManager and PlayerManager to your scene. Assign player prefabs to GameManager.
4) Add an EnemySpawner and assign an enemy prefab.
5) Optionally create ScriptableObject ItemDefinition assets for loot.
6) Ensure Input axes (Horizontal, Vertical, Jump, Fire1) exist in Project Settings > Input Manager.

You can now build a test scene and press Play to move, jump, and attack enemies.
