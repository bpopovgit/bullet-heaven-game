# Bullet Heaven Game Wiki

Welcome to the development wiki for **Bullet Heaven Game**, a Unity 2D bullet-heaven project with elemental combat, timed runs, elites, a dragon boss, pre-run loadouts, and active skills.

## Quick Links

- [Getting Started](Getting-Started)
- [Project Structure](Project-Structure)
- [Gameplay Systems](Gameplay-Systems)
- [Unity Setup](Unity-Setup)
- [Content Authoring](Content-Authoring)
- [Troubleshooting](Troubleshooting)
- [Development Roadmap](Development-Roadmap)

## Current Gameplay Loop

1. The player enters through the `Main` menu scene.
2. Single Player setup leads into a starting loadout.
3. The player chooses a weapon, bomb, active skill, and passive.
4. Gameplay loads into `Game.unity`.
5. The player moves with the Unity Input System and aims with the mouse.
6. The player shoots with the primary weapon and uses `Q` / `E` active skills.
7. Enemies spawn from authored spawn points and timed faction wave stages.
8. Enemies damage the player through contact or projectiles.
9. Human allies, Angels, Demons, and Zombies use faction-aware targeting.
10. Dead reward-enabled enemies award score, XP, and optional pickups.
11. Level-ups pause the game and offer upgrade choices when `LevelUpManager` is configured.
12. Timed elites and the dragon boss interrupt the normal run flow.
13. Boss kills award a separate boss reward choice.

## Current Unity Version

```text
Unity 6000.3.14f1
```

## Documentation Policy

The main repository `README.md` and `Docs/` folder are the source-of-truth developer docs. This wiki is the easier-to-browse documentation mirror.

When a system changes:

1. Update the code.
2. Update the matching file under `Docs/`.
3. Update `Docs/WikiSource/`.
4. Copy the wiki source into the GitHub wiki repository.
