# System Handoff

This document is a practical map of the current Unity 6 project. It is meant to help a second developer or designer quickly understand:

- which scene does what
- which script owns which system
- which systems are runtime-bootstrapped versus scene-authored
- where to look when changing loadouts, combat, bosses, pickups, UI, or progression

## Fast Answer: What Manages The Loadout Weapons?

The loadout-selected starting weapons are mainly controlled by these scripts:

- `Assets/Scripts/GameSystems/RunLoadoutState.cs`
  - stores the selected starting weapon, bomb, active skill, and passive
  - also stores the display names and descriptions used by the loadout UI
- `Assets/Scripts/UIScripts/MainMenuRuntime.cs`
  - builds the runtime loadout menu in `Main.unity`
  - lets the player cycle the starting weapon choice before the run
- `Assets/Scripts/GameSystems/RunLoadoutApplier.cs`
  - applies the chosen loadout when `Game.unity` loads
  - creates a runtime clone of the base `WeaponDefinition` and mutates it into the selected weapon preset
- `Assets/Scripts/PlayerScripts/PlayerShooting.cs`
  - actually fires the current weapon
  - reads the assigned `WeaponDefinition` and player stat modifiers
- `Assets/Scripts/Combat/WeaponDefinition.cs`
  - data container for the weapon's damage, element, fire rate, projectile prefab, status effect, splash, and pierce
- `Assets/Scripts/Combat/BulletElemental.cs`
  - uses the selected weapon definition at runtime when bullets are spawned
  - applies element color, damage, splash, pierce, and status effects

If someone wants to change how the starting weapons feel, the first place to inspect is:

- `Assets/Scripts/GameSystems/RunLoadoutApplier.cs`

That is where the selected loadout weapon becomes:

- `Ember Repeater`
- `Frost Lance`
- `Venom Caster`
- `Storm Needler`

## Scene Map

### `Assets/Scenes/Main.unity`

Purpose:

- front-end entry scene
- main menu scene
- single-player setup scene
- loadout selection scene

Authored root objects currently in the scene:

- `Main Camera`

Important note:

- almost all visible UI in this scene is built at runtime by `MainMenuRuntime`
- this scene is intentionally lightweight

Runtime systems expected here:

- `MainMenuRuntime`
- `EventSystem` with `InputSystemUIInputModule`
- a generated `Canvas` if no authored canvas is already present

### `Assets/Game.unity`

Purpose:

- active gameplay scene
- survival arena
- all combat, spawning, pickups, progression, and boss logic happen here

Current scene root objects:

- `Main Camera`
- `Player`
- `Virtual Camera`
- `Canvas`
- `EventSystem`
- `LevelGrid`
- `Spawn_01`
- `Spawn_02`
- `Spawn_03`
- `Spawn_04`
- `EnemySystems`
- `ScoreManager`
- `LevelUpManager`
- `RunTimer`
- `EnemyWaveDirector`
- `EliteSpawnDirector`

Important authored child objects already present:

- under `Player`
  - `FirePoint`
  - `VFX_Burn`
  - `VFX_Frost`
  - `VFX_Poison`
  - `VFX_Shock`
- under `Canvas`
  - `ScoreText`
  - `HealthText`
  - `HealthSlider`
  - `LevelText`
  - `ExperienceText`
  - `ExperienceSlider`
  - `RunTimerText`
  - `RunAnnouncementText`
  - `LevelUpPanel`
  - `LevelUpTitle`
  - `UpgradeButton_1`
  - `UpgradeButton_2`
  - `UpgradeButton_3`
  - `GameOverPanel`
  - `RestartButton`

Runtime systems expected here:

- `RunLoadoutApplier`
- `BossSpawnDirector`
- `GameAudio`
- `PlaySessionLogWriter`
- `BombCooldownUI`
- `SecondarySkillCooldownUI`

### `Assets/Scenes/SampleScene.unity`

Purpose:

