# Development Roadmap

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
- Level-up choice UI.
- Spawn-point based enemy respawning.
- Health, magnet, and bomb survival pickups.
- Optional health UI support.
- Run timer and timer UI support.
- Wave director and basic spawn scaling.
- Timed elite enemy spawns.

## Recommended Next Features

### 1. XP Bar UI

Use `ExperienceUI` with:

- level text
- XP text
- UI Slider

### 2. Low-Health Feedback

Add:

- red screen vignette
- pulsing HP text
- warning sound

### 3. More Survival Pickups

Health, magnet, and bomb pickups exist. Add:

- temporary shield
- temporary speed boost
- temporary fire-rate boost

### 4. Advanced Wave Content

Basic spawn scaling exists. Next, add richer timed content:

- add ranged enemies
- add elemental variants
- create special swarm waves
- spawn elites

### 5. Elite Enemy Polish

Timed elite spawns exist. Next polish:

- spawn warning text or sound
- elite death burst
- guaranteed pickup drop
- elite-specific sprite or outline

### 6. First Mini-Boss

Suggested boss:

```text
Elemental Warden
```

Patterns:

- slow chase
- projectile ring
- summon minions
- phase change at 66% and 33% HP

### 7. Weapon Evolution

Examples:

- Fire: explosions on kill.
- Frost: freeze bursts.
- Lightning: chain lightning.
- Poison: poison pools.
- Physical: piercing rail shots.

## Design Rule

Every minute of play should add at least one of:

- a new choice
- a new threat
- a new reward
- a new pattern the player must learn
