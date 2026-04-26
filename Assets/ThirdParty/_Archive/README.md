# Third-Party Asset Archive

This folder stores inactive imported assets that may be useful later but are not part of the active vertical slice.

Rules:
- Active scenes, prefabs, controllers, and ScriptableObjects must not reference assets under `_Archive`.
- Keep source family and license context in `ARCHIVE_MANIFEST.md`.
- Prefer active content under `Assets/ThirdParty/<Source>/...` only for selected, currently wired assets.
- Keep large full-pack downloads, zips, screenshots, and source exports outside the repo unless there is a clear implementation need.
- Run `Game > Visuals > Validate Asset Archive` after moving assets in or out of this folder.