- legacy / spare scene
- not part of the current main menu -> gameplay flow

Current root objects:

- `Main Camera`

Recommendation:

- treat this as a non-active scene unless the team intentionally reuses it later

## Runtime-Bootstrapped Systems

These are not required to exist as scene-authored objects because they create themselves when needed:

- `MainMenuRuntime`
- `RunLoadoutApplier`
- `BossSpawnDirector`
- `GameAudio`
- `PlaySessionLogWriter`
- `BombCooldownUI`
- `SecondarySkillCooldownUI`

This means:

- `Main.unity` stays clean and minimal
- `Game.unity` does not need every helper manually placed
- some systems are code-owned rather than scene-owned

When debugging, remember that not every important object exists in the hierarchy before Play mode starts.

## Core Flow

1. Player enters `Main.unity`.
2. `MainMenuRuntime` builds the runtime front-end UI.
3. Player chooses:
   - starting weapon
   - bomb on `Q`
   - active skill on `E`
   - passive perk
4. `RunLoadoutState` stores those choices.
5. `MainMenuRuntime` loads `Game.unity`.
6. `RunLoadoutApplier` waits for the player to exist, then applies the chosen loadout.
7. `RunTimer`, `EnemyWaveDirector`, `EliteSpawnDirector`, and `BossSpawnDirector` drive the run structure.
8. Player fights enemies, levels up, uses active skills, and progresses into elite and boss events.
9. Combat actors carry `FactionMember`, so targeting and damage can support Humans, Angels, Demons, Zombies, and allied units.

## Script Responsibility Map

### `Assets/Scripts/BulletScripts`

#### `Bullet.cs`

Purpose:

- older generic projectile script
- handles damage, pierce, lifetime, and wall collision

Current status:

- appears to be a legacy/simple bullet path
- the active player weapon path now uses `BulletElemental.cs`

### `Assets/Scripts/Combat`

#### `AutoDestroyAfterTime.cs`

Purpose:

- tiny utility that destroys a GameObject after a lifetime

Use case:

- temporary effects or spawned visuals

#### `BossWorldHealthBar.cs`

Purpose:

- creates and maintains the dragon's floating world-space health bar
- positions it above the boss
- updates the fill amount and color

#### `BulletElemental.cs`

Purpose:

- active player projectile logic
- receives a `WeaponDefinition` and applies its behavior at runtime

Responsibilities:

- projectile lifetime
- wall collision
- hostile enemy hit detection
- elemental bullet color
- damage
- splash damage
- pierce
- status effect chance
- faction-safe damage through `FactionCombat`

#### `DamageType.cs`

Purpose:

- defines the combat enums and `DamagePacket` struct

Contains:

- `DamageElement`
- `StatusEffect`
- `DamagePacket`

This is the shared combat payload format for:

- bullets
- enemy contact damage
- enemy projectiles
- bombs
- freeze/slow/burn/poison/shock interactions

#### `FactionCombat.cs`

Purpose:

- centralized faction-aware damage router

Responsibilities:

- check whether an attacker and target are hostile
- prevent friendly fire unless a future system explicitly changes that rule
- route valid damage packets into `PlayerHealth` or `EnemyHealth`
- support child-collider prefabs by searching parent objects for health/faction components

#### `FactionMember.cs`

Purpose:

- faction identity component for anything that can be targeted or damaged as a battlefield actor

Responsibilities:

- stores the actor faction:
  - `Human`
  - `Angel`
  - `Demon`
  - `Zombie`
  - `Neutral`
- controls whether the actor is targetable
- provides `Ensure(...)` helper calls so old prefabs keep working without manual setup

Default runtime assumptions:

- player defaults to `Human`
- existing enemies default to `Zombie`

#### `FactionProjectile.cs`

Purpose:

- simple faction-aware projectile used by runtime allies and future non-player combatants

Responsibilities:

- carries a `DamagePacket`
- stores the firing actor's `FactionMember`
- applies damage only when `FactionCombat` says the target is hostile
- destroys itself after impact or lifetime expiry

