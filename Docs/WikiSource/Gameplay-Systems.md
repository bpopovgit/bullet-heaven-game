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
- `Poison`

Call `DamagePacket.Clamp()` before applying damage from configurable data.

## Player Shooting

`PlayerShooting` reads the active `WeaponDefinition`, mouse position, and `PlayerStats`.

Upgrade-aware stats:

- damage multiplier
- fire rate
- projectile count
- pierce
- splash radius

Extra projectiles are fired as spread shots.

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

Default upgrades:

- `Sharpened Rounds`: +15% damage
- `Trigger Rhythm`: +15% fire rate
- `Fleet Footing`: +10% movement speed
- `Magnetic Field`: +1.5 pickup radius
- `Split Shot`: +1 projectile
- `Punch Through`: +1 pierce
- `Vital Core`: +20 max HP and heal 20
- `Volatile Payload`: +0.5 splash radius

Upgrades modify either:

- `PlayerStats`
- `PlayerHealth`

## Level-Up Popup

`LevelUpManager` pauses the game with `Time.timeScale = 0`, shows three choices, applies the clicked upgrade, hides the panel, then restores time scale.

If `LevelUpManager` is missing or not configured, `PlayerExperience` auto-picks an upgrade so the game remains playable.

## Survival Pickups

Pickups share movement and attraction behavior through `PlayerPickup`.

Current pickup types:

- `HealthPickup`: heals the player.
- `MagnetPickup`: pulls every active XP gem to the player.
- `BombPickup`: damages enemies near the player.

Enemy pickup drops are configured on `EnemyHealth`.

If no custom prefab is assigned, the game creates simple runtime pickups:

- red health pickup
- cyan magnet pickup
- yellow/orange bomb pickup

## Health UI

`PlayerHealth` raises `HealthChanged` when HP or max HP changes.

`PlayerHealthUI` can show:

- HP text
- HP slider

## Run Timer

`RunTimer` tracks active survival time using scaled `Time.deltaTime`.

It advances during gameplay, pauses during level-up choices, and stops when the player dies.

Useful events:

- `TimeChanged`
- `WholeSecondChanged`
- `MinuteChanged`
- `RunEnded`

`RunTimerUI` displays:

```text
Time: 00:00
```

## Enemy Spawning

`EnemyRespawnManager`:

- keeps enemies up to `maxAlive`
- respawns after `respawnDelay`
- uses `EnemySpawnPoint` positions
- avoids spawning too close to the player
- avoids spawning too close to living enemies
- can prefer the farthest valid spawn point

Dynamic spawn-radius mode exists in code but is currently disabled for easier balancing.

## Wave Director

`EnemyWaveDirector` listens to `RunTimer.WholeSecondChanged` and applies stages to `EnemyRespawnManager`.

Each stage can configure:

- start time in seconds
- max alive enemies
- respawn delay
- immediate refill
- optional enemy prefab pool

Default stages:

```text
0:00  maxAlive 8   respawnDelay 4.0
1:00  maxAlive 10  respawnDelay 3.5
2:00  maxAlive 12  respawnDelay 3.0
3:00  maxAlive 15  respawnDelay 2.5
```

## Elite Enemies

`EliteSpawnDirector` spawns occasional elite enemies using the current run timer.

Elites reuse normal enemy prefabs and get runtime modifiers through `EliteEnemy`.

Default behavior:

```text
First elite: 90 seconds
Interval: 90 seconds
Max elites alive: 1
Health multiplier: 4x
Reward multiplier: 5x
Scale multiplier: 1.4x
Tint: gold
```

If `Elite Prefabs` is empty, the director uses the respawn manager's current enemy pool.

Elite spawn and defeat can display messages through `RunAnnouncementUI`.

Default messages:

```text
ELITE INCOMING
ELITE DEFEATED
```

## Player Status Effects

`StatusReceiver` handles:

- slow
- shock/stun
- burn
- poison

Burn and poison use `PlayerHealth.TakeDamageDirect()`.
