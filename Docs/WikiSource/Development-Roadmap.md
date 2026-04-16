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

## Recommended Next Features

### 1. Health UI

Add:

- HP bar
- HP text
- low-health warning

### 2. XP Bar UI

Use `ExperienceUI` with:

- level text
- XP text
- UI Slider

### 3. Survival Pickups

Add:

- small heart
- large heart
- magnet pickup
- bomb pickup
- temporary shield

### 4. Run Timer

Track:

- elapsed time
- survival time
- timed events
- boss spawn time

### 5. Spawn Scaling

Over time:

- increase max alive enemies
- reduce respawn delay
- add ranged enemies
- add elemental variants
- spawn elites

### 6. Elite Enemies

Add occasional stronger enemies with:

- more HP
- larger sprite
- more score
- more XP
- guaranteed drop

### 7. First Mini-Boss

Suggested boss:

```text
Elemental Warden
```

Patterns:

- slow chase
- projectile ring
- summon minions
- phase change at 66% and 33% HP

### 8. Weapon Evolution

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

