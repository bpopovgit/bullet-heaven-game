# Getting Started

## Requirements

- Unity `6000.3.14f1`
- Git
- A C# editor such as Visual Studio, Rider, or VS Code

## Clone the Project

```bash
git clone https://github.com/bpopovgit/bullet-heaven-game.git
cd bullet-heaven-game
```

Open the folder in Unity Hub using Unity `6000.3.14f1`.

## First Scenes to Inspect

Menu flow starts in:

```text
Assets/Scenes/Main.unity
```

Gameplay happens in:

```text
Assets/Game.unity
```

Also check:

```text
ProjectSettings/EditorBuildSettings.asset
```

## First Test Run

1. Open `Assets/Scenes/Main.unity`.
2. Press Play.
3. Enter Single Player.
4. Open Loadout.
5. Choose a weapon, bomb, active skill, and passive.
6. Start the run.
7. Move, shoot, use `Q`, use `E`, kill enemies, collect XP.

## Expected Behavior

- Menu loads before gameplay.
- Loadout choices carry into the run.
- Score increases when enemies die.
- Green XP gems drop from enemies.
- XP gems fly toward the player when inside pickup radius.
- Level-ups show an upgrade popup if `LevelUpManager` is wired.
- Timed elites and the dragon boss can appear during the run.
