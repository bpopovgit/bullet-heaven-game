# Bullet Heaven Game Wiki

Welcome to the development wiki for **Bullet Heaven Game**, a Unity 2D bullet-heaven prototype with elemental combat, score, XP gems, level-up upgrades, enemy spawning, and status effects.

## Quick Links

- [Getting Started](Getting-Started)
- [Project Structure](Project-Structure)
- [Gameplay Systems](Gameplay-Systems)
- [Unity Setup](Unity-Setup)
- [Content Authoring](Content-Authoring)
- [Troubleshooting](Troubleshooting)
- [Development Roadmap](Development-Roadmap)

## Current Gameplay Loop

1. The player moves with the Unity Input System.
2. The player aims and shoots toward the mouse cursor.
3. Enemies spawn from configured spawn points.
4. Enemies damage the player through contact or projectiles.
5. Player bullets damage enemies using elemental `DamagePacket` data.
6. Dead enemies award score and drop XP gems.
7. XP gems are pulled toward the player within pickup range.
8. Level-ups pause the game and offer upgrade choices when `LevelUpManager` is configured.
9. Survival pickups can heal the player, pull all XP gems, or damage nearby enemies.
10. A run timer tracks active survival time and stops when the player dies.
11. A wave director can increase enemy pressure as the run timer advances.
12. Elite enemies can spawn on timed intervals with boosted health, rewards, scale, and tint.

## Current Unity Version

```text
Unity 2022.3.56f1
```

## Documentation Policy

The main repository `README.md` and `Docs/` folder are the source-of-truth developer docs. This wiki is the easier-to-browse documentation space.

When a system changes:

1. Update the code.
2. Update the matching file under `Docs/`.
3. Update this wiki source if the public documentation changed.
