# Dungeon Crawler — Final Project Description
name: abed elmhdy 214907099
name: shahed abu amar 213651292
link for git : https://github.com/abedmhdy/final-project.git
## Game Name
Dungeon Crawler

## Genre
Top-down action / arcade combat.

## Short Description
The player fights through 3 dungeon rooms, clearing waves of enemies with
melee combat, collecting power-ups, and reaching the exit door of each
room to progress. Not hypercasual: progress requires clearing enemy waves,
managing health, and using power-ups with limited duration.

## Inspiration / References
Loosely inspired by classic top-down room-clearing combat (in the vein of
early Zelda-style dungeon rooms). Built from scratch for this course —
not copied from a tutorial.

## Main Objective
Clear all enemy waves in Level 1, Level 2 and Level 3 (in order) and walk
through each level's exit door. Reaching the exit of Level 3 wins the
game. Player health reaching 0 at any point ends the game (Game Over).

## Player Controls
Built entirely on Unity's **New Input System** (`Assets/Input/GameControls.inputactions`):

| Action | Binding |
|---|---|
| Move | WASD or Arrow Keys |
| Attack | Space or Left Mouse Button |
| Pause / Resume | Escape |

## Core Mechanics
- Melee attack with a short cooldown, hits every enemy in a small circle in front of the player.
- Enemies patrol until the player is close, then chase and attack.
- Each level spawns its enemies in **waves** — the exit door stays locked until every enemy in every wave is dead.
- Power-up pickups: **Health Pack** (heals), **Speed Boost** and **Damage Boost** (temporary, timed).
- 3 hand-built levels of increasing difficulty (more waves, tougher enemy mix).

## Systems Architecture

The game is split into a small number of systems, each with one clear
responsibility. They mostly talk to each other through C# events instead
of direct references, so a system can be replaced or extended without
rewriting the others.

**Game State Machine (`GameStateManager`)** — the single source of truth
for whether the game is at the Main Menu, Playing, Paused, Game Over, or
Victory. Its only jobs are holding the current state, changing it, and
raising the `OnGameStateChanged` event. It is a small static class and
knows nothing about scenes, levels or `Time.timeScale`. Every other system
reacts to state changes through that event rather than being told
directly what to show.

**Game Flow (`GameManager`)** — runs the actual game flow: starting the
game, loading the next level, restarting, pausing/resuming
(`Time.timeScale`), victory, game over and returning to the main menu. It
owns scene loading and the current level index, and persists across scene
loads. Whenever the flow needs a new state it calls
`GameStateManager.ChangeState(...)`; it never stores the state itself.

**Player systems (`PlayerController`, `PlayerCombat`, `PlayerHealth`)** —
movement, melee attacking, and health are three small classes instead of
one large "Player" god-class. `PlayerHealth` raises `OnHealthChanged` and
`OnPlayerDied` events; it has no idea the UI or the GameManager exist.
The player (and every enemy) faces left/right by flipping its
`SpriteRenderer` (`flipX`); the melee attack circle is mirrored to the
side the player faces.

**Enemy AI — Complex System #1 (`EnemyAI`, `EnemyHealth`, `EnemyData`)** —
every enemy runs its own 4-state machine (Patrol → Chase → Attack → Dead).
All of an enemy type's tuning (health, damage, speed, detection/attack
range) lives in an `EnemyData` ScriptableObject asset, so a new enemy type
is a new asset + prefab, not new code. `EnemyHealth` raises an `OnDied`
event when an enemy dies, which is how the wave system knows a wave is
cleared without needing to know anything about AI states.

