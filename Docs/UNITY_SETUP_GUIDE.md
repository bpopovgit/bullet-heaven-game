# Unity Setup Guide

This guide covers the scene and prefab wiring needed for the current systems.

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

`PlayerExperience` requires `PlayerStats`; Unity should add it automatically if missing.

`PlayerPickupCollector` requires `PlayerExperience`.

## Shooting Setup

On `PlayerShooting`, assign:

- `Muzzle`: the fire point child transform.
- `Main Cam`: optional; it auto-fills from `Camera.main` if empty.
- `Weapon`: a `WeaponDefinition` asset.
- `Multi Projectile Spread Angle`: default `12`.

The weapon's `bulletPrefab` should have:

- `Rigidbody2D`
- `Collider2D`
- `BulletElemental`

## Score UI Setup

1. Create a `ScoreManager` GameObject in the scene.
2. Add `ScoreManager`.
3. Under the Canvas, create a TMP text object named `ScoreText`.
4. Add `ScoreTextUI` to the same TMP text object.
5. Leave the prefix as `Score: ` or customize it.

## Run Timer Setup

1. Create an empty scene object named `RunTimer`.
2. Add the `RunTimer` script.
3. Leave `Start On Awake` enabled.
4. Under the Canvas, create a TMP text object named `RunTimerText`.
5. Add `RunTimerUI` to `RunTimerText`.
6. Assign `Run Timer` if desired, or leave it empty to use `RunTimer.Instance`.
7. Leave the prefix as `Time: ` or customize it.

Recommended top-left HUD placement:

```text
Score: 0
Time: 00:00
HP: 100 / 100
```

Because `RunTimer` uses scaled time, it pauses automatically during level-up popups.

## Enemy Reward Setup

On enemy prefabs, configure `EnemyHealth`:

- `Max Health`
- `Points On Death`
- `Experience On Death`
- `Experience Drop Chance`
- Optional `Experience Gem Prefab`
- `Health Drop Chance`
- `Health Pickup Amount`
- Optional `Health Pickup Prefab`
- `Magnet Drop Chance`
- Optional `Magnet Pickup Prefab`
- `Bomb Drop Chance`
- `Bomb Damage`
- `Bomb Radius`
- Optional `Bomb Pickup Prefab`

If no XP gem prefab is assigned, the game creates a simple green runtime gem automatically.

If pickup prefabs are empty, the game creates simple runtime pickups automatically:

- red health pickup
- cyan magnet pickup
- yellow/orange bomb pickup

## XP and Leveling Setup

On the `Player`, add:

- `PlayerStats`
- `PlayerExperience`
- `PlayerPickupCollector`

Useful test settings:

```text
PlayerExperience > Base Experience To Next Level = 3
EnemyHealth > Experience On Death = 1 or higher
```

Set the threshold back higher once the loop is verified.

## Level-Up Choice UI Setup

1. Under the Canvas, create a panel named `LevelUpPanel`.
2. Add a TMP title text named `LevelUpTitle`.
3. Add three buttons:

```text
UpgradeButton_1
UpgradeButton_2
UpgradeButton_3
```

4. Each button should have a TMP child text:

```text
UpgradeButtonText_1
UpgradeButtonText_2
UpgradeButtonText_3
```

5. Create an active scene object named `LevelUpManager`.
6. Add the `LevelUpManager` script.
7. Assign:

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

8. Disable only `LevelUpPanel` by default.
9. Keep `LevelUpManager` active.

Expected behavior:

- Level-up pauses the game.
- The panel appears.
- Clicking a choice applies the upgrade.
- The panel closes.
- The game resumes.

## EventSystem Setup

If upgrade buttons do not respond to clicks, check the scene has an active `EventSystem`.

When using the new Input System, prefer:

```text
Input System UI Input Module
```

instead of the old standalone input module.

## Experience UI Setup

`ExperienceUI` is optional.

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

If `Player Health` is empty, the script tries to find one in the scene.

## Enemy Respawn Setup

The scene should contain an object with `EnemyRespawnManager`.

Assign:

- enemy prefabs
- spawn points, or leave empty to auto-find `EnemySpawnPoint` components
- max alive
- respawn delay
- min distance from player
- min distance between enemies

Spawn point objects should have:

- `EnemySpawnPoint`
- a clear name such as `Spawn_01`

## Wave Director Setup

`EnemyWaveDirector` is optional but recommended once `RunTimer` exists.

1. Create an empty scene object named `EnemyWaveDirector`.
2. Add the `EnemyWaveDirector` script.
3. Assign:

```text
Run Timer: RunTimer
Respawn Manager: EnemySystems or whichever object has EnemyRespawnManager
```

Both fields can be left empty because the script auto-finds them, but assigning them is clearer.

4. Configure `Stages`.

Suggested starting stages:

```text
Element 0:
  Start Time Seconds: 0
  Max Alive: 8
  Respawn Delay: 4
  Fill Immediately: true

Element 1:
  Start Time Seconds: 60
  Max Alive: 10
  Respawn Delay: 3.5
  Fill Immediately: true

Element 2:
  Start Time Seconds: 120
  Max Alive: 12
  Respawn Delay: 3
  Fill Immediately: true

Element 3:
  Start Time Seconds: 180
  Max Alive: 15
  Respawn Delay: 2.5
  Fill Immediately: true
```

To introduce a new enemy type later, add that prefab to the stage's `Enemy Prefabs` array. If the array is empty, the existing respawn-manager prefab pool is kept.

## Elite Spawn Director Setup

1. Create an empty scene object named `EliteSpawnDirector`.
2. Add the `EliteSpawnDirector` script.
3. Assign:

```text
Run Timer: RunTimer
Respawn Manager: EnemySystems or whichever object has EnemyRespawnManager
```

Both fields can be left empty because the script auto-finds them, but assigning them is clearer.

Recommended starting values:

```text
Spawn Elites: true
First Elite Time Seconds: 90
Elite Interval Seconds: 90
Max Elites Alive: 1
Health Multiplier: 4
Reward Multiplier: 5
Scale Multiplier: 1.4
Pickup Drop Chance Bonus: 0.25
Tint Color: gold/yellow
```

For quick testing:

```text
First Elite Time Seconds: 10
Elite Interval Seconds: 20
```

If `Elite Prefabs` is empty, the director chooses from the respawn manager's current enemy pool.

If you want only specific enemies to become elites, add those prefabs to `Elite Prefabs`.

## Run Announcement UI Setup

`RunAnnouncementUI` is optional but recommended for elite and boss warnings.

1. Under the Canvas, create a TMP text object named `RunAnnouncementText`.
2. Add `RunAnnouncementUI` to it.
3. Anchor it near the upper-middle of the screen.
4. Recommended Rect Transform:

```text
Anchor: Top Center
Pivot X: 0.5
Pivot Y: 1
Pos X: 0
Pos Y: -80
Width: 600
Height: 80
```

5. Recommended TextMeshPro settings:

```text
Font Size: 42
Alignment: Center / Middle
Color: gold or white
Text Input: empty
```

`EliteSpawnDirector` will use this automatically through `RunAnnouncementUI.Instance`.