#### `FriendlyAlly.cs`

Purpose:

- first-pass Human ally combatant

Responsibilities:

- follows the player in a formation offset
- scans for hostile faction targets
- fires simple faction projectiles
- respects status movement/stun effects

Current status:

- placeholder runtime unit
- intended to be replaced or supplemented by authored Human/Angel/Demon ally prefabs later

#### `FactionTargeting.cs`

Purpose:

- shared target selection rules for faction combat

Responsibilities:

- decides whether two factions are hostile
- assigns target priority by faction matchup
- finds the best target by priority first, then distance

Current design rules:

- Demons prefer Angels, then Humans, then Zombies.
- Angels prefer Demons, then Humans, then Zombies.
- Humans prefer Zombies, then Angels/Demons.
- Zombies attack any other faction.

#### `FactionType.cs`

Purpose:

- shared enum for the game's current faction set

#### `DragonBoss.cs`

Purpose:

- specialized runtime behavior for the dragon boss

Responsibilities:

- receives boss tuning from `BossSpawnDirector`
- scales/tints the boss
- creates the boss world health bar
- handles phase two below 50% HP
- fires special boss volleys
- announces enraged state

#### `EliteEnemy.cs`

Purpose:

- converts a regular enemy into an elite variant

Responsibilities:

- multiply health and rewards
- increase pickup drop chances
- scale the enemy up
- tint the sprite

#### `EnemyHealth.cs`

Purpose:

- enemy HP and death pipeline

Responsibilities:

- receive `DamagePacket`
- apply elemental resistance multipliers
- forward status effects to `StatusReceiver`
- die and award score
- drop XP gems
- roll pickup drops
- raise `Died` event

This script is central to:

- score
- XP
- pickups
- elite tuning
- boss death reward flow

#### `EnemyResistances.cs`

Purpose:

- optional per-enemy elemental multiplier table

Responsibilities:

- lets enemies resist or become weak to specific elements

#### `EnemyRespawnManager.cs`

Purpose:

- main regular enemy spawner for the gameplay scene

Responsibilities:

- holds enemy prefab pool
- finds or uses authored `EnemySpawnPoint`s
- keeps the number of alive enemies near a target cap
- applies wave settings from `EnemyWaveDirector`
- supports special spawns for elites/boss helpers
- validates player distance and enemy spacing

Important note:

- dynamic spawning exists only as inactive/commented future mode right now
- the active mode uses authored spawn points

#### `EnemySpawnPoint.cs`

Purpose:

- scene-authored normal enemy spawn marker

Responsibilities:

- provides a spawn position
- optionally auto-classifies itself into directional regions:
  - `North`
  - `East`
  - `South`
  - `West`
  - `Center`
  - `Any`
- draws scene gizmos for spawn authoring

#### `StatusReceiver.cs`

Purpose:

- common status effect receiver for actors

Responsibilities:

- slow handling
- freeze handling
- burn DOT
- poison DOT
- shock stun
- movement speed multiplier output
- freeze tint
- freeze ring and sparkle visuals
- optional status VFX playback

Used by:

- enemies
- player

#### `WeaponDefinition.cs`

Purpose:

- ScriptableObject weapon data

Responsibilities:

- display name
- element
- status effect payload
- effect chance
- base damage
- fire rate
- bullet speed
- splash radius
- pierce
- projectile prefab reference

### `Assets/Scripts/EnemyScripts`

#### `EnemyMeleeDamage.cs`

Purpose:

- handles touch/contact damage from melee enemies

Responsibilities:

- detects hostile player/enemy collision
- applies repeating timed damage while in contact
- can attach elemental/status payloads to melee hits
- respects faction hostility through `FactionCombat`

#### `EnemyMovement.cs`

Purpose:

- simple chase movement for regular enemies

Responsibilities:

- finds the best hostile faction target
- moves toward that target
- respects `StatusReceiver.SpeedMultiplier`

