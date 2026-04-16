# Roadmap

This roadmap is intentionally practical. It focuses on changes that make the prototype feel more like a complete bullet-heaven run.

## Completed Foundations

- Top-down player movement.
- Mouse-aimed shooting.
- Elemental damage packets.
- Enemy health and resistances.
- Melee and ranged enemies.
- Enemy projectiles.
- Player damage, i-frames, knockback, and death.
- Score system.
- XP gems.
- Level-up upgrades.
- Optional level-up choice UI.
- Spawn-point based enemy respawning.

## Highest-Value Next Features

### 1. Visible XP Bar and Level Indicator

Add an always-visible UI display for:

- current level
- current XP
- XP needed for next level

The `ExperienceUI` script already supports TMP texts and an optional Slider.

### 2. Health UI

The player currently has HP logic, but no dedicated HP display is documented.

Add:

- HP text
- HP bar
- low-health visual cue

Useful script idea:

```text
PlayerHealthUI
```

### 3. Pickup Variety

Add drops beyond XP:

- small heart
- large heart
- magnet pickup
- bomb pickup
- temporary shield
- temporary speed boost

Suggested first implementation:

- `HealthPickup`
- `MagnetPickup`
- `BombPickup`

### 4. Wave Timer and Difficulty Scaling

Add a run timer and spawn scaling:

- increase `maxAlive` over time
- decrease respawn delay over time
- introduce ranged enemies after a delay
- introduce elemental variants later

Suggested script:

```text
RunTimer
EnemyWaveDirector
```

### 5. Elite Enemies

Add occasional stronger enemies:

- more health
- larger sprite
- higher score
- more XP
- guaranteed pickup drop

This can reuse existing enemy scripts at first.

### 6. First Mini-Boss

A first boss should interrupt the normal rhythm.

Suggested boss:

```text
Elemental Warden
```

Simple pattern:

- chases slowly
- fires ring projectiles
- summons minions at 66% HP
- fires faster below 33% HP

Reward:

- big XP burst
- score bonus
- guaranteed upgrade choice

### 7. Weapon Evolution

After a weapon reaches enough upgrades, allow an evolved version.

Examples:

- Physical pistol evolves into piercing rail shots.
- Fire evolves into explosions on kill.
- Frost evolves into freeze bursts.
- Lightning evolves into chain lightning.
- Poison evolves into poison pools.

### 8. Map Architecture

Move beyond an empty arena:

- open central space
- obstacle clusters
- choke points
- reward-risk areas
- spawn points placed around the edges

This makes movement choices more interesting than walking in circles forever.

## Suggested Immediate Implementation Order

1. Health UI.
2. XP bar UI.
3. Health pickup.
4. Magnet pickup.
5. Run timer.
6. Spawn scaling.
7. Elite enemy.
8. Mini-boss.

## Design Rule of Thumb

Every minute of play should add at least one of these:

- a new choice
- a new threat
- a new reward
- a new pattern the player must learn

