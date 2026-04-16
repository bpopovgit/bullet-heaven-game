# Bullet Heaven Game

A Unity 2D bullet-heaven prototype with elemental combat, enemy spawning, scoring, XP gems, and level-up upgrades.

## Current Gameplay Loop

1. The player moves with the Unity Input System.
2. The player aims and shoots toward the mouse cursor.
3. Enemies spawn from configured spawn points.
4. Enemies damage the player through contact or projectiles.
5. Player bullets damage enemies using elemental `DamagePacket` data.
6. Dead enemies award score and drop XP gems.
7. XP gems are pulled toward the player within pickup range.
8. Level-ups pause the game and offer upgrade choices when `LevelUpManager` is configured.

## Documentation

- [Docs Index](Docs/README.md)
- [Project Overview](Docs/PROJECT_OVERVIEW.md)
- [Gameplay Systems](Docs/GAMEPLAY_SYSTEMS.md)
- [Unity Setup Guide](Docs/UNITY_SETUP_GUIDE.md)
- [Content Authoring](Docs/CONTENT_AUTHORING.md)
- [Roadmap](Docs/ROADMAP.md)

## Unity Version

This project currently targets Unity `2022.3.56f1`.

## Main Scene Notes

The active gameplay work has been happening in `Assets/Game.unity`. Check Unity's Build Settings before making a build, because `ProjectSettings/EditorBuildSettings.asset` may not include every scene used during development.