#### `EnemyProjectile.cs`

Purpose:

- enemy projectile behavior

Responsibilities:

- travel speed
- lifetime
- wall collision
- apply damage and status to hostile actors on hit
- preserve the firing actor's faction so projectiles do not damage allies

#### `RangedShooter.cs`

Purpose:

- ranged enemy combat brain

Responsibilities:

- maintain preferred range from the best hostile faction target
- move in/out to hold that range
- stop acting while stunned/frozen
- spawn `EnemyProjectile`
- expose projectile prefab so the dragon can reuse it

### `Assets/Scripts/GameSystems`

#### `AllySquadSpawner.cs`

Purpose:

- runtime bootstrapper for the first friendly Human ally squad

Responsibilities:

- waits for `Game.unity`
- finds the player
- spawns a small Human squad near the player
- auto-loads `Resources/Prefabs/Factions/HumanAlly` when available
- falls back to generated runtime placeholder allies when no prefab exists
- gives each ally:
  - `FactionMember` set to `Human`
  - `EnemyHealth` with rewards disabled
  - `FriendlyAlly`
  - simple generated placeholder visuals and physics

#### `FactionStarterPrefabBuilder.cs`

Purpose:

- editor-only helper for creating starter faction prefabs

Responsibilities:

- adds a Unity menu item:
  - `Tools > Bullet Heaven > Factions > Create Starter Prefabs`
- creates placeholder faction marker sprites
- creates starter prefabs under:
  - `Assets/Resources/Prefabs/Factions/`

Generated prefabs:

- `HumanAlly`
- `AngelTestUnit`
- `DemonTestUnit`
- `ZombieTestUnit`

#### `BossSpawnDirector.cs`

Purpose:

- run-timed boss spawner

Responsibilities:

- spawn dragon at configured time
- retry spawn if threshold was reached but spawn missed
- use authored `BossSpawnPoint`s when present
- otherwise spawn north/top of player/camera
- choose boss prefab
- tune the dragon
- announce boss spawn and boss death
- trigger boss reward popup

#### `BossSpawnPoint.cs`

Purpose:

- authored boss spawn marker for map-specific boss entrances

Responsibilities:

- gives `BossSpawnDirector` an intentional spawn location
- supports priority sorting

#### `EliteSpawnDirector.cs`

Purpose:

- scheduled elite encounter spawner

Responsibilities:

- spawn elites on a timer
- limit elite count alive
- choose elite prefab
- apply elite modifiers
- show elite announcements

#### `EnemyWaveDirector.cs`

Purpose:

- timed wave stage controller

Responsibilities:

- pick the active enemy wave stage based on elapsed run time
- push wave settings into `EnemyRespawnManager`

Each stage can define:

- start time
- max alive
- respawn delay
- enemy prefab pool
- allowed spawn regions

#### `FactionSkirmishDirector.cs`

Purpose:

- runtime bootstrapper for the first visible Angel/Demon/Zombie battlefield test

Responsibilities:

- waits for `Game.unity`
- spawns a small skirmish near the player after a short delay
- tries to use generated starter prefabs from:
  - `Resources/Prefabs/Factions/AngelTestUnit`
  - `Resources/Prefabs/Factions/DemonTestUnit`
  - `Resources/Prefabs/Factions/ZombieTestUnit`
- falls back to generated placeholder actors when prefabs do not exist
- gives spawned actors faction identity, health, movement, melee damage, physics, and status support

Current intent:

- Angels and Demons should visibly prefer fighting each other.
- Zombies should attack whichever hostile actor is closest.
- Humans should focus Zombies first.

#### `GameAudio.cs`

Purpose:

- centralized SFX playback manager

Responsibilities:

- load clips from `Assets/Resources/Audio/SFX/...`
- keep a small voice pool for overlapping sounds
- randomize clip choice when folders contain multiple sounds
- expose helper calls for all current gameplay events

