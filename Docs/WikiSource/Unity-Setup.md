# Unity Setup

## Required Unity Version

- Unity `6000.3.14f1`

## Scene Flow

Recommended Build Settings order:

1. `Assets/Scenes/Main.unity`
2. `Assets/Game.unity`

`Main.unity` is the front-end scene. `Game.unity` is the gameplay scene.

## Player Setup

The `Player` GameObject should have:

- `Rigidbody2D`
- `Collider2D`
- `PlayerInput`
- `PlayerMovement`
- `PlayerShooting`
- `PlayerHealth`
- `StatusReceiver`
- `PlayerStats`
- `PlayerExperience`
- `PlayerPickupCollector`

## Main Menu

The menu is currently created at runtime by `MainMenuRuntime`.

It provides:

- mode selection
- single-player setup
- loadout setup

No manually-authored menu canvas is required for the current version.

## Loadout

Current loadout categories:

- weapon
- bomb
- active skill
- passive

The selected values are stored in `RunLoadoutState` and applied by `RunLoadoutApplier` when gameplay loads.

## Level-Up Popup

Create this hierarchy under the gameplay Canvas:

```text
LevelUpPanel
  LevelUpTitle
  UpgradeButton_1
    UpgradeButtonText_1
  UpgradeButton_2
    UpgradeButtonText_2
  UpgradeButton_3
    UpgradeButtonText_3
```

Create an active scene object:

```text
LevelUpManager
```

Assign:

```text
Panel: LevelUpPanel
Title Text: LevelUpTitle
Choice Buttons:
  Element 0: UpgradeButton_1
  Element 1: UpgradeButton_2
  Element 2: UpgradeButton_3
Choice Texts:
  Element 0: UpgradeButtonText_1
  Element 1: UpgradeButtonText_2
  Element 2: UpgradeButtonText_3
```

## HUD

Gameplay UI can include:

- score text
- HP text and optional HP slider
- level and XP text
- XP slider
- run timer text
- bomb cooldown widget
- secondary-skill cooldown widget

## Run Announcement UI

Create a TMP text object named `RunAnnouncementText`, add `RunAnnouncementUI`, and place it near the upper-middle of the gameplay HUD.

This is used for:

- elite spawn / defeat
- boss warnings
- run-start loadout announcements

## Enemy Rewards

On each enemy prefab's `EnemyHealth`, configure:

- score reward
- XP reward
- pickup drop chances
- optional pickup / gem prefabs

If these prefabs are empty, the game creates runtime placeholder versions for testing.

## Wave, Elite, and Boss Directors

Gameplay scene should contain:

- `EnemyRespawnManager`
- `EnemyWaveDirector`
- `EliteSpawnDirector`
- `BossSpawnDirector`

Optional:

- `BossSpawnPoint` objects for authored boss entrances

## Audio

`GameAudio` loads runtime clips from:

```text
Assets/Resources/Audio/SFX/
```

Useful folders include:

```text
BombThrow
BombImpact
SkillMagneticPulse
SkillArcaneShield
SkillFrostNova
EliteSpawn
EliteDefeated
```

## EventSystem

If UI buttons do not click, confirm there is an active `EventSystem`.

With the new Input System, use:

```text
Input System UI Input Module
```
