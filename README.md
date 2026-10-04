# Dungeon Crawler — Final Project

A top-down action game built with Unity **6000.3.20f1** for the Video
Game Development course final project. See [GAME_DESIGN.md](GAME_DESIGN.md)
for the full game/architecture description required for submission.

## How to open and run

1. **Add the two art files first (before opening Unity).** The game's
   sprites come from Unity Technologies' free Asset Store package
   ["2D Roguelike | 2D Sample Project"](https://assetstore.unity.com/packages/templates/tutorials/2d-roguelike-complete-project-299017).
   Its license does not allow the image files in a public repository, so
   only their `.meta` files are committed. Copy `UrbanTheme.png` and
   `SnowTheme.png` from that package's `Roguelike2D/TutorialAssets/Sprites/`
   folder into `Assets/Roguelike2D/TutorialAssets/Sprites/` in this project.
   (If Unity was opened without them and deleted the `.meta` files, run
   `git checkout -- Assets/Roguelike2D` and copy the PNGs again.)
2. Open Unity Hub, add this folder as a project (or open it directly),
   using Editor version **6000.3.20f1**.
3. Open `Assets/Scenes/MainMenu.unity`.
4. Press Play.

## Controls
- Move: **WASD** or **Arrow Keys**
- Attack: **Space** or **Left Mouse Button** (walk over a weapon to pick it up)
- Pause / Resume: **Escape**

## Project layout
```
Assets/
  Scripts/    Gameplay code (Core, Player, Enemy, PowerUps, Weapons, Loot, Level, UI, Effects)
  Scenes/     MainMenu, Level1, Level2, Level3
  Prefabs/    Player, enemies, pickups (power-ups and weapons), exit door, wall, GameManager
  Data/       EnemyData / PowerUpData / WeaponData / LootTable ScriptableObject assets
  Animations/ Animator Controllers + clips for player/enemies
  Input/      GameControls.inputactions (New Input System)
  Art/        Our own weapon icons (Sword, Axe, Hammer)
  Roguelike2D/ Sprites from Unity's free "2D Roguelike" Asset Store sample
```