#### `PlaySessionLogWriter.cs`

Purpose:

- automatic play-session logger

Responsibilities:

- create a session log file per editor play session
- mirror Unity logs to disk
- help debug intermittent issues like spawning or scene handoff problems

#### `RunLoadoutApplier.cs`

Purpose:

- converts pre-run selections into actual gameplay state

Responsibilities:

- wait for `Game` scene and player object
- apply starting weapon preset
- apply passive bonuses
- add/configure `PlayerActiveBomb`
- add/configure `PlayerSecondaryActiveSkill`
- show run-start summary announcement

This is the runtime bridge between menu choices and gameplay behavior.

#### `RunLoadoutState.cs`

Purpose:

- static storage for pre-run selected options

Responsibilities:

- store current:
  - weapon
  - bomb
  - active skill
  - passive
- cycle each option in the menu
- provide names and descriptions for the UI
- build a human-readable summary string

#### `RunTimer.cs`

Purpose:

- authoritative run clock

Responsibilities:

- elapsed time
- start/stop/reset
- formatted timer string
- whole-second/minute events
- run-end event

Drives:

- timer UI
- wave stages
- elites
- boss timing

### `Assets/Scripts/Pickups`

#### `BombPickup.cs`

Purpose:

- world pickup that damages nearby enemies when collected

#### `HealthPickup.cs`

Purpose:

- world pickup that heals the player

#### `MagnetPickup.cs`

Purpose:

- world pickup that attracts all XP gems toward the player

#### `PickupSpriteFactory.cs`

Purpose:

- runtime helper for simple generated pickup sprites and physics

Used by:

- XP gems
- health pickups
- magnet pickups
- bomb pickups
- some runtime UI/visual helpers

#### `PlayerPickup.cs`

Purpose:

- abstract base class for pickups the player can attract and collect

Responsibilities:

- trigger-based collection
- attraction movement toward player
- shared SFX hook
- common pickup lifetime/physics behavior

### `Assets/Scripts/PlayerScripts`

#### `BombAbilityDefinition.cs`

Purpose:

- runtime data definition for the chosen bomb ability

#### `BombExplosionVisual.cs`

Purpose:

- lightweight runtime explosion visual for active bombs

#### `PlayerActiveBomb.cs`

Purpose:

- player bomb skill on `Q`

Responsibilities:

- hold selected bomb config
- track cooldown
- read `Q`
- convert mouse position into target position
- spawn `PlayerBombProjectile`
- expose cooldown/icon info for bomb UI

#### `PlayerBombProjectile.cs`

Purpose:

- travels the thrown active bomb to its target and explodes

Responsibilities:

- move toward cursor-selected target
- apply area damage/status on arrival
- spawn bomb explosion VFX
- play impact SFX

#### `PlayerExperience.cs`

Purpose:

- XP, level, and upgrade choice flow

Responsibilities:

- gain experience
- calculate thresholds
- queue multiple pending level-ups
- request upgrade choices
- apply chosen upgrades

#### `PlayerHealth.cs`

Purpose:

- player HP and death pipeline

Responsibilities:

- receive damage
- i-frames
- knockback
- DOT damage
- heal
- increase max HP
- temporary invulnerability
- game over flow
- restart current scene

#### `PlayerMovement.cs`

Purpose:

- player locomotion with Unity Input System

Responsibilities:

- read `Move` action
- move rigidbody
- apply status-based and stat-based speed multipliers

#### `PlayerPickupCollector.cs`

Purpose:

- attraction radius around player for collectibles

Responsibilities:

- periodically pull nearby pickups toward the player
- scale pickup radius with player stats

#### `PlayerSecondaryActiveSkill.cs`

Purpose:

- player active skill on `E`

Responsibilities:

- hold selected skill config
- track cooldown
- read `E`
- activate chosen skill:
  - `Magnetic Pulse`
  - `Arcane Shield`
  - `Frost Nova`
