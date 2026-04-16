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

## Level-Up Auto-Picks Instead of Showing Popup

Check:

- Scene has an active `LevelUpManager`.
- `LevelUpManager` has the script attached.
- `Panel` is assigned.
- `Choice Buttons` has size `3`.
- Buttons are assigned.
- `LevelUpManager` is active even though `LevelUpPanel` starts disabled.

## Game Freezes After Choosing Upgrade

Check the Console for red errors.

The expected flow is:

1. Button click.
2. Upgrade applies.
3. Panel hides.
4. `Time.timeScale` returns to `1`.

If the panel remains visible, check button assignments and `LevelUpManager`.

## Buttons Do Not Click

Check:

- An active `EventSystem` exists.
- With the new Input System, use `Input System UI Input Module`.
- The level-up panel is not blocking its own buttons with another UI object.

## Player Does Not Shoot

Check:

- `PlayerShooting.weapon` is assigned.
- The weapon has a `bulletPrefab`.
- The bullet prefab has `BulletElemental`.
- `muzzle` is assigned.
- `PlayerInput` has a `Fire` action.

## Enemy Projectiles Damage Player Too Often

Check:

- `PlayerHealth.iFrameTime`.
- Enemy projectile damage values.
- Projectile spawn rate on `RangedShooter`.
- Whether multiple projectile prefabs overlap.

## Build Settings Scene Is Wrong

Check:

```text
File > Build Settings
```

Make sure the intended gameplay scene is included.

