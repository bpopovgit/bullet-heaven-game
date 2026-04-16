# Getting Started

## Requirements

- Unity `2022.3.56f1`
- Git
- A C# editor such as Visual Studio, Rider, or VS Code

## Clone the Project

```powershell
git clone https://github.com/bpopovgit/bullet-heaven-game.git
cd bullet-heaven-game
```

Open the folder in Unity Hub using Unity `2022.3.56f1`.

## First Scene to Inspect

The active gameplay work has been happening in:

```text
Assets/Game.unity
```

Also check:

```text
Assets/Scenes/
ProjectSettings/EditorBuildSettings.asset
```

before making a build, because the scene used during development may differ from the scene currently included in Build Settings.

## First Test Run

1. Open the gameplay scene.
2. Confirm the scene has a `Player`.
3. Confirm the scene has a `ScoreManager`.
4. Confirm the scene has an enemy spawning object with `EnemyRespawnManager`.
5. Press Play.
6. Move, shoot, kill enemies, collect green XP gems.

## Expected Behavior

- Score increases when enemies die.
- Green XP gems drop from enemies.
- XP gems fly toward the player when inside pickup radius.
- Level-ups show an upgrade popup if `LevelUpManager` is wired.
- If no `LevelUpManager` exists, the game auto-picks an upgrade and logs it in the Console.