- expose cooldown/icon data for UI

#### `PlayerShooting.cs`

Purpose:

- primary weapon firing logic

Responsibilities:

- read `Fire` action from `PlayerInput`
- aim toward mouse
- respect weapon fire rate
- apply projectile spread/multi-shot
- spawn weapon bullets
- play shoot SFX

This is the script that actually uses the selected starting weapon every frame.

#### `PlayerStats.cs`

Purpose:

- runtime stat modifier container for player upgrades and loadout bonuses

Tracks:

- damage multiplier
- fire rate multiplier
- move speed multiplier
- pickup radius bonus
- bonus projectiles
- bonus pierce
- bonus splash radius

#### `PlayerUpgradeOption.cs`

Purpose:

- data/logic container for level-up upgrades and boss reward upgrades

Responsibilities:

- apply a single upgrade or upgrade bundle to the player
- provide default upgrade pools
- provide boss reward upgrade pools

#### `SecondaryActiveSkillDefinition.cs`

Purpose:

- runtime data definition for the selected `E` skill

#### `SecondarySkillVisual.cs`

Purpose:

- lightweight runtime pulse/aura visual for `E` skills

### `Assets/Scripts/UIScripts`

#### `BombCooldownUI.cs`

Purpose:

- bottom-left `Q` skill cooldown widget

Responsibilities:

- bootstrap itself in gameplay scene
- track `PlayerActiveBomb`
- show bomb icon
- radial cooldown overlay
- numeric countdown
- bomb name and keybind

#### `CursorScript.cs`

Purpose:

- swaps in a custom hardware cursor if assigned

#### `ExperienceUI.cs`

Purpose:

- binds `PlayerExperience` to level text, XP text, and XP slider

#### `LevelUpManager.cs`

Purpose:

- pause-and-choose upgrade popup

Responsibilities:

- show level-up choices
- show boss reward choices
- pause/resume time scale
- route button clicks to the chosen upgrade handler

#### `MainMenuRuntime.cs`

Purpose:

- runtime front-end UI builder for `Main.unity`

Responsibilities:

- ensure menu `Canvas` and `EventSystem`
- build mode selection screen
- build single-player setup screen
- build multiplayer placeholder screen
- build loadout screen
- write changes into `RunLoadoutState`
- load the gameplay scene

#### `PlayerHealthUI.cs`

Purpose:

- binds `PlayerHealth` to HP text and HP slider

#### `RunAnnouncementUI.cs`

Purpose:

- short message banner for things like:
  - elite incoming
  - elite defeated
  - dragon descends
  - dragon enraged
  - loadout applied summary

#### `RunTimerUI.cs`

Purpose:

- binds `RunTimer` to the on-screen timer text

#### `ScoreManager.cs`

Purpose:

- authoritative score counter

Responsibilities:

- current score
- score changed event
- add/reset score

#### `ScoreTextUI.cs`

Purpose:

- binds `ScoreManager` to the score text

#### `SecondarySkillCooldownUI.cs`

Purpose:

- bottom-left `E` skill cooldown widget

Responsibilities:

- bootstrap itself in gameplay scene
- track `PlayerSecondaryActiveSkill`
- show skill-specific icon
- radial cooldown overlay
- numeric countdown
- skill name and keybind

## Practical Ownership By Feature

### Main menu and run setup

- `MainMenuRuntime.cs`
- `RunLoadoutState.cs`

### Starting weapon selection and primary fire

- `RunLoadoutState.cs`
- `RunLoadoutApplier.cs`
- `WeaponDefinition.cs`
- `PlayerShooting.cs`
- `BulletElemental.cs`
- `PlayerStats.cs`

### Bomb on `Q`

- `PlayerActiveBomb.cs`
- `BombAbilityDefinition.cs`
- `PlayerBombProjectile.cs`
- `BombExplosionVisual.cs`
- `BombCooldownUI.cs`

### Active skill on `E`

