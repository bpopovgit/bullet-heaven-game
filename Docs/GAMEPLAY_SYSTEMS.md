# Gameplay Systems

## Damage Model

Damage flows through `DamagePacket`, defined in `DamageType.cs`.

`DamagePacket` includes:

- `amount`
- `element`
- `splashRadius`
- `sourcePos`
- `status`
- `statusDuration`
- `statusStrength`

Elements:

- `Physical`
- `Fire`
- `Lightning`
- `Frost`
- `Poison`

Statuses:

- `None`
- `Burn`
- `Shock`
- `Slow`
- `Freeze`
- `Poison`

`DamagePacket.Clamp()` should be called before applying damage when a packet is built from configurable data.

## Faction Battlefield Foundation

The game now has a first-pass faction identity layer for the roguelike battlefield direction.

Current factions are defined by `FactionType`:

- `Neutral`
- `Human`
- `Angel`
- `Demon`
- `Zombie`

Actors use `FactionMember` to declare:

- which faction they belong to
- whether they can currently be targeted

Default behavior:

- the player is automatically treated as `Human`
- existing enemies are automatically treated as `Zombie`
- authored prefabs can override this by adding `FactionMember` directly

Target selection is handled by `FactionTargeting`.

Current priority rules:

- Demons prefer Angels, then Humans, then Zombies.
- Angels prefer Demons, then Humans, then Zombies.
- Humans prefer Zombies, then Angels/Demons.
- Zombies attack any other faction, choosing by distance when priority is equal.

Damage routing is handled by `FactionCombat`.

This means hostile checks are now centralized before damage is applied to `PlayerHealth` or `EnemyHealth`. Player bullets, enemy projectiles, and melee contact damage all respect faction hostility.

This is the foundation for:

- allied minions around the player
- angel vs demon fights happening without the player
- zombies attacking everyone
- future playable factions and map-specific faction setups

## Primary Weapons

`PlayerShooting` reads:

- the active `WeaponDefinition`
- mouse position from the Input System
- current `PlayerStats`

It spawns the weapon's bullet prefab from the configured muzzle/fire point.

Current starting weapon identities:

- `Ember Repeater`: fast fire, burn-focused
- `Frost Lance`: slower frost rounds with crowd-control lean
- `Venom Caster`: poison splash pressure
- `Storm Needler`: faster, sharper lightning burst

Current upgrade-aware primary weapon stats:

- fire rate
- projectile count
- damage multiplier
- pierce
- splash radius

## Pre-Run Loadout

The run now starts with a selected loadout chosen in the menu scene.

Current loadout categories:

- starting weapon
- bomb on `Q`
- active skill on `E`
- passive perk

`RunLoadoutState` stores the selected choices, and `RunLoadoutApplier` applies them when gameplay loads.

The current front-end flow is:

```text
Main Menu -> Single Player Setup -> Loadout -> Start Run
```

## Bomb Skill on `Q`

`PlayerActiveBomb` handles the player's first active skill slot.

Current bomb behavior:

- press `Q`
- target the mouse position
- throw a visible bomb projectile
- clamp throw distance to a max range
- detonate on arrival
- apply bomb-specific damage/status behavior

Current bomb options:

- `Frag Bomb`
- `Frost Bomb`
- `Fire Bomb`
- `Shock Bomb`

Bombs have:

- per-bomb visuals
- per-bomb cooldown
- bomb-specific SFX folders
- a bottom-left cooldown widget via `BombCooldownUI`

## Secondary Active Skill on `E`

`PlayerSecondaryActiveSkill` handles the second active slot.

The selected skill is chosen in the loadout screen and shown in its own cooldown widget through `SecondarySkillCooldownUI`.

Current `E` skills:

- `Magnetic Pulse`
  - pushes nearby enemies away
  - attracts nearby pickups
- `Arcane Shield`
  - grants short invulnerability
  - clears nearby enemy projectiles
- `Frost Nova`
  - freezes nearby enemies in place
  - applies strong frost visuals rather than direct kill damage

Each `E` skill now has:

- distinct icon colors
- separate cooldown values
- separate sound effects

## Enemy Health, Score, XP, and Status

`EnemyHealth` controls:

- max health
- death state
- score reward
- XP reward
- optional XP gem prefab
- optional pickup drops
- death event for respawn tracking

On death:

1. Adds score through `ScoreManager`.
2. Drops XP through `XPGem`.
3. Rolls optional health, magnet, and bomb pickup drops.
4. Raises `Died`.
5. Plays enemy death SFX.
6. Destroys the enemy GameObject.

`EnemyHealth` also forwards status-bearing packets into `StatusReceiver`, so enemies now properly react to:

- slow
- freeze
- burn
- poison
- shock

## Status Effects

`StatusReceiver` is the shared status system used by the player and now by enemies too.

Current supported behaviors:

- `Slow`: reduces movement speed
- `Shock`: stun-like lock state
- `Freeze`: fully locks movement and uses frost visuals
- `Burn`: damage over time
- `Poison`: damage over time

Freeze now adds:

- icy blue tint
- frost ring
- icy sparkles

That makes `Frost Nova` read as hard crowd control instead of just blue damage.

