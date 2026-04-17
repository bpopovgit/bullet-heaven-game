# Project Overview

## Summary

`Bullet Heaven Game` is a Unity 2D top-down bullet-heaven prototype. The project currently supports movement, shooting, elemental damage, enemy spawning, enemy contact/projectile damage, score, XP gems, pickup radius, level-ups, and upgrade choices.

## Main Folders

```text
Assets/
  Art/                 Environment sprites and visual art.
  Audio/               Audio assets.
  GameData/            ScriptableObject data such as tiles and weapons.
  Prefabs/             Player bullets, enemies, enemy projectiles, and VFX prefabs.
  Scenes/              Unity scene files.
  Scripts/             Gameplay, combat, enemy, player, and UI scripts.
  TextMesh Pro/        TMP package assets and examples.
  UIAssets/            UI source assets.
  VFX/                 Hit VFX prefabs.
Packages/             Unity package manifest and lock data.
ProjectSettings/      Unity project configuration.
```

## Main Script Groups

```text
Assets/Scripts/Combat/
```

Core combat and shared gameplay systems:

- `DamageType.cs`: defines `DamageElement`, `StatusEffect`, and `DamagePacket`.
- `WeaponDefinition.cs`: ScriptableObject data for player weapons.
- `BulletElemental.cs`: player bullet behavior using weapon and player stat data.
- `EnemyHealth.cs`: enemy health, score reward, XP drop, and death event.
- `EliteEnemy.cs`: runtime elite enemy modifier.
- `EnemyResistances.cs`: element-specific enemy damage multipliers.
- `StatusReceiver.cs`: burn, poison, slow, shock/stun handling.
- `EnemyRespawnManager.cs`: keeps enemies spawned up to a cap.
- `EnemySpawnPoint.cs`: marker component for spawn points.
- `XPGem.cs`: XP pickup behavior.

```text
Assets/Scripts/PlayerScripts/
```

Player movement, shooting, health, XP, and upgrades:

- `PlayerMovement.cs`: Input System movement through Rigidbody2D.
- `PlayerShooting.cs`: mouse-aimed shooting from the muzzle/fire point.
- `PlayerHealth.cs`: HP, i-frames, knockback, death, restart, healing, max HP upgrades.
- `PlayerExperience.cs`: XP, levels, upgrade choice requests.
- `PlayerStats.cs`: runtime stat bonuses from upgrades.
- `PlayerPickupCollector.cs`: attracts XP gems within pickup radius.
- `PlayerUpgradeOption.cs`: upgrade option data and application logic.

```text
Assets/Scripts/GameSystems/
```

Run-level systems:

- `RunTimer.cs`: tracks active survival time and exposes time events for UI and future wave pacing.
- `EnemyWaveDirector.cs`: applies timed spawn stages to `EnemyRespawnManager`.
- `EliteSpawnDirector.cs`: spawns boosted elite enemies on timed intervals.

```text
Assets/Scripts/EnemyScripts/
```

Enemy behavior:

- `EnemyMovement.cs`: basic chase movement.
- `EnemyMeleeDamage.cs`: contact damage and status application.
- `RangedShooter.cs`: ranged enemy movement and shooting.
- `EnemyProjectile.cs`: enemy projectile damage and status application.

```text
Assets/Scripts/Pickups/
```

Pickup behavior:

- `PlayerPickup.cs`: shared attraction and collection behavior.
- `XPGem.cs`: XP pickup behavior lives in Combat but uses `PlayerPickup`.
- `HealthPickup.cs`: heals the player.
- `MagnetPickup.cs`: pulls active XP gems to the player.
- `BombPickup.cs`: damages enemies near the player.
- `PickupSpriteFactory.cs`: creates simple runtime placeholder pickup visuals.

```text
Assets/Scripts/UIScripts/
```

UI and scene managers:

- `ScoreManager.cs`: singleton score state and score-changed event.
- `ScoreTextUI.cs`: TMP score display.
- `ExperienceUI.cs`: optional XP/level display.
- `PlayerHealthUI.cs`: optional HP display.
- `RunTimerUI.cs`: optional survival timer display.
- `RunAnnouncementUI.cs`: temporary run-event announcement display.
- `LevelUpManager.cs`: optional level-up choice popup.
- `CursorScript.cs`: custom cursor setup.

## Important Prefab Areas

```text
Assets/Prefabs/Enemies/Melee/
Assets/Prefabs/Enemies/Ranged/
Assets/Prefabs/Projectiles/Enemy/
Assets/VFX/Hit/
```

Enemy prefabs should include the appropriate movement/damage script, `EnemyHealth`, and optional `EnemyResistances`.

## Current Scene Setup Expectations

The gameplay scene should contain:

- `Player`
- `ScoreManager`
- `RunTimer`
- `EnemyWaveDirector`
- `EliteSpawnDirector`
- `EnemyRespawnManager` or equivalent enemy system object
- `Canvas` with score UI and optional level/XP UI
- Optional `LevelUpManager`
- `EnemySpawnPoint` objects
- Camera and virtual camera setup
