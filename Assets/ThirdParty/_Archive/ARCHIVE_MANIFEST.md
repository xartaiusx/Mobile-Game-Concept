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

## KayKit Adventurers - Accessory And Duplicate Formats

- Source pack: KayKit Adventurers
- Original location: `Assets/ThirdParty/KayKit/Adventurers/Assets`, `Characters/gltf`, and `Textures`
- Archive location: `Assets/ThirdParty/_Archive/KayKit/Adventurers`
- Assets kept: accessory model folders, GLTF character duplicates, and extra texture folders.
- Reason kept: may be useful for later visual dressing, but duplicate formats and accessories should not sit in active art paths.
- License: CC0/free commercial use per KayKit Adventurers local license and source listing.
- Active/reference status: archived only.
- Future intended use: manually select one canonical FBX asset when needed; avoid wiring GLTF duplicates unless there is a clear import reason.

## KayKit Character Animations - Alternate Formats And Mannequins

- Source pack: KayKit Character Animations / KayKit related import
- Original location: `Assets/ThirdParty/KayKit/CharacterAnimations/AdventurersAnimations/gltf` and `MannequinCharacter`
- Archive location: `Assets/ThirdParty/_Archive/KayKit/CharacterAnimations`
- Assets kept: GLTF animation duplicates and mannequin reference files.
- Reason kept: future rig/clip investigation, but not used by active controllers.
- License: CC0/free commercial use per KayKit source listing; keep KayKit source notes with active license manifest.
- Active/reference status: archived only; active controllers use the selected FBX animation files in `Assets/ThirdParty/KayKit/CharacterAnimations/AdventurersAnimations/fbx`.
- Future intended use: inspect only if FBX clip coverage is insufficient.

## Kenney Tiny Dungeon - Unused Tiles And Tool Exports

- Source pack: Kenney Tiny Dungeon
- Original location: `Assets/ThirdParty/Kenney/TinyDungeon/Tiles`, `Tilemap`, and `Tiled`
- Archive location: `Assets/ThirdParty/_Archive/Kenney/TinyDungeon`
- Assets kept: unused Tiny Dungeon tile PNGs plus Tilemap/Tiled exports.
- Reason kept: lightweight future dungeon dressing candidates and source workflow references.
- License: CC0 per Kenney source listing; see `Assets/ThirdParty/Licenses/Kenney_TinyDungeon_LICENSE_NOTE.md`.
- Active/reference status: archived only; active vertical slice uses only `tile_0000`, `tile_0001`, `tile_0002`, `tile_0016`, and `tile_0017` in the original active folder.
- Future intended use: selectively promote individual tiles back to active art if the arena dressing expands.

## External Vault Candidates

- KayKit screenshots and preview images under `Assets/ThirdParty/_Documentation/KayKitAdventurers` are documentation-only and safe to move outside the repo if source/license notes remain.
- Large original downloads, zip files, and complete untouched asset packs should stay outside the repo.
- Do not commit new duplicate FBX/GLTF/OBJ sets unless a specific implementation needs them.

## Delete/Junk Candidates

No `.DS_Store`, `thumbs.db`, extracted zip residue, or temporary files were found during the Phase 9.11 archive audit.
