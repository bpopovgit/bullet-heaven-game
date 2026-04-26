# System Graph

This page gives a high-level map of how the project's current systems connect. It is intentionally simplified so it can be pasted into a game design document or used as a technical orientation page.

## Scene / System Flow

```mermaid
flowchart TD
    A["Main.unity"] --> B["MainMenuRuntime"]
    B --> C["RunLoadoutState"]
    B --> D["Load Game.unity"]

    D --> E["Game.unity"]
    E --> F["RunLoadoutApplier"]
    F --> G["PlayerShooting"]
    F --> H["PlayerActiveBomb (Q)"]
    F --> I["PlayerSecondaryActiveSkill (E)"]
    F --> J["PlayerStats / PlayerHealth"]

    E --> K["RunTimer"]
    K --> L["EnemyWaveDirector"]
    K --> M["EliteSpawnDirector"]
    K --> N["BossSpawnDirector"]

    L --> O["EnemyRespawnManager"]
    O --> P["EnemySpawnPoint"]
    O --> Q["Enemy Prefabs"]

    Q --> R["EnemyMovement / RangedShooter / EnemyMeleeDamage"]
    R --> S["EnemyProjectile"]

    G --> T["WeaponDefinition"]
    G --> U["BulletElemental"]
    U --> V["EnemyHealth"]
    U --> W["StatusReceiver"]

    H --> X["PlayerBombProjectile"]
    X --> V
    X --> W

    I --> V
    I --> W

    V --> Y["ScoreManager"]
    V --> Z["XPGem"]
    V --> AA["Health / Magnet / Bomb Pickups"]

    Z --> AB["PlayerExperience"]
    AA --> AC["PlayerPickupCollector"]

    AB --> AD["LevelUpManager"]
    AD --> AE["PlayerUpgradeOption"]
    AE --> J

    N --> AF["DragonBoss"]
    AF --> AG["BossWorldHealthBar"]
    AF --> AH["Boss Reward Popup"]
    AH --> AE

    AI["GameAudio"] --> G
    AI --> H
    AI --> I
    AI --> R
    AI --> V
    AI --> AD
    AI --> M
    AI --> N
```

## UI Ownership Graph

```mermaid
flowchart LR
    A["ScoreManager"] --> B["ScoreTextUI"]
    C["PlayerHealth"] --> D["PlayerHealthUI"]
    E["PlayerExperience"] --> F["ExperienceUI"]
    G["RunTimer"] --> H["RunTimerUI"]
    I["PlayerActiveBomb"] --> J["BombCooldownUI"]
    K["PlayerSecondaryActiveSkill"] --> L["SecondarySkillCooldownUI"]
    M["RunAnnouncementUI"] --> N["Elite / Boss / Loadout Messages"]
    O["LevelUpManager"] --> P["Level-up and Boss Reward Panel"]
```

## Loadout Ownership Graph

```mermaid
flowchart TD
    A["MainMenuRuntime"] --> B["RunLoadoutState"]
    B --> C["Weapon Choice"]
    B --> D["Bomb Choice"]
    B --> E["Skill Choice"]
    B --> F["Passive Choice"]

    B --> G["RunLoadoutApplier"]
    G --> H["PlayerShooting"]
    G --> I["PlayerActiveBomb"]
    G --> J["PlayerSecondaryActiveSkill"]
    G --> K["PlayerStats / PlayerHealth"]

    H --> L["WeaponDefinition runtime clone"]
    L --> M["BulletElemental"]
```

## Enemy / Boss Escalation Graph

```mermaid
flowchart TD
    A["RunTimer"] --> B["EnemyWaveDirector"]
    A --> C["EliteSpawnDirector"]
    A --> D["BossSpawnDirector"]

    B --> E["EnemyRespawnManager"]
    E --> F["Spawn_01-04 / EnemySpawnPoint"]
    E --> G["Regular Enemy Pool"]

    C --> H["EliteEnemy"]
    D --> I["DragonBoss"]
    I --> J["BossWorldHealthBar"]
    I --> K["Boss Reward Choice"]
```

## Notes

- `Main.unity` is mostly runtime-generated UI.
- `Game.unity` is the real gameplay scene.
- several systems are bootstrap-only and do not need authored scene objects:
  - `RunLoadoutApplier`
  - `BossSpawnDirector`
  - `GameAudio`
  - `PlaySessionLogWriter`
  - `BombCooldownUI`
  - `SecondarySkillCooldownUI`
- the current loadout-selected weapon path uses:
  - `RunLoadoutState`
  - `RunLoadoutApplier`
  - `PlayerShooting`
  - `WeaponDefinition`
  - `BulletElemental`
