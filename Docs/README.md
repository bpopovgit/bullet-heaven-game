# Documentation Index

This folder is the repository source-of-truth documentation for the current Unity 6 version of the project.

## Recommended Online Home

Keep these Markdown files in the repository as the canonical developer docs. If the project is hosted on GitHub, the root `README.md` works well as the public landing page, while `Docs/` stays versioned with the code.

The GitHub Wiki is useful as the browsable external mirror, but the repo docs should remain the source that gets updated first.

## Documents

- [Project Overview](PROJECT_OVERVIEW.md): scenes, folders, and script groups.
- [Gameplay Systems](GAMEPLAY_SYSTEMS.md): combat, loadouts, active skills, spawning, bosses, and audio.
- [Unity Setup Guide](UNITY_SETUP_GUIDE.md): scene wiring steps for the current menu and gameplay flow.
- [Content Authoring](CONTENT_AUTHORING.md): how to add enemies, weapons, status effects, projectiles, pickups, and SFX content.
- [Roadmap](ROADMAP.md): practical next features from the current project state.
- [System Handoff](SYSTEM_HANDOFF.md): scene-by-scene and script-by-script ownership map for onboarding.
- [System Graph](SYSTEM_GRAPH.md): Mermaid diagrams for the current scene and system architecture.

## Documentation Rules

- Update docs in the same change as code when a system's setup or behavior changes.
- Prefer short, practical explanations over exhaustive prose.
- Document Unity Inspector setup whenever a script requires scene or prefab references.
- When the GitHub Wiki is meant to stay current, update `Docs/WikiSource/` after updating these repo docs.
