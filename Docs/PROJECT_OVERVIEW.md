# Project Overview

## Summary

`Bullet Heaven Game` is a Unity 2D top-down bullet-heaven project built around short survival runs, elemental combat, timed enemy pressure, boss encounters, pre-run loadouts, and active abilities.

The current game loop supports:

- top-down movement with the new Input System
- mouse-aimed primary shooting
- a pre-run menu and loadout flow
- a chosen starting weapon, bomb, secondary skill, and passive
- level-ups with upgrade choices
- score, XP, pickups, elites, and a dragon boss
- run timer, announcements, and combat SFX

## Current Unity Version

```text
Unity 6000.3.14f1
```

## Main Folders

```text
Assets/
  Art/                 Environment sprites and visual art.
  Audio/               Optional imported raw audio assets.
  GameData/            ScriptableObject data such as tiles and weapons.
  Prefabs/             Enemies, bullets, projectiles, and prefab content.
  Resources/           Runtime-loaded assets such as SFX folders.
  Scenes/              Scene files such as Main and SampleScene.
  Scripts/             Combat, player, game-system, pickup, and UI scripts.
  TextMesh Pro/        TMP package assets and examples.
  UIAssets/            UI source assets.
  VFX/                 Runtime and authored hit / visual prefabs.
Packages/             Unity package manifest and lock data.
ProjectSettings/      Unity project configuration.
Docs/                 Repository source-of-truth documentation.
```

## Main Scene Flow

The project currently uses a two-scene flow:

```text
Assets/Scenes/Main.unity
Assets/Game.unity
```

- `Main.unity` is the front-end entry scene.
- `Game.unity` is the active gameplay scene.

`MainMenuRuntime` builds the menu UI at runtime inside `Main.unity`, including:

- mode selection
- single-player setup
- loadout setup

`RunLoadoutState` stores the selected starting kit, and `RunLoadoutApplier` applies that kit when `Game.unity` loads.

## Main Script Groups

### Combat

```text
Assets/Scripts/Combat/
```

Shared combat and enemy-state systems:

- `DamageType.cs`: defines `DamageElement`, `StatusEffect`, and `DamagePacket`.
- `WeaponDefinition.cs`: ScriptableObject weapon data used by player shooting.
- `BulletElemental.cs`: player bullet behavior using weapon and player stat data.
- `EnemyHealth.cs`: enemy HP, score reward, XP reward, pickup drops, and status application.
- `EnemyResistances.cs`: element-specific enemy damage multipliers.
- `EliteEnemy.cs`: runtime elite enemy modifier.
- `DragonBoss.cs`: boss behavior extension and phase logic.
- `BossWorldHealthBar.cs`: world-space boss HP bar above the dragon.
- `EnemyRespawnManager.cs`: manages living enemy cap and spawn attempts.
- `EnemySpawnPoint.cs`: spawn point marker with region grouping.
- `StatusReceiver.cs`: shared status handling, including slow, burn, poison, shock, and freeze visuals.
- `XPGem.cs`: XP pickup behavior.

### Player

```text
Assets/Scripts/PlayerScripts/
```

Player combat, survivability, run growth, and active abilities:

- `PlayerMovement.cs`: Input System movement through Rigidbody2D.
- `PlayerShooting.cs`: mouse-aimed shooting from the player muzzle/fire point.
- `PlayerHealth.cs`: HP, healing, max HP upgrades, i-frames, death, and temporary invulnerability.
- `PlayerExperience.cs`: XP thresholds, levels, upgrade requests, and pending level-ups.
- `PlayerStats.cs`: runtime stat modifiers such as damage, fire rate, projectile count, pickup radius, and movement speed.
- `PlayerUpgradeOption.cs`: upgrade option generation and application logic.
- `PlayerPickupCollector.cs`: attraction of XP gems and pickups within range.
- `PlayerActiveBomb.cs`: `Q` active skill setup and usage.
- `PlayerBombProjectile.cs`: cursor-targeted bomb projectile behavior.
- `BombAbilityDefinition.cs`: bomb configuration data container.
- `BombExplosionVisual.cs`: runtime bomb impact visuals.
- `PlayerSecondaryActiveSkill.cs`: `E` active skill system.
- `SecondaryActiveSkillDefinition.cs`: secondary skill configuration data container.
- `SecondarySkillVisual.cs`: runtime secondary-skill visuals.

