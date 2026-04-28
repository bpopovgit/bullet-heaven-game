# Content Authoring

## Creating Starter Faction Prefabs

Use the editor helper when you want quick placeholder faction prefabs:

```text
Tools > Bullet Heaven > Factions > Create Starter Prefabs
```

This creates:

```text
Assets/Resources/Prefabs/Factions/HumanAlly.prefab
Assets/Resources/Prefabs/Factions/AngelTestUnit.prefab
Assets/Resources/Prefabs/Factions/DemonTestUnit.prefab
Assets/Resources/Prefabs/Factions/ZombieTestUnit.prefab
```

It also creates simple marker sprites under:

```text
Assets/Art/Sprites/Factions/
```

`AllySquadSpawner` automatically tries to load:

```text
Resources/Prefabs/Factions/HumanAlly
```

If that prefab exists, the starting Human ally squad uses it. If it does not exist, the game falls back to generated runtime circles.

These are still placeholder prefabs. Replace their sprites and tuning as the faction art/design becomes clearer.

## Adding a New Enemy Variant

1. Duplicate an existing enemy prefab from:

```text
Assets/Prefabs/Enemies/Melee/
Assets/Prefabs/Enemies/Ranged/
```

2. Rename it clearly:

```text
MeleeEnemy_Arcane
RangedEnemy_FireElite
```

3. Configure components:

- `EnemyHealth`
- `EnemyResistances` if needed
- `EnemyMovement` or `RangedShooter`
- `EnemyMeleeDamage` for contact attackers
- projectile prefab if ranged

4. Add the prefab to `EnemyRespawnManager.enemyPrefabs`.

To introduce the enemy later in a run, add it to an `EnemyWaveDirector` stage's `Enemy Prefabs` array instead.

To allow an enemy to spawn as an elite, add the prefab to `EliteSpawnDirector.Elite Prefabs`.

## Adding or Repositioning Spawn Points

Regular enemy spawns currently rely on authored `EnemySpawnPoint` objects.

Each spawn point can now be grouped into regions such as:

- `North`
- `East`
- `South`
- `West`
- `Center`

Use this when shaping wave flow per map.

Boss spawns should use `BossSpawnPoint` if you want authored entrances. If none exist, the boss falls back to spawning north / above the player.

## Adding Enemy Resistances

Use `EnemyResistances.overrides`.

Suggested multipliers:

- `0.5`: strong resistance
- `0.75`: light resistance
- `1.0`: normal
- `1.25`: light weakness
- `1.5`: strong weakness

## Adding a New Weapon

1. Create a weapon asset:

```text
Create > Game > Weapon Definition
```

2. Put it under:

```text
Assets/GameData/Weapon Definition/
```

3. Configure:

- `Display Name`
- `Element`
- `On Hit Effect`
- `Effect Chance`
- `Status Duration`
- `Status Strength`
- `Base Damage`
- `Shots Per Second`
- `Bullet Speed`
- `Splash Radius`
- `Pierce`
- `Bullet Prefab`

4. If the weapon should be selectable in the loadout, update:

- `RunLoadoutState`
- `RunLoadoutApplier`
- any matching visuals / SFX / descriptions

## Adding a New Bomb

To add a new bomb family:

1. Extend `StartingBombChoice` in `RunLoadoutState`.
2. Add display name and description there.
3. Add bomb config in `PlayerActiveBomb`.
4. Add or update visuals in:

```text
Assets/Scripts/PlayerScripts/BombAbilityDefinition.cs
Assets/Scripts/PlayerScripts/BombExplosionVisual.cs
Assets/Scripts/PlayerScripts/PlayerBombProjectile.cs
```

5. Add SFX folders under:

```text
Assets/Resources/Audio/SFX/BombThrow
Assets/Resources/Audio/SFX/BombImpact
```

If the new bomb needs unique SFX logic beyond shared throw / impact folders, update `GameAudio`.

## Adding a New Secondary Skill on `E`

1. Extend `StartingSkillChoice` in `RunLoadoutState`.
2. Add name and description there.
3. Add a config entry in `PlayerSecondaryActiveSkill.CreateConfig()`.
4. Implement the effect branch in `PlayerSecondaryActiveSkill`.
5. Add icon/color behavior if needed.
6. Add a dedicated SFX folder if the skill needs its own sound.

Current examples:

- `Magnetic Pulse`
- `Arcane Shield`
- `Frost Nova`

## Adding Player Upgrade Options

Current upgrade options are generated in:

```text
Assets/Scripts/PlayerScripts/PlayerUpgradeOption.cs
```

Add new default upgrades inside `CreateDefaultPool()`.

If the upgrade needs a new stat:

1. Add a new value to `PlayerUpgradeType`.
2. Add a field/method in `PlayerStats` or `PlayerHealth`.
3. Add a case in `PlayerUpgradeOption.Apply()`.
4. Wire that stat into the relevant gameplay script.
5. Update documentation.

## Adding a New Status Effect

1. Add the enum value in `DamageType.cs`.
2. Add handling in `StatusReceiver.ApplyStatus()`.
3. Implement the effect routine.
4. Add optional visuals or VFX.
5. Update any weapons, projectiles, bombs, or skills that should apply it.

## Adding Custom XP Gem or Pickup Prefabs

Runtime placeholders are fine for prototyping, but custom prefabs give better visuals.

Common requirements:

- `SpriteRenderer`
- trigger `Collider2D`
- optional kinematic `Rigidbody2D`
- matching pickup or gem script

Relevant components:

- `XPGem`
- `HealthPickup`
- `MagnetPickup`
- `BombPickup`

Assign these prefabs on `EnemyHealth`.

## Adding Enemy Projectiles

1. Duplicate an existing projectile prefab from:

```text
Assets/Prefabs/Projectiles/Enemy/
```

2. Configure `EnemyProjectile`:

- speed
- damage
- lifetime
- walls mask
- element
- status
- status duration
- status strength

3. Assign it to the ranged enemy's `RangedShooter.enemyProjectilePrefab`.

## Adding or Updating SFX

Gameplay SFX are loaded from:

```text
Assets/Resources/Audio/SFX/
```

Each folder can contain one or many clips. If multiple clips exist, `GameAudio` randomly chooses one at runtime.

Examples:

```text
PlayerShoot
BombThrow
BombImpact
SkillMagneticPulse
SkillArcaneShield
SkillFrostNova
EnemyDeath
EliteSpawn
EliteDefeated
```

## Naming Conventions

Suggested asset naming:

```text
WD_Pistol_Physical
MeleeEnemy_Fire
RangedEnemy_Frost
EnemyProjectile_Lightning
Hit_Poison
Spawn_01
BossSpawn_North
```

Suggested script naming:

```text
SystemName.cs
PlayerThing.cs
EnemyThing.cs
ThingUI.cs
```

Keep script names and class names identical.
