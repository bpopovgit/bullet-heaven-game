# Bullet Heaven Game

A Unity 2D top-down bullet-heaven project with elemental combat, pre-run loadouts, active abilities, timed enemy pressure, elites, and a dragon boss.

## Current Features

- Unity 6 project flow with a front-end menu scene and a gameplay scene
- pre-run loadout selection for:
  - starting weapon
  - bomb on `Q`
  - active skill on `E`
  - passive perk
- primary mouse-aimed shooting with upgrade scaling
- score, XP gems, and level-up upgrade choices
- health, magnet, and bomb pickups
- run timer and wave-based enemy escalation
- timed elite spawns with announcements
- dragon boss with phase two and boss reward choices
- gameplay SFX loaded from `Resources` folders with random clip variation

## Current Gameplay Loop

1. The player enters through the main menu scene.
2. Single Player setup leads into a starting loadout.
3. The player chooses a weapon, bomb, active skill, and passive.
4. Gameplay loads into the survival arena.
5. The player moves with the Unity Input System and aims with the mouse.
6. The player shoots with the primary weapon and uses `Q` / `E` active abilities.
7. Enemies spawn from authored spawn points and timed wave stages.
8. Enemies damage the player through contact or projectiles.
9. Dead enemies award score, XP, and optional pickups.
10. Level-ups pause the game and offer upgrade choices.
11. Timed elites and the dragon boss interrupt the normal run flow.
12. Boss kills award a separate boss reward choice.

## Main Scenes

```text
Assets/Scenes/Main.unity
Assets/Game.unity
```

- `Main.unity` is the front-end entry scene.
- `Game.unity` is the active gameplay scene.

## Documentation

- [Docs Index](Docs/README.md)
- [Project Overview](Docs/PROJECT_OVERVIEW.md)
- [Gameplay Systems](Docs/GAMEPLAY_SYSTEMS.md)
- [Unity Setup Guide](Docs/UNITY_SETUP_GUIDE.md)
- [Content Authoring](Docs/CONTENT_AUTHORING.md)
- [Roadmap](Docs/ROADMAP.md)
- [System Handoff](Docs/SYSTEM_HANDOFF.md)
- [System Graph](Docs/SYSTEM_GRAPH.md)

## Unity Version

This project currently targets Unity `6000.3.14f1`.

## Notes

- Gameplay SFX are loaded from `Assets/Resources/Audio/SFX/`.
- The GitHub Wiki is a browsable mirror of the repo documentation, but the `Docs/` folder in this repository is the source of truth.
