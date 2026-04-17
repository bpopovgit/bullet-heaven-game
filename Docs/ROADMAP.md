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
- Health, magnet, and bomb survival pickups.
- Optional health UI support.
- Run timer and timer UI support.
- Wave director and basic spawn scaling.
- Timed elite enemy spawns.

## Highest-Value Next Features

### 1. Visible XP Bar and Level Indicator

Add an always-visible UI display for:

- current level
- current XP
- XP needed for next level

The `ExperienceUI` script already supports TMP texts and an optional Slider.

### 2. Low-Health Feedback

Health UI support exists. Next, add stronger feedback when the player is in danger:

- red screen vignette
- pulsing HP text
- warning sound

### 3. More Pickup Variety

Health, magnet, and bomb pickups exist. Good next pickup additions:

- temporary shield
- temporary speed boost
- temporary fire-rate boost

### 4. Advanced Wave Content

Basic wave scaling exists. Next, add more interesting timed content:

- introduce ranged enemies after a delay
- introduce elemental variants later
- create special swarm waves
- add timed elite spawns

Suggested script:

```text
RunTimer
EnemyWaveDirector
```

### 5. Elite Enemy Polish

Timed elite spawns exist. Next polish:

- spawn warning text or sound
- elite death burst
- guaranteed pickup drop
- elite-specific sprite or outline

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

1. XP bar UI.
2. Low-health feedback.
3. Temporary shield pickup.
4. Timed enemy pool changes.
5. Elite warning/death feedback.
6. Mini-boss.
7. Weapon evolution.

## Design Rule of Thumb

Every minute of play should add at least one of these:

- a new choice
- a new threat
- a new reward
- a new pattern the player must learn
