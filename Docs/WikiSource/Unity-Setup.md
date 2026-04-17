# Unity Setup

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

## Score UI

1. Create a scene object named `ScoreManager`.
2. Add the `ScoreManager` script.
3. Under the Canvas, create a TMP text object named `ScoreText`.
4. Add `ScoreTextUI` to `ScoreText`.
5. Keep the prefix as `Score: ` or customize it.

## Run Timer UI

1. Create an empty scene object named `RunTimer`.
2. Add the `RunTimer` script.
3. Leave `Start On Awake` enabled.
4. Under the Canvas, create a TMP text object named `RunTimerText`.
5. Add `RunTimerUI` to `RunTimerText`.
6. Leave `Run Timer` empty to use `RunTimer.Instance`, or assign the scene timer object.

Recommended HUD order:

```text
Score: 0
Time: 00:00
HP: 100 / 100
```

## XP and Leveling

On the Player:

1. Add `PlayerExperience`.
2. Add `PlayerPickupCollector`.
3. Confirm `PlayerStats` exists.

For faster testing:

```text
PlayerExperience > Base Experience To Next Level = 3
```

## Level-Up Popup

Create this hierarchy under the Canvas:

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

Add the `LevelUpManager` script and assign:

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

## Experience UI

`ExperienceUI` is optional.

Assign:

- `Player Experience`
- `Level Text`
- `Experience Text`
- optional `Experience Slider`

If `Player Experience` is empty, the script tries to find one in the scene.

## Enemy Rewards

On each enemy prefab's `EnemyHealth`:

- set `Points On Death`
- set `Experience On Death`
- set `Experience Drop Chance`
- optionally assign `Experience Gem Prefab`
- set `Health Drop Chance`
- set `Health Pickup Amount`
- optionally assign `Health Pickup Prefab`
- set `Magnet Drop Chance`
- optionally assign `Magnet Pickup Prefab`
- set `Bomb Drop Chance`
- set `Bomb Damage`
- set `Bomb Radius`
- optionally assign `Bomb Pickup Prefab`

If no gem prefab is assigned, a simple runtime green gem is created.

If no pickup prefabs are assigned, simple runtime pickups are created.

## Wave Director

1. Create an empty scene object named `EnemyWaveDirector`.
2. Add the `EnemyWaveDirector` script.
3. Assign:

```text
Run Timer: RunTimer
Respawn Manager: EnemySystems or whichever object has EnemyRespawnManager
```

4. Configure stages.

Example:

```text
0 seconds:    maxAlive 8,  respawnDelay 4
60 seconds:   maxAlive 10, respawnDelay 3.5
120 seconds:  maxAlive 12, respawnDelay 3
180 seconds:  maxAlive 15, respawnDelay 2.5
```

If a stage's enemy prefab array is empty, the current enemy pool remains active.

## Elite Spawn Director

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

## Run Announcement UI

1. Under the Canvas, create a TMP text object named `RunAnnouncementText`.
2. Add `RunAnnouncementUI`.
3. Anchor it near the upper-middle of the screen.

Recommended Rect Transform:

```text
Anchor: Top Center
Pivot X: 0.5
Pivot Y: 1
Pos X: 0
Pos Y: -80
Width: 600
Height: 80
```

Recommended TMP settings:

```text
Font Size: 42
Alignment: Center / Middle
Color: gold or white
Text Input: empty
```

## Health UI

To use `PlayerHealthUI`:

1. Create TMP text for HP.
2. Optionally create a UI Slider for HP.
3. Add `PlayerHealthUI` to a UI object.
4. Assign `Player Health`, `Health Text`, and optional `Health Slider`.

If `Player Health` is empty, the script tries to find one in the scene.

## EventSystem

If UI buttons do not click, confirm there is an active `EventSystem`.

For the new Input System, use:

```text
Input System UI Input Module
```
