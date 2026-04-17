# Project Structure

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

## Script Groups

### Combat

```text
Assets/Scripts/Combat/
```

- `DamageType.cs`: `DamageElement`, `StatusEffect`, and `DamagePacket`.
- `WeaponDefinition.cs`: ScriptableObject data for player weapons.
- `BulletElemental.cs`: player bullet behavior.
- `EnemyHealth.cs`: enemy health, score reward, XP drop, and death event.
- `EnemyResistances.cs`: element-specific enemy damage multipliers.
- `StatusReceiver.cs`: burn, poison, slow, shock/stun handling.
- `EnemyRespawnManager.cs`: enemy respawn/cap logic.
- `EnemySpawnPoint.cs`: spawn point marker.
- `XPGem.cs`: XP pickup behavior.

### Player

```text
Assets/Scripts/PlayerScripts/
```

- `PlayerMovement.cs`: Rigidbody2D movement.
- `PlayerShooting.cs`: mouse-aimed shooting.
- `PlayerHealth.cs`: HP, i-frames, knockback, death, healing, max HP upgrades.
- `PlayerExperience.cs`: XP, levels, and upgrade choice requests.
- `PlayerStats.cs`: runtime upgrade bonuses.
- `PlayerPickupCollector.cs`: XP gem attraction radius.
- `PlayerUpgradeOption.cs`: upgrade data and application logic.

### Game Systems

```text
Assets/Scripts/GameSystems/
```

- `RunTimer.cs`: active survival timer and time events.
- `EnemyWaveDirector.cs`: timed spawn-stage controller.

### Enemies

```text
Assets/Scripts/EnemyScripts/
```

- `EnemyMovement.cs`: simple chase behavior.
- `EnemyMeleeDamage.cs`: contact damage and statuses.
- `RangedShooter.cs`: ranged movement and shooting.
- `EnemyProjectile.cs`: enemy projectile behavior.

### Pickups

```text
Assets/Scripts/Pickups/
```

- `PlayerPickup.cs`: shared attraction and collection behavior.
- `HealthPickup.cs`: heals the player.
- `MagnetPickup.cs`: pulls active XP gems to the player.
- `BombPickup.cs`: damages enemies near the player.
- `PickupSpriteFactory.cs`: creates simple runtime placeholder pickup visuals.

### UI

```text
Assets/Scripts/UIScripts/
```

- `ScoreManager.cs`: score state and score-changed event.
- `ScoreTextUI.cs`: TMP score display.
- `ExperienceUI.cs`: optional XP/level display.
- `PlayerHealthUI.cs`: optional HP display.
- `RunTimerUI.cs`: optional survival timer display.
- `LevelUpManager.cs`: level-up choice popup.
- `CursorScript.cs`: custom cursor setup.

## Important Prefab Areas

```text
Assets/Prefabs/Enemies/Melee/
Assets/Prefabs/Enemies/Ranged/
Assets/Prefabs/Projectiles/Enemy/
Assets/VFX/Hit/
```
