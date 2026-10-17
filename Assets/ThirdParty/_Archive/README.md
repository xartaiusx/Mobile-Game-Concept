# Inactive Asset Archive

Imported assets retained for possible future use. They are not part of the active playable slice.

- Active scenes, prefabs, animation controllers, and ScriptableObjects must not reference this folder.
- Preserve source, license, and selection details in [ARCHIVE_MANIFEST.md](ARCHIVE_MANIFEST.md).
- To use an archived asset, move only the selected content to its active source folder, update the manifest, and connect it through a project visual wrapper.
- Keep complete downloads, duplicate formats, and unrelated exports outside the repository.
- Run **Game > Visuals > Validate Asset Archive** after moving assets.

See the [development guide](../../../DEVELOPMENT.md) for the full asset workflow.

