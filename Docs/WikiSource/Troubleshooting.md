# Troubleshooting

## Score Does Not Change

Check:

- The scene has an active `ScoreManager`.
- Enemies die through `EnemyHealth`.
- Enemy `Points On Death` is greater than zero.
- `ScoreTextUI` is attached to the TMP score text object.

## XP Gems Drop But Do Not Move Toward Player

Check:

- Player has `PlayerPickupCollector`.
- Player has `PlayerExperience`.
- Player has `PlayerStats`.
- The pickup radius is large enough.
- The gem has `XPGem` and a trigger collider.

## Loadout Selections Do Not Apply In Game

Check:

- You started from `Main.unity`, not directly from `Game.unity`, if you expected menu-selected values.
- `RunLoadoutApplier` exists in the project and the gameplay scene fully loaded.
- The Console shows `LOADOUT APPLIED`.

## Q or E Ability Has No Sound

Check:

- The correct SFX folder exists under `Assets/Resources/Audio/SFX/`.
- The clip imported as a usable audio asset.
- The folder name matches the expected runtime path exactly.

## Level-Up Auto-Picks Instead of Showing Popup

Check:

- Scene has an active `LevelUpManager`.
- `Panel` is assigned.
- `Choice Buttons` has size `3`.
- Buttons are assigned.
- `LevelUpManager` is active even though `LevelUpPanel` starts disabled.

## Buttons Do Not Click

Check:

- An active `EventSystem` exists.
- With the new Input System, use `Input System UI Input Module`.
- Another UI object is not blocking the buttons.

## Boss Does Not Spawn

Check:

- `BossSpawnDirector` exists in the gameplay scene.
- `RunTimer` is active.
- `EnemyRespawnManager` is active.
- The session log does not show a missing prefab or spawn failure.

## Build Settings Scene Is Wrong

Check:

```text
File > Build Settings
```

Recommended order:

1. `Assets/Scenes/Main.unity`
2. `Assets/Game.unity`
