# Unity Setup Guide

This guide covers the current scene, HUD, front-end, audio, and prefab wiring for the live project state.

## Scene Flow

The project currently expects this scene flow:

```text
Assets/Scenes/Main.unity
Assets/Game.unity
```

Recommended Build Settings order:

1. `Assets/Scenes/Main.unity`
2. `Assets/Game.unity`

`Main.unity` is the front-end entry scene. `Game.unity` is the gameplay scene.

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

Runtime scripts such as `RunLoadoutApplier`, `PlayerActiveBomb`, and `PlayerSecondaryActiveSkill` will attach or configure themselves as needed during gameplay.

## Main Menu Setup

The current front-end is script-built by `MainMenuRuntime`.

For `Main.unity`:

- keep the scene itself
- keep an `EventSystem` if you want, or let the runtime menu create one
- no hand-authored menu canvas is required for the current flow

`MainMenuRuntime` builds:

- mode selection
- single-player setup
- multiplayer placeholder
- loadout setup

## Shooting Setup

On `PlayerShooting`, assign:

- `Muzzle`: the fire point child transform
- `Main Cam`: optional, auto-fills from `Camera.main` if empty
- `Weapon`: a `WeaponDefinition` asset used as the default/fallback
- `Multi Projectile Spread Angle`

The weapon's `bulletPrefab` should have:

- `Rigidbody2D`
- `Collider2D`
- `BulletElemental`

## Starting Loadout Flow

The selected loadout is stored in `RunLoadoutState` and applied in gameplay by `RunLoadoutApplier`.

Current categories:

- weapon
- bomb (`Q`)
- active skill (`E`)
- passive

No inspector wiring is required for the menu-to-game handoff beyond keeping the main and gameplay scenes available.

## Bomb Setup

`PlayerActiveBomb` is added/configured during gameplay.

Bomb behavior assumptions:

- uses mouse-targeted throwing
- uses a visible bomb projectile
- uses bomb-specific visuals and SFX
- uses `BombCooldownUI` for the `Q` HUD element

If you add new bomb families later, update:

- `RunLoadoutState`
- `PlayerActiveBomb`
- `BombAbilityDefinition`
- bomb SFX folders in `Resources`

## Secondary Skill Setup

`PlayerSecondaryActiveSkill` is configured during gameplay based on the chosen loadout.

Current `E` skills:

- `Magnetic Pulse`
- `Arcane Shield`
- `Frost Nova`

`SecondarySkillCooldownUI` displays the selected skill icon and cooldown beside the bomb UI.

## Score UI Setup

1. Create a `ScoreManager` GameObject in the gameplay scene.
2. Add `ScoreManager`.
3. Under the gameplay `Canvas`, create a TMP text object named `ScoreText`.
4. Add `ScoreTextUI` to `ScoreText`.

## Health UI Setup

`PlayerHealthUI` is optional but recommended.

To use it:

1. Create TMP text for HP.
2. Optionally create a UI Slider for the HP bar.
3. Add `PlayerHealthUI` to a UI object.
4. Assign:

```text
Player Health
Health Text
Health Slider
```

If `Player Health` is left empty, the script tries to find the player health component in the scene.

## Experience UI Setup

`ExperienceUI` is optional but recommended.

To use it:

1. Create TMP text for level.
2. Create TMP text for XP.
3. Optionally create a UI Slider for the XP bar.
4. Add `ExperienceUI` to a UI object.
5. Assign:

```text
Player Experience
Level Text
Experience Text
Experience Slider
```

If `Player Experience` is left empty, the script tries to find one in the scene.

## Run Timer Setup

1. Create an empty scene object named `RunTimer`.
2. Add the `RunTimer` script.
3. Leave `Start On Awake` enabled unless you have a custom start flow.
4. Under the gameplay `Canvas`, create a TMP text object named `RunTimerText`.
5. Add `RunTimerUI` to `RunTimerText`.

Because `RunTimer` uses scaled time, it automatically pauses during:

- level-up popups
- boss reward selection

## Run Announcement UI Setup

`RunAnnouncementUI` is optional but strongly recommended.

1. Under the gameplay `Canvas`, create a TMP text object named `RunAnnouncementText`.
2. Add `RunAnnouncementUI`.
3. Recommended Rect Transform:

```text
Anchor: Top Center
Pivot X: 0.5
Pivot Y: 1
Pos X: 0
Pos Y: -80
Width: 600
Height: 80
```

4. Recommended TMP settings:

```text
Font Size: 42
Alignment: Center / Middle
Color: gold or white
Raycast Target: Off
Text Input: empty
```

This is used by:

- elite spawn/defeat announcements
- boss spawn / phase / defeat messaging
- run-start loadout announcements

## Level-Up Choice UI Setup

Under the gameplay `Canvas`, create:

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

Create an active scene object named `LevelUpManager`, add the script, and assign:

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

Disable only `LevelUpPanel` by default. Keep `LevelUpManager` active.

## Bomb Cooldown UI Setup

`BombCooldownUI` can auto-bootstrap, but it is easiest to let the runtime create it if missing.

Expected behavior:

- bottom-left position
- bomb icon
- `Q` label
- radial cooldown sweep
- numeric countdown while cooling down

## Secondary Skill Cooldown UI Setup

`SecondarySkillCooldownUI` behaves like the bomb widget and sits beside it.

Expected behavior:

- bottom-left position beside the bomb UI
- icon based on the selected `E` skill
- `E` label
- radial cooldown sweep
- numeric countdown

## Enemy Reward Setup

On enemy prefabs, configure `EnemyHealth`:

- `Max Health`
- `Points On Death`
- `Experience On Death`
- `Experience Drop Chance`
- optional `Experience Gem Prefab`
- `Health Drop Chance`
- `Health Pickup Amount`
- optional `Health Pickup Prefab`
- `Magnet Drop Chance`
- optional `Magnet Pickup Prefab`
- `Bomb Drop Chance`
- `Bomb Damage`
- `Bomb Radius`
- optional `Bomb Pickup Prefab`

If no XP gem prefab is assigned, the game creates a simple green runtime gem automatically.

If pickup prefabs are empty, the game creates simple runtime pickups automatically.

## Enemy Respawn Setup

The gameplay scene should contain an object with `EnemyRespawnManager`.

Assign:

- enemy prefabs
- spawn points, or leave empty to auto-find `EnemySpawnPoint` components
- max alive
- respawn delay
- min distance from player
- min distance between enemies

Spawn point objects should have:

- `EnemySpawnPoint`
- clear names such as `Spawn_01`

## Wave Director Setup

`EnemyWaveDirector` is recommended once `RunTimer` exists.

1. Create an empty scene object named `EnemyWaveDirector`.
2. Add the `EnemyWaveDirector` script.
3. Assign:

```text
Run Timer: RunTimer
Respawn Manager: EnemySystems or whichever object has EnemyRespawnManager
```

Each stage can now also restrict `Allowed Spawn Regions`, which makes map-based wave shaping much easier.

## Elite Spawn Director Setup

1. Create an empty scene object named `EliteSpawnDirector`.
2. Add the `EliteSpawnDirector` script.
3. Assign:

```text
Run Timer: RunTimer
Respawn Manager: EnemySystems or whichever object has EnemyRespawnManager
```

Recommended values:

```text
Spawn Elites: true
First Elite Time Seconds: 90
Elite Interval Seconds: 90
Max Elites Alive: 1
Health Multiplier: 4
Reward Multiplier: 5
Scale Multiplier: 1.4
Pickup Drop Chance Bonus: 0.25
```

For quick testing:

```text
First Elite Time Seconds: 10
Elite Interval Seconds: 20
```

## Boss Setup

1. Create an empty scene object named `BossSpawnDirector`.
2. Add the `BossSpawnDirector` script.
3. Assign or leave auto-find references for:

```text
Run Timer
Respawn Manager
```

Optional:

- add `BossSpawnPoint` objects to author specific boss entrances

Default fallback behavior:

- if no boss spawn point exists, the dragon spawns north / above the player

## Audio Setup

`GameAudio` loads clips from `Assets/Resources/Audio/SFX`.

Recommended folders include:

```text
PlayerShoot
EnemyShoot
EnemyDeath
PlayerHit
PlayerDeath
XPGem
LevelUp
HealthPickup
MagnetPickup
BombPickup
BombThrow
BombImpact
SkillMagneticPulse
SkillArcaneShield
SkillFrostNova
EliteSpawn
EliteDefeated
UISelect
```

If you place multiple clips in one folder, the game randomly chooses one at runtime.

## EventSystem Setup

If UI buttons do not respond to clicks, check the scene has an active `EventSystem`.

With the new Input System, prefer:

```text
Input System UI Input Module
```

## Session Logging

`PlaySessionLogWriter` writes one `.txt` log file per play session.

Use it when debugging issues like:

- boss spawn timing
- missing loadout application
- bomb / secondary skill behavior
- menu-to-game handoff issues
