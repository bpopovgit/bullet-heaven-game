# Content Authoring

## New Enemy Variant

1. Duplicate an enemy prefab from:

```text
Assets/Prefabs/Enemies/Melee/
Assets/Prefabs/Enemies/Ranged/
```

2. Rename it clearly.
3. Configure:

- `EnemyHealth`
- `EnemyResistances`
- `EnemyMovement` or `RangedShooter`
- projectile prefab if ranged

4. Add it to `EnemyRespawnManager.enemyPrefabs` or to a specific wave stage.

## Spawn Authoring

Regular enemies use `EnemySpawnPoint`.

Bosses can use `BossSpawnPoint`.

Use directional spawn regions when shaping wave flow by map.

## New Weapon

Create:

```text
Create > Game > Weapon Definition
```

Recommended folder:

```text
Assets/GameData/Weapon Definition/
```

If the weapon should be selectable in the loadout, also update:

- `RunLoadoutState`
- `RunLoadoutApplier`

## New Bomb

Extend:

- `StartingBombChoice`
- `RunLoadoutState`
- `PlayerActiveBomb`

Add or update bomb visuals and SFX as needed.

## New Secondary Skill

Extend:

- `StartingSkillChoice`
- `RunLoadoutState`
- `PlayerSecondaryActiveSkill`

If the skill needs its own sound, add a matching folder under `Assets/Resources/Audio/SFX/`.

## New Status Effect

1. Add a value to `StatusEffect` in `DamageType.cs`.
2. Add handling in `StatusReceiver.ApplyStatus()`.
3. Implement the effect routine.
4. Update documentation.

## SFX Authoring

Gameplay SFX are loaded from:

```text
Assets/Resources/Audio/SFX/
```

You can place multiple clips in one folder. The game will pick one at random.
