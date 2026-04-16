# Content Authoring

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

- `EnemyHealth`: health, score, XP.
- `EnemyResistances`: optional element multipliers.
- `EnemyMeleeDamage` or `RangedShooter`: attack behavior.
- `EnemyProjectile`: only on projectile prefabs.

4. Add the prefab to `EnemyRespawnManager.enemyPrefabs` in the scene.

## Adding Enemy Resistances

Use `EnemyResistances.overrides`.

Examples:

```text
Fire enemy:
  Fire = 0.5
  Frost = 1.5

Poison enemy:
  Poison = 0.5
  Fire = 1.25
```

Keep multipliers easy to understand at first:

- `0.5`: strong resistance
- `0.75`: light resistance
- `1.0`: normal
- `1.25`: light weakness
- `1.5`: strong weakness

## Adding a New Weapon

1. In the Project window, create a weapon asset:

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

4. Assign it to `PlayerShooting.weapon` on the Player.

## Adding Player Upgrade Options

Current upgrade options are generated in:

```text
Assets/Scripts/PlayerScripts/PlayerUpgradeOption.cs
```

Add new default upgrades inside `CreateDefaultPool()`.

Example:

```csharp
new PlayerUpgradeOption(
    "Heavy Caliber",
    "+25% damage, slower shots",
    PlayerUpgradeType.DamagePercent,
    amount: 0.25f)
```

If the upgrade needs a new stat:

1. Add a new value to `PlayerUpgradeType`.
2. Add a field/method in `PlayerStats` or `PlayerHealth`.
3. Add a case in `PlayerUpgradeOption.Apply()`.
4. Wire that stat into the relevant gameplay script.
5. Document the upgrade in `Docs/GAMEPLAY_SYSTEMS.md`.

## Adding a New Status Effect

1. Add the enum value in `DamageType.cs`.
2. Add handling in `StatusReceiver.ApplyStatus()`.
3. Implement the effect routine.
4. Add optional VFX fields if needed.
5. Update enemy projectiles or weapons to apply the new status.

## Adding a Custom XP Gem Prefab

The runtime default gem is useful for testing, but a prefab gives better visuals.

Prefab requirements:

- `SpriteRenderer`
- `Collider2D` set as trigger
- optional `Rigidbody2D` set to kinematic
- `XPGem`

Then assign the prefab to:

```text
EnemyHealth > Experience Gem Prefab
```

If this field is empty, the default runtime green gem is used.

## Adding Custom Survival Pickup Prefabs

Custom pickup prefabs are optional. If these fields are empty on `EnemyHealth`, runtime placeholder pickups are spawned.

Health pickup prefab requirements:

- `SpriteRenderer`
- trigger `Collider2D`
- optional kinematic `Rigidbody2D`
- `HealthPickup`

Magnet pickup prefab requirements:

- `SpriteRenderer`
- trigger `Collider2D`
- optional kinematic `Rigidbody2D`
- `MagnetPickup`

Bomb pickup prefab requirements:

- `SpriteRenderer`
- trigger `Collider2D`
- optional kinematic `Rigidbody2D`
- `BombPickup`

Assign custom prefabs on `EnemyHealth`:

```text
Health Pickup Prefab
Magnet Pickup Prefab
Bomb Pickup Prefab
```

## Adding a New Enemy Projectile

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

3. Assign it to a ranged enemy's `RangedShooter.enemyProjectilePrefab`.

## Naming Conventions

Suggested asset naming:

```text
WD_Pistol_Physical
MeleeEnemy_Fire
RangedEnemy_Frost
EnemyProjectile_Lightning
Hit_Poison
Spawn_01
```

Suggested script naming:

```text
SystemName.cs
PlayerThing.cs
EnemyThing.cs
ThingUI.cs
```

Keep script names and class names identical.
