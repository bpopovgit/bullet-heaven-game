# Gameplay Systems

## Combat and Damage

Damage flows through `DamagePacket`.

Key fields:

- `amount`
- `element`
- `splashRadius`
- `sourcePos`
- `status`
- `statusDuration`
- `statusStrength`

Elements:

- `Physical`
- `Fire`
- `Lightning`
- `Frost`
- `Poison`

Statuses:

- `None`
- `Burn`
- `Shock`
- `Slow`
- `Freeze`
- `Poison`

## Pre-Run Loadout

The run now begins with a chosen loadout:

- starting weapon
- bomb on `Q`
- active skill on `E`
- passive perk

This loadout is selected in the menu scene and applied when gameplay starts.

## Player Shooting

`PlayerShooting` reads the active `WeaponDefinition`, mouse position, and `PlayerStats`.

Upgrade-aware stats:

- damage multiplier
- fire rate
- projectile count
- pierce
- splash radius

Extra projectiles fire as spread shots.

## Bomb Skill on `Q`

`PlayerActiveBomb` handles the first active skill slot.

Current bombs:

- `Frag Bomb`
- `Frost Bomb`
- `Fire Bomb`
- `Shock Bomb`

Bombs use cursor-targeted throws, visible projectile travel, impact visuals, and a cooldown icon in the HUD.

## Secondary Skill on `E`

`PlayerSecondaryActiveSkill` handles the second active slot.

Current options:

- `Magnetic Pulse`
- `Arcane Shield`
- `Frost Nova`

`Frost Nova` now freezes enemies in place and uses clear icy visuals rather than acting like a burst-damage kill button.

## Score System

`ScoreManager` is a scene singleton.

Enemies call:

```csharp
ScoreManager.Instance.AddScore(points);
```

`ScoreTextUI` listens for score changes and updates the TMP score text.

## XP and Leveling

`EnemyHealth` drops XP gems on death.

`PlayerPickupCollector` attracts nearby XP gems.

`XPGem` calls:

```csharp
PlayerExperience.AddExperience(amount);
```

`PlayerExperience` handles level thresholds and upgrade choices.

Default values:

```text
First level-up: 10 XP
Growth: 1.25x per level
Choices per level: 3
```

## Upgrade System

Default upgrades include:

- `Sharpened Rounds`
- `Trigger Rhythm`
- `Fleet Footing`
- `Magnetic Field`
- `Split Shot`
- `Punch Through`
- `Vital Core`
- `Volatile Payload`

## Survival Pickups

Current pickup types:

- `HealthPickup`
- `MagnetPickup`
- `BombPickup`

Pickups use shared attraction/collection behavior through `PlayerPickup`.

## HUD and Timer

Current gameplay HUD supports:

- score
- HP
- XP / level
- run timer
- bomb cooldown
- secondary-skill cooldown

`RunTimer` uses scaled `Time.deltaTime`, so it pauses during popups that set `Time.timeScale = 0`.

## Enemy Spawning

`EnemyRespawnManager` uses authored `EnemySpawnPoint` objects.

Wave stages can now also choose allowed spawn regions such as:

- `North`
- `East`
- `South`
- `West`
- `Center`

## Elite Enemies

`EliteSpawnDirector` spawns occasional elite enemies using the current run timer.

Elites reuse enemy prefabs and get runtime modifiers through `EliteEnemy`.

Elite spawn and defeat can display messages through `RunAnnouncementUI`.

## Dragon Boss

The current first boss is a dragon.

The boss flow supports:

- timed spawn
- boss-only spawn logic from above / north of the player
- optional authored `BossSpawnPoint` anchors
- phase two below 50% HP
- world-space boss HP bar
- boss reward popup on death

## Audio System

`GameAudio` loads clips from `Assets/Resources/Audio/SFX/...`.

If a folder contains multiple clips, one is chosen at random at runtime.

This is used for:

- primary shooting
- pickups
- bombs
- `E` skills
- elite events
- enemy death
