# Dungeon Crawler — Final Project

A top-down action game built with Unity **6000.3.20f1** for the Video
Game Development course final project. See [GAME_DESIGN.md](GAME_DESIGN.md)
for the full game/architecture description required for submission.

## How to open and run

1. Open Unity Hub, add this folder as a project (or open it directly),
   using Editor version **6000.3.20f1**.
2. Open `Assets/Scenes/MainMenu.unity`.
3. Press Play.

## Controls
- Move: **WASD** or **Arrow Keys**
- Attack: **Space** or **Left Mouse Button**
- Pause / Resume: **Escape**

## Project layout
```
Assets/
  Scripts/    Gameplay code (Core, Player, Enemy, PowerUps, Level, UI)
  Scenes/     MainMenu, Level1, Level2, Level3
  Prefabs/    Player, enemies, pickups, exit door, wall, GameManager
  Data/       EnemyData / PowerUpData ScriptableObject assets
  Animations/ Animator Controllers + clips for player/enemies
  Input/      GameControls.inputactions (New Input System)
  Art/        Generated placeholder sprites
```
