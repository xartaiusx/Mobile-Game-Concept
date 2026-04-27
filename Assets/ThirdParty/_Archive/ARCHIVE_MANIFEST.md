# Asset Archive Manifest

Archive status: inactive. Assets listed here must not be referenced by active scenes, gameplay prefabs, visual prefabs, AnimatorControllers, or ScriptableObjects.

## KayKit Adventurers - Character Variants

- Source pack: KayKit Adventurers
- Original location: `Assets/ThirdParty/KayKit/Adventurers/Characters/fbx`
- Archive location: `Assets/ThirdParty/_Archive/KayKit/Adventurers/Characters/fbx`
- Assets kept: `Mage.fbx`, `Ranger.fbx`, `Rogue_Hooded.fbx`, and matching texture files.
- Reason kept: useful future visual variants, but not part of the Warrior-only active slice.
- License: CC0/free commercial use per KayKit Adventurers local license and source listing.
- Active/reference status: archived only; active visuals use `Knight.fbx`, `Rogue.fbx`, and `Barbarian.fbx` in the original active folder.
- Future intended use: optional future NPC/enemy visual candidates after the Warrior slice is stable.

## KayKit Adventurers - Accessory FBX Candidates

- Source pack: KayKit Adventurers
- Original location: `Assets/ThirdParty/KayKit/Adventurers/Assets/fbx` and `Textures`
- Archive location: `Assets/ThirdParty/_Archive/KayKit/Adventurers`
- Assets kept: accessory FBX models, matching accessory textures, and extra shared texture files.
- Reason kept: useful future visual dressing candidates, while keeping inactive accessories out of active art paths.
- License: CC0/free commercial use per KayKit Adventurers local license and source listing.
- Active/reference status: archived only.
- Future intended use: manually select one canonical FBX asset when needed, move only that asset back to an active source folder, then wire it through a visual wrapper.

## KayKit Character Animations - Mannequin References

- Source pack: KayKit Character Animations / KayKit related import
- Original location: `Assets/ThirdParty/KayKit/CharacterAnimations/MannequinCharacter`
- Archive location: `Assets/ThirdParty/_Archive/KayKit/CharacterAnimations/MannequinCharacter`
- Assets kept: `Mannequin_Large.fbx`, `Mannequin_Medium.fbx`, and matching mannequin texture.
- Reason kept: future rig/scale investigation, but not used by active controllers.
- License: CC0/free commercial use per KayKit source listing; keep KayKit source notes with active license manifest.
- Active/reference status: archived only; active controllers use the selected FBX animation files in `Assets/ThirdParty/KayKit/CharacterAnimations/AdventurersAnimations/fbx`.
- Future intended use: inspect only if a future animation retargeting pass needs mannequin references.

## Kenney Tiny Dungeon - Unused Tiles

- Source pack: Kenney Tiny Dungeon
- Original location: `Assets/ThirdParty/Kenney/TinyDungeon/Tiles`
- Archive location: `Assets/ThirdParty/_Archive/Kenney/TinyDungeon/Tiles`
- Assets kept: unused Tiny Dungeon individual tile PNGs.
- Reason kept: lightweight future dungeon dressing candidates.
- License: CC0 per Kenney source listing; see `Assets/ThirdParty/Licenses/Kenney_TinyDungeon_LICENSE_NOTE.md`.
- Active/reference status: archived only; active vertical slice uses only `tile_0000`, `tile_0001`, `tile_0002`, `tile_0016`, and `tile_0017` in the original active folder.
- Future intended use: selectively promote individual tiles back to active art if the arena dressing expands.

## External Vault Candidates

- Phase 9.11 moved duplicate and source-workflow exports out of the repo to `../Mobile-Game-Concept-External-Asset-Vault/Phase9.11`.
- Moved KayKit duplicate/source formats: Adventurers character GLB duplicates, accessory `fbx(unity)` duplicates, accessory GLTF/BIN sets, accessory OBJ/MTL sets, animation GLB duplicates, and mannequin GLB duplicates.
- Moved Kenney workflow exports: `Tilemap` PNG sheets and `Tiled` TMX/TSX samples.
- KayKit screenshots and preview images under `Assets/ThirdParty/_Documentation/KayKitAdventurers` are documentation-only and safe to move outside the repo if source/license notes remain.
- Large original downloads, zip files, and complete untouched asset packs should stay outside the repo.
- Do not commit new duplicate FBX/GLTF/OBJ sets unless a specific implementation needs them.

## Delete/Junk Candidates

- Deleted tracked root package residue: `Mobile-Game-Concept-updated.zip` and `Mobile-Game-Concept-UPDATED-README.txt`.
- No `.DS_Store`, `thumbs.db`, or temporary files were found during the Phase 9.11 archive cleanup.