- `PlayerSecondaryActiveSkill.cs`
- `SecondaryActiveSkillDefinition.cs`
- `SecondarySkillVisual.cs`
- `SecondarySkillCooldownUI.cs`

### Score, XP, and level-ups

- `ScoreManager.cs`
- `ScoreTextUI.cs`
- `XPGem.cs`
- `PlayerExperience.cs`
- `ExperienceUI.cs`
- `LevelUpManager.cs`
- `PlayerUpgradeOption.cs`

### Pickups

- `PlayerPickup.cs`
- `PlayerPickupCollector.cs`
- `HealthPickup.cs`
- `MagnetPickup.cs`
- `BombPickup.cs`
- `PickupSpriteFactory.cs`

### Enemies, elites, and bosses

- `EnemyRespawnManager.cs`
- `EnemySpawnPoint.cs`
- `EnemyWaveDirector.cs`
- `EliteSpawnDirector.cs`
- `EliteEnemy.cs`
- `BossSpawnDirector.cs`
- `BossSpawnPoint.cs`
- `DragonBoss.cs`
- `BossWorldHealthBar.cs`

### Faction targeting and battlefield AI

- `FactionType.cs`
- `FactionMember.cs`
- `FactionTargeting.cs`
- `FactionCombat.cs`
- `FactionProjectile.cs`
- `FriendlyAlly.cs`
- `AllySquadSpawner.cs`
- `FactionSkirmishDirector.cs`
- `EnemyMovement.cs`
- `RangedShooter.cs`
- `EnemyMeleeDamage.cs`
- `EnemyProjectile.cs`
- `BulletElemental.cs`

### Status effects

- `DamageType.cs`
- `StatusReceiver.cs`
- `EnemyResistances.cs`
- `EnemyHealth.cs`
- `PlayerHealth.cs`

### Audio

- `GameAudio.cs`

### Timer and run structure

- `RunTimer.cs`
- `RunTimerUI.cs`
- `EnemyWaveDirector.cs`
- `EliteSpawnDirector.cs`
- `BossSpawnDirector.cs`

## Notes For A Second Developer

- The active gameplay scene is `Assets/Game.unity`.
- The active front-end scene is `Assets/Scenes/Main.unity`.
- Several important systems bootstrap themselves at runtime, so not everything appears in the hierarchy before Play mode.
- The current enemy spawning system is authored spawn-point based, not fully dynamic.
- The current faction system is a foundation layer. Existing enemies default to Zombies and the player defaults to Human, so old gameplay continues while new faction prefabs can be introduced gradually.
- `Bullet.cs` looks like a legacy/simple projectile path; current player weapon behavior uses `BulletElemental.cs`.
- `SampleScene.unity` is not part of the current game flow.

## Suggested Starting Points For Common Changes

If someone wants to change...

- loadout menu behavior:
  - `MainMenuRuntime.cs`
- which weapons are available in loadout:
  - `RunLoadoutState.cs`
  - `RunLoadoutApplier.cs`
- how a starting weapon behaves:
  - `RunLoadoutApplier.cs`
  - `WeaponDefinition.cs`
  - `PlayerShooting.cs`
- how bombs behave:
  - `PlayerActiveBomb.cs`
  - `PlayerBombProjectile.cs`
- how `E` skills behave:
  - `PlayerSecondaryActiveSkill.cs`
- level-up options:
  - `PlayerUpgradeOption.cs`
  - `PlayerExperience.cs`
  - `LevelUpManager.cs`
- wave pacing:
  - `EnemyWaveDirector.cs`
  - `EnemyRespawnManager.cs`
- boss behavior:
  - `BossSpawnDirector.cs`
  - `DragonBoss.cs`
- UI text/sliders:
  - `ScoreTextUI.cs`
  - `PlayerHealthUI.cs`
  - `ExperienceUI.cs`
  - `RunTimerUI.cs`
  - `BombCooldownUI.cs`
  - `SecondarySkillCooldownUI.cs`