**Wave / Spawn System — Complex System #2 (`WaveSpawner`)** — each level
scene has a `WaveSpawner` configured with an ordered list of waves. It
spawns one wave, waits (via each enemy's `OnDied` event) until every enemy
in that wave is dead, then starts the next wave. When the last wave is
cleared it unlocks that level's `ExitDoor`.

**Power-Ups (`PowerUpData`, `PowerUpPickup`)** — deliberately kept simple
and built on the existing player components rather than a separate
system. Each pickup has a `PowerUpData` ScriptableObject asset (type,
value, duration); `PowerUpPickup` checks the type and calls the matching
player component: Health → `PlayerHealth.Heal`, SpeedBoost →
`PlayerController.ApplySpeedBoost`, DamageBoost →
`PlayerCombat.ApplyDamageBoost`.

**UI (`UIStateController`, `HUDController`, `MainMenuButtons`,
`InGameMenuButtons`)** — `UIStateController` listens to
`GameStateManager.OnGameStateChanged` and shows/hides the HUD, Pause, Game
Over and Victory panels accordingly. `HUDController` listens to
`PlayerHealth.OnHealthChanged` and `WaveSpawner.OnWaveChanged` to update
the health bar and wave counter. Buttons call small handler scripts that
forward to `GameManager`.

## Complex System #1 — Enemy AI
See `Assets/Scripts/Enemy/EnemyAI.cs`. A plain `enum`-driven state
machine (no external framework): `Patrol`, `Chase`, `Attack`, `Dead`.
Transitions are simple distance checks against values pulled from the
enemy's `EnemyData` asset. Two enemy types (Grunt, Brute) reuse the exact
same script with different data assets.

## Complex System #2 — Wave / Spawn System
See `Assets/Scripts/Level/WaveSpawner.cs`. Each level defines its waves
in the Inspector as a list of `(enemy prefab, spawn point)` pairs grouped
into waves. The spawner runs them as a coroutine, using each enemy's
death event to know when to advance, and unlocks the exit door and fires
`OnAllWavesCleared` once the level is clear.

## Game State Machine
`MainMenu → Playing → Paused → Playing → GameOver` or
`MainMenu → Playing → Victory`. The state lives in
`Assets/Scripts/Core/GameStateManager.cs` (enum in `GameState.cs`);
`Assets/Scripts/Core/GameManager.cs` decides when each transition happens.

## Levels / Expected Playtime
3 levels (`Level1`, `Level2`, `Level3`), each a single arena room:

| Level | Waves | Enemy mix |
|---|---|---|
| Level 1 | 1 | 3 Grunts |
| Level 2 | 2 | Grunts + Brutes |
| Level 3 | 3 | Grunts + Brutes, final wave is the toughest |

Expected playtime for a full clear: roughly 5–8 minutes.

## Additional Notes
- All game art (player, enemies, floor, walls, exit, pickups) comes from
  Unity Technologies' free **"2D Roguelike | 2D Sample Project"** from the
  Unity Asset Store (Standard Unity Asset Store EULA), mainly its Urban
  theme sprite sheet (`Assets/Roguelike2D/TutorialAssets/Sprites/`). Only
  the sprites were imported, not the sample's scripts, scenes or
  animations. Collision areas, prefab structure and gameplay are
  unchanged. The floor and walls use the SpriteRenderer's Tiled draw mode
  so tiles repeat instead of stretching (each wall's BoxCollider2D size
  matches its tiled size), and the Brute is drawn 1.3× larger than the
  Grunt through its SpriteRenderer size. Movement/attack/hurt/death
  feedback is still delivered through our own Animator-driven scale and
  color animations.
  - Player: hooded scavenger. Grunt: pale zombie. Brute: red-shirt zombie.
  - Health: tomatoes. Speed: soda. Damage: meat (Snow theme sheet).
  - Exit door: the "EXIT" sign. Floor: dirt tile. Walls: rubble tile.
- Built and tested against Unity **6000.3.20f1** only.

## Requirements Checklist

Verified against the three course requirement files (`2026פרויקט גמר
דרישות.docx`, `הנחיות להגשת מטלות.docx`, `Final Projects.pptx`) and, where
marked, an automated Play-mode run driven end-to-end through Unity's own
API (Main Menu → Start → clear Level 1/2/3's waves → Victory → Restart →
damage the player to Game Over → Main Menu), not just a compile check.

| Requirement | Status | Where |
|---|---|---|
| Unity 6000.3.20f1 exactly | Done | `ProjectSettings/ProjectVersion.txt`; every batch run in this session used this exact Editor |
| Basic UI (Start/Pause/End) | Done, automated-verified | `MainMenu.unity`, `UIStateController.cs` — Pause/GameOver/Victory panel switching verified in Play mode |
| Game State Machine | Done, automated-verified | `GameStateManager.cs`, `GameState.cs`, `GameManager.cs` — all 5 states and their transitions verified |
| Complex System #1 — Enemy AI | Done | `EnemyAI.cs` (Patrol/Chase/Attack/Dead per enemy) |
| Complex System #2 — Wave System | Done, automated-verified | `WaveSpawner.cs` — full wave sequencing verified clearing all waves on all 3 levels |
| Animator / Animations | Done | `Assets/Animations/Player`, `Assets/Animations/Enemy` (Idle/Move/Attack/Hurt/Death) |
| Events (reduce coupling) | Done | Player/GameStateManager/Enemy/Wave events — no direct cross-system references for state changes |
| ScriptableObjects | Done | `EnemyData.cs`, `PowerUpData.cs` + 5 data assets |
| New Input System only | Done | `GameControls.inputactions`; no `UnityEngine.Input` usage anywhere in the project |
| Prefabs | Done | 9 prefabs under `Assets/Prefabs` |
| Scalable code | Done | New enemy/power-up type = new asset, not new code; new level = new scene + wave config |
| ~3 levels | Done | Level1 (1 wave) → Level2 (2 waves) → Level3 (3 waves) |
| No game-breaking bugs | Done, automated-verified | Full flow completed with zero exceptions in the final automated run; one real bug found and fixed (see below) |
| Visual Studio 2022 integration | Done | `com.unity.ide.visualstudio` package; `.sln`/`.csproj` regenerate correctly on each compile (not version-controlled, standard practice) |

### Bugs found and fixed during development
- **Enemy prefabs on the wrong physics layer** — `Enemy_Grunt`/`Enemy_Brute`
  were saved on the Default layer instead of the Enemy layer, so the
  player's attack never registered a hit and waves could never clear.
  Fixed by correcting `m_Layer` on both prefabs.
- **`GameManager` NullReferenceException on scene load** — the per-scene
  duplicate `GameManager` (which self-destroys since only the first one
  persists) still tried to unsubscribe input events it never subscribed
  to. Fixed by guarding `OnEnable`/`OnDisable` with an `Instance == this`
  check.
- **Characters never turned around** — movement code flipped the
  character's `localScale.x`, but the Animator clips also animate
  `localScale` and overwrote the flip every frame (and the Brute's larger
  scale). Fixed by facing with `SpriteRenderer.flipX` and sizing the Brute
  through its SpriteRenderer, neither of which the Animator touches.
- **Attacks registered while paused** — input callbacks keep firing when
  `Time.timeScale` is 0, so one attack could land during Pause/Game
  Over/Victory. `PlayerCombat` now ignores attacks while time is frozen.
