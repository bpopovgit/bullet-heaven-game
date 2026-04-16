# Content Authoring

## New Enemy Variant

1. Duplicate an enemy prefab from:

```text
Assets/Prefabs/Enemies/Melee/
Assets/Prefabs/Enemies/Ranged/
```

2. Rename it clearly:

```text
MeleeEnemy_FireElite
RangedEnemy_Poison
```

3. Configure:

- `EnemyHealth`
- `EnemyResistances`
- `EnemyMeleeDamage` or `RangedShooter`
- projectile prefab if ranged

4. Add it to `EnemyRespawnManager.enemyPrefabs`.

## Enemy Resistances

Use `EnemyResistances.overrides`.

Suggested multipliers:

- `0.5`: strong resistance
- `0.75`: light resistance
- `1.0`: normal
- `1.25`: light weakness
- `1.5`: strong weakness

## New Weapon

Create:

```text
Create > Game > Weapon Definition
```

Recommended folder:

```text
Assets/GameData/Weapon Definition/
```

Configure:

- display name
- element
- on-hit effect
- effect chance
- status duration
- status strength
- base damage
- shots per second
- bullet speed
- splash radius
- pierce
- bullet prefab

Assign the weapon asset to `PlayerShooting.weapon`.

## New Upgrade

Edit:

```text
Assets/Scripts/PlayerScripts/PlayerUpgradeOption.cs
```

Add a new entry to `CreateDefaultPool()`.

If the upgrade needs a new stat:

1. Add a new `PlayerUpgradeType`.
2. Add data/methods to `PlayerStats` or `PlayerHealth`.
3. Add a case in `PlayerUpgradeOption.Apply()`.
4. Wire the stat into the gameplay script that should use it.
5. Update documentation.

## New Status Effect

1. Add a value to `StatusEffect` in `DamageType.cs`.
2. Add handling in `StatusReceiver.ApplyStatus()`.
3. Implement the routine.
4. Add VFX fields if needed.
5. Configure weapons or enemy attacks to apply it.

## Custom XP Gem Prefab

Prefab requirements:

- `SpriteRenderer`
- trigger `Collider2D`
- optional kinematic `Rigidbody2D`
- `XPGem`

Assign it to:

```text
EnemyHealth > Experience Gem Prefab
```

## Custom Survival Pickups

Custom pickup prefabs are optional. If they are not assigned, runtime placeholder pickups are created.

Health pickup prefab:

- `SpriteRenderer`
- trigger `Collider2D`
- optional kinematic `Rigidbody2D`
- `HealthPickup`

Magnet pickup prefab:

- `SpriteRenderer`
- trigger `Collider2D`
- optional kinematic `Rigidbody2D`
- `MagnetPickup`

Bomb pickup prefab:

- `SpriteRenderer`
- trigger `Collider2D`
- optional kinematic `Rigidbody2D`
- `BombPickup`

Assign them on `EnemyHealth`.
