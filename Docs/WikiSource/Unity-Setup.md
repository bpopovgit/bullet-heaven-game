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

If no gem prefab is assigned, a simple runtime green gem is created.

## EventSystem

If UI buttons do not click, confirm there is an active `EventSystem`.

For the new Input System, use:

```text
Input System UI Input Module
```

