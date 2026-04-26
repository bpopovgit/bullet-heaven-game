# Project Structure

## Main Folders

```text
Assets/
  Art/
  Audio/
  GameData/
  Prefabs/
  Resources/
  Scenes/
  Scripts/
  TextMesh Pro/
  UIAssets/
  VFX/
Packages/
ProjectSettings/
Docs/
```

## Script Groups

### Combat

```text
Assets/Scripts/Combat/
```

- `DamageType.cs`
- `WeaponDefinition.cs`
- `BulletElemental.cs`
- `EnemyHealth.cs`
- `EnemyResistances.cs`
- `EliteEnemy.cs`
- `DragonBoss.cs`
- `BossWorldHealthBar.cs`
- `EnemyRespawnManager.cs`
- `EnemySpawnPoint.cs`
- `StatusReceiver.cs`
- `XPGem.cs`

### Player

```text
Assets/Scripts/PlayerScripts/
```

- `PlayerMovement.cs`
- `PlayerShooting.cs`
- `PlayerHealth.cs`
- `PlayerExperience.cs`
- `PlayerStats.cs`
- `PlayerUpgradeOption.cs`
- `PlayerPickupCollector.cs`
- `PlayerActiveBomb.cs`
- `PlayerBombProjectile.cs`
- `PlayerSecondaryActiveSkill.cs`

### Game Systems

```text
Assets/Scripts/GameSystems/
```

- `RunTimer.cs`
- `RunLoadoutState.cs`
- `RunLoadoutApplier.cs`
- `EnemyWaveDirector.cs`
- `EliteSpawnDirector.cs`
- `BossSpawnDirector.cs`
- `BossSpawnPoint.cs`
- `GameAudio.cs`
- `PlaySessionLogWriter.cs`

### Enemies

```text
Assets/Scripts/EnemyScripts/
```

- `EnemyMovement.cs`
- `EnemyMeleeDamage.cs`
- `RangedShooter.cs`
- `EnemyProjectile.cs`

### Pickups

```text
Assets/Scripts/Pickups/
```

- `PlayerPickup.cs`
- `HealthPickup.cs`
- `MagnetPickup.cs`
- `BombPickup.cs`
- `PickupSpriteFactory.cs`

### UI

```text
Assets/Scripts/UIScripts/
```

- `MainMenuRuntime.cs`
- `ScoreManager.cs`
- `ScoreTextUI.cs`
- `PlayerHealthUI.cs`
- `ExperienceUI.cs`
- `RunTimerUI.cs`
- `RunAnnouncementUI.cs`
- `LevelUpManager.cs`
- `BombCooldownUI.cs`
- `SecondarySkillCooldownUI.cs`

## Important Runtime Asset Areas

```text
Assets/Resources/Audio/SFX/
Assets/Prefabs/Enemies/Melee/
Assets/Prefabs/Enemies/Ranged/
Assets/Prefabs/Projectiles/Enemy/
Assets/VFX/Hit/
```
