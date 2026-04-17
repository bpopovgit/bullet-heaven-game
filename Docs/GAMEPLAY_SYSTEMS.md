# Gameplay Systems

## Damage Model

Damage is passed through `DamagePacket`, defined in `DamageType.cs`.

`DamagePacket` includes:

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

`DamagePacket.Clamp()` should be called before applying damage when a packet is built from configurable data.

## Player Shooting

`PlayerShooting` reads:

- `WeaponDefinition`
- `PlayerStats`
- mouse position from the Input System

It spawns `weapon.bulletPrefab` from the configured muzzle/fire point. Extra projectile upgrades create spread shots using `multiProjectileSpreadAngle`.

Current upgrade-aware shooting stats:

- fire rate
- projectile count
- damage multiplier
- pierce
- splash radius

## Weapon Definitions

`WeaponDefinition` is a ScriptableObject created from:

```text
Create > Game > Weapon Definition
```

Important fields:

- `displayName`
- `element`
- `onHitEffect`
- `effectChance`
- `statusDuration`
- `statusStrength`
- `baseDamage`
- `shotsPerSecond`
- `bulletSpeed`
- `splashRadius`
- `pierce`
- `bulletPrefab`

## Enemy Health, Score, and XP Drops

`EnemyHealth` controls:

- max health
- death state
- score reward
- XP reward
- optional XP gem prefab
- optional health, magnet, and bomb pickup drops
- death event for respawn tracking

On death:

1. Adds score through `ScoreManager`.
2. Drops XP through `XPGem`.
3. Rolls optional survival pickup drops.
4. Raises `Died`.
5. Destroys the enemy GameObject.

If no XP prefab is assigned, `XPGem.SpawnDefault()` creates a simple green gem at runtime.

## Survival Pickups

Pickups share attraction behavior through `PlayerPickup`.

Current pickup types:

- `HealthPickup`: heals the player.
- `MagnetPickup`: attracts every active XP gem to the player.
- `BombPickup`: damages enemies in a radius around the player.

`EnemyHealth` controls pickup drop chances:

- `healthDropChance`
- `magnetDropChance`
- `bombDropChance`

If a pickup prefab is not assigned, the game spawns a simple colored runtime pickup:

- red: health
- cyan: magnet
- yellow/orange: bomb

`PlayerPickupCollector` attracts all `PlayerPickup` objects, including XP gems.

## Score System

`ScoreManager` is a scene-level singleton.

Core API:

```csharp
ScoreManager.Instance.AddScore(amount);
ScoreManager.Instance.ResetScore();
```

`ScoreTextUI` subscribes to `ScoreManager.ScoreChanged` and updates TMP text.

Scene requirements:

- Active `ScoreManager` GameObject.
- `ScoreTextUI` attached to a TMP text object.

## Health UI

`PlayerHealth` raises `HealthChanged` whenever HP or max HP changes.

`PlayerHealthUI` can display:

- HP text
- HP slider

It can auto-find `PlayerHealth` if the field is left empty.

## Run Timer

`RunTimer` tracks active survival time using scaled `Time.deltaTime`.

That means:

- it advances during normal gameplay
- it pauses during level-up choices because `Time.timeScale` is `0`
- it stops when the player dies

Core API:

```csharp
RunTimer.Instance.StartTimer();
RunTimer.Instance.StopTimer();
RunTimer.Instance.ResetTimer(startAfterReset: true);
RunTimer.FormatTime(seconds);
```

Events:

- `TimeChanged`: raised when the displayed whole second changes.
- `WholeSecondChanged`: useful for future wave/director logic.
- `MinuteChanged`: useful for difficulty milestones.
- `RunEnded`: raised when the timer stops.

`RunTimerUI` displays the time as:

```text
Time: 00:00
```

For runs longer than an hour, formatting becomes:

```text
Time: 1:00:00
```

## XP and Leveling

`PlayerExperience` tracks:

- current level
- current XP
- XP needed for the next level
- pending level-ups
- upgrade choices

Default first threshold:

```text
10 XP
```

Default growth:

```text
1.25x per level
```

When enough XP is collected, the player levels up. If `LevelUpManager` exists and is configured, the game pauses and shows upgrade choices. If not, the system auto-picks the first generated upgrade so leveling still works.

## XP Pickup Flow

1. Enemy dies and drops an XP gem.
2. `PlayerPickupCollector` finds nearby gems using `OverlapCircleNonAlloc`.
3. Gems inside range call `AttractTo(playerTransform)`.
4. `XPGem` moves toward the player.
5. On contact, `XPGem` calls `PlayerExperience.AddExperience()`.

`Magnetic Field` upgrades increase pickup radius through `PlayerStats.PickupRadiusBonus`.

## Upgrade System

Upgrade options are represented by `PlayerUpgradeOption`.

Current default pool:

- `Sharpened Rounds`: +15% damage
- `Trigger Rhythm`: +15% fire rate
- `Fleet Footing`: +10% movement speed
- `Magnetic Field`: +1.5 pickup radius
- `Split Shot`: +1 projectile
- `Punch Through`: +1 pierce
- `Vital Core`: +20 max HP and heal 20
- `Volatile Payload`: +0.5 splash radius

Upgrades currently modify either `PlayerStats` or `PlayerHealth`.

## Level-Up UI

`LevelUpManager` is optional but recommended.

When configured, it:

1. Stores the current `Time.timeScale`.
2. Sets `Time.timeScale = 0`.
3. Shows the level-up panel.
4. Fills choice buttons with upgrade titles and descriptions.
5. Applies the chosen upgrade.
6. Hides the panel.
7. Restores time scale.

## Enemy Spawning

`EnemyRespawnManager` maintains a maximum number of living enemies.

Current behavior:

- Uses configured `enemyPrefabs`.
- Uses configured or auto-found `EnemySpawnPoint` objects.
- Can prefer the farthest valid spawn point.
- Avoids spawning too close to the player.
- Avoids spawning too close to existing enemies.
- Respawns after a delay when an enemy dies.

Dynamic spawn-radius mode exists in the script but is currently disabled/commented for balancing simplicity.

## Wave Director

`EnemyWaveDirector` listens to `RunTimer.WholeSecondChanged` and applies timed stages to `EnemyRespawnManager`.

Each stage can set:

- start time in seconds
- `maxAlive`
- `respawnDelay`
- whether to fill to the new cap immediately
- optional enemy prefab pool

If a stage has an empty prefab pool, the current enemy prefab pool remains unchanged.

Default stages:

```text
0:00  maxAlive 8   respawnDelay 4.0
1:00  maxAlive 10  respawnDelay 3.5
2:00  maxAlive 12  respawnDelay 3.0
3:00  maxAlive 15  respawnDelay 2.5
```

This is the first step toward timed waves, elites, and bosses.

## Player Status Effects

`StatusReceiver` handles effects applied to the player:

- Slow modifies `SpeedMultiplier`.
- Shock sets `IsStunned` and forces movement speed to zero.
- Burn and poison deal damage over time through `PlayerHealth.TakeDamageDirect()`.
- Optional VFX references can play while effects are active.