## Survival Pickups

Pickups share attraction behavior through `PlayerPickup`.

Current pickup types:

- `HealthPickup`: heals the player
- `MagnetPickup`: attracts every active XP gem to the player
- `BombPickup`: damages enemies in a radius around the player

`EnemyHealth` controls pickup drop chances:

- `healthDropChance`
- `magnetDropChance`
- `bombDropChance`

If a pickup prefab is not assigned, the game spawns a simple colored runtime pickup:

- red: health
- cyan: magnet
- yellow/orange: bomb

## Score System

`ScoreManager` is a scene-level singleton.

Core API:

```csharp
ScoreManager.Instance.AddScore(amount);
ScoreManager.Instance.ResetScore();
```

`ScoreTextUI` subscribes to `ScoreManager.ScoreChanged` and updates TMP text.

## Health, XP, and Timer HUD

Current HUD support:

- `PlayerHealthUI`: HP text and HP slider
- `ExperienceUI`: level text, XP text, XP slider
- `RunTimerUI`: `Time: 00:00`
- `BombCooldownUI`: `Q` cooldown icon
- `SecondarySkillCooldownUI`: `E` cooldown icon

The timer uses scaled time, so it pauses automatically during level-up choices and reward popups.

## XP and Leveling

`PlayerExperience` tracks:

- current level
- current XP
- XP needed for the next level
- pending level-ups
- upgrade choices

Default first threshold:

```text
10 XP
```

Default growth:

```text
1.25x per level
```

When enough XP is collected, the player levels up. If `LevelUpManager` is configured, the game pauses and shows upgrade choices. If not, it auto-picks the first generated upgrade so leveling still works.

## Upgrade System

Upgrade options are represented by `PlayerUpgradeOption`.

Current default pool includes:

- `Sharpened Rounds`
- `Trigger Rhythm`
- `Fleet Footing`
- `Magnetic Field`
- `Split Shot`
- `Punch Through`
- `Vital Core`
- `Volatile Payload`

Upgrades currently modify either:

- `PlayerStats`
- `PlayerHealth`

## Level-Up UI

`LevelUpManager` is optional but recommended.

When configured, it:

1. Stores the current `Time.timeScale`.
2. Sets `Time.timeScale = 0`.
3. Shows the level-up panel.
4. Fills choice buttons with upgrade titles and descriptions.
5. Applies the chosen upgrade.
6. Hides the panel.
7. Restores time scale.

## Enemy Spawning and Wave Regions

`EnemyRespawnManager` maintains a maximum number of living enemies and spawns from authored `EnemySpawnPoint` objects.

Current behavior:

- uses configured `enemyPrefabs`
- uses configured or auto-found `EnemySpawnPoint` objects
- avoids invalid spawn positions
- supports stage-based prefab pools
- supports region-aware filtering

`EnemySpawnPoint` now supports directional grouping such as:

- `North`
- `East`
- `South`
- `West`
- `Center`

`EnemyWaveDirector` can restrict each wave stage to specific spawn regions, which makes runs feel more authored than simple global spawning.

## Elite Enemies

`EliteSpawnDirector` uses the run timer and respawn manager to spawn occasional elite enemies.

Elites reuse existing enemy prefabs and are modified at runtime by `EliteEnemy`.

Elite modifiers include:

- health multiplier
- score/XP reward multiplier
- visual scale multiplier
- sprite tint
- pickup drop chance bonus

Elite spawn and elite defeat can display temporary messages through `RunAnnouncementUI` and play matching audio.

## Dragon Boss

The current first boss is a dragon-themed ranged boss.

The boss flow supports:

- timed boss spawn
- boss-only spawn logic from above / north of the player
- optional authored `BossSpawnPoint` anchors
- boss phase two below 50% HP
- floating world-space HP bar
- boss reward popup on death

Phase two currently changes:

- tint
- attack aggression
- pressure feel

Boss reward choices are separate from normal level-up upgrades and are intended to feel stronger and more run-defining.

## Main Menu, Setup, and Loadout UI

`MainMenuRuntime` generates the front-end UI at runtime inside `Main.unity`.

Current screens:

- mode select
- single-player setup
- multiplayer placeholder
- loadout setup

That means the front-end is script-driven right now rather than authored from a manually-built persistent menu prefab.

## Audio System

`GameAudio` loads SFX from `Resources` folders and randomly picks one clip when multiple files exist in a folder.

Examples:

```text
Assets/Resources/Audio/SFX/PlayerShoot
Assets/Resources/Audio/SFX/BombThrow
Assets/Resources/Audio/SFX/BombImpact
Assets/Resources/Audio/SFX/SkillMagneticPulse
Assets/Resources/Audio/SFX/SkillArcaneShield
Assets/Resources/Audio/SFX/SkillFrostNova
```

This allows quick variation without extra code changes.

## Session Logging

`PlaySessionLogWriter` writes one text log per play session. This is useful for debugging issues such as:

- boss spawn timing
- loadout application flow
- activation/cooldown problems
- menu/runtime handoff problems

The logger writes to Unity's `persistentDataPath` under a `SessionLogs` folder.
