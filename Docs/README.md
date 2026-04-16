# Documentation Index

This folder documents the current project structure, gameplay systems, Unity setup steps, and intended next work.

## Recommended Online Home

Keep these Markdown files in the repository as the source of truth. If the project is hosted on GitHub, the root `README.md` becomes the public landing page, and the `Docs/` folder works well for versioned technical documentation.

For longer design notes, tutorials, or public-facing guides, a GitHub Wiki can be useful later. The repo docs should remain the canonical developer documentation because they change together with the code.

## Documents

- [Project Overview](PROJECT_OVERVIEW.md): high-level layout, assets, scenes, and script groups.
- [Gameplay Systems](GAMEPLAY_SYSTEMS.md): how combat, scoring, XP, upgrades, enemies, and UI connect.
- [Unity Setup Guide](UNITY_SETUP_GUIDE.md): scene wiring steps for score, XP, level-up UI, enemies, and prefabs.
- [Content Authoring](CONTENT_AUTHORING.md): how to add enemies, weapons, projectiles, VFX, and upgrades.
- [Roadmap](ROADMAP.md): suggested next features and implementation order.

## Documentation Rules

- Update docs in the same change as code when a system's setup or behavior changes.
- Prefer short, practical explanations over exhaustive prose.
- Document Unity Inspector setup whenever a script requires scene or prefab references.
- Put player-facing design notes in `Docs/ROADMAP.md` until they become implemented systems.