### Game Systems

```text
Assets/Scripts/GameSystems/
```

Run-level orchestration, menu handoff, audio, and logging:

- `RunTimer.cs`: tracks scaled active run time and raises time events.
- `RunLoadoutState.cs`: static selected loadout state from the menu.
- `RunLoadoutApplier.cs`: applies starting weapon, bomb, skill, and passive when gameplay loads.
- `EnemyWaveDirector.cs`: timed enemy stage controller.
- `EliteSpawnDirector.cs`: timed elite enemy spawner.
- `BossSpawnDirector.cs`: dragon boss spawn flow and boss reward trigger.
- `BossSpawnPoint.cs`: optional authored boss-only spawn anchors.
- `GameAudio.cs`: runtime-loaded SFX system with random variation per folder.
- `PlaySessionLogWriter.cs`: writes one play-session log file per run for debugging.

### Enemy Scripts

```text
Assets/Scripts/EnemyScripts/
```

Enemy movement, contact damage, and ranged behavior:

- `EnemyMovement.cs`: chase movement for melee enemies and any simple movers.
- `EnemyMeleeDamage.cs`: contact damage and status application to the player.
- `RangedShooter.cs`: ranged spacing, movement, and projectile firing.
- `EnemyProjectile.cs`: enemy projectile damage and status application.

### Pickups

```text
Assets/Scripts/Pickups/
```

Pickup attraction, collection, and placeholder visual generation:

- `PlayerPickup.cs`: shared collection and attraction behavior.
- `HealthPickup.cs`: heals the player.
- `MagnetPickup.cs`: pulls active XP gems to the player.
- `BombPickup.cs`: damages enemies near the player.
- `PickupSpriteFactory.cs`: creates runtime placeholder pickup sprites.

### UI

```text
Assets/Scripts/UIScripts/
```

HUD, popups, cooldown widgets, and menu runtime:

- `MainMenuRuntime.cs`: runtime-generated front-end and loadout UI.
- `ScoreManager.cs`: singleton score state and score-changed event.
- `ScoreTextUI.cs`: TMP score display.
- `PlayerHealthUI.cs`: HP text and optional HP slider.
- `ExperienceUI.cs`: level text, XP text, and optional XP slider.
- `RunTimerUI.cs`: run timer display.
- `RunAnnouncementUI.cs`: temporary on-screen event messages.
- `LevelUpManager.cs`: level-up choice popup.
- `BombCooldownUI.cs`: bottom-left bomb cooldown icon and timer.
- `SecondarySkillCooldownUI.cs`: bottom-left `E` skill cooldown icon and timer.
- `CursorScript.cs`: custom cursor setup.

## Important Runtime Asset Areas

```text
Assets/Resources/Audio/SFX/
Assets/Prefabs/Enemies/Melee/
Assets/Prefabs/Enemies/Ranged/
Assets/Prefabs/Projectiles/Enemy/
Assets/VFX/Hit/
```

`GameAudio` loads clips from `Assets/Resources/Audio/SFX/...`, so gameplay SFX should live under those runtime-readable folders rather than only under `Assets/Audio/`.

## Current Gameplay Scene Expectations

The gameplay scene should contain or auto-bootstrap the following:

- `Player`
- `ScoreManager`
- `RunTimer`
- `EnemyRespawnManager`
- `EnemyWaveDirector`
- `EliteSpawnDirector`
- `BossSpawnDirector`
- `Canvas` with score / HP / XP / timer UI
- `LevelUpManager`
- `EnemySpawnPoint` objects
- optional `BossSpawnPoint` objects
- `RunAnnouncementUI`

The menu scene should contain:

- the `Main` scene itself
- an `EventSystem` or none at all (the runtime menu will create one if missing)
- no manually-authored menu canvas is required for the current flow
