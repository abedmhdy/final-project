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
melee combat, collecting power-ups and stronger weapons, and reaching the
exit door of each room to progress. Not hypercasual: progress requires
clearing enemy waves, managing health, and using power-ups with limited
duration. The player starts with a Sword and earns an Axe and then a
Hammer as the rooms get harder.

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
- Melee attack that hits every enemy in a small circle in front of the player. Its damage, reach, cooldown and knockback come from the equipped weapon.
- **Weapons:** Sword (starter, balanced), Axe (more damage, shorter reach, slower), Hammer (most damage and knockback, shortest reach, slowest). Walking over a weapon pickup equips it if it is an upgrade. The weapon is drawn in the player's hand, follows the facing direction and swings on every attack.
- **Loot:** each defeated enemy has a small chance to drop a pickup (health, speed, damage, or rarely an Axe from a Brute).
- Enemies patrol until the player is close, then chase and attack.
- Each level spawns its enemies in **waves** — the exit door stays locked until every enemy in every wave is dead.
- Power-up pickups: **Health Pack** (heals), **Speed Boost** and **Damage Boost** (temporary, timed).
- 3 hand-built levels of increasing difficulty (more waves, tougher enemy mix), each with a guaranteed weapon reward (see Levels).
- Feedback: floating damage numbers and knockback on hits, short HUD messages for waves, "Room Cleared", level start, pickups and weapon upgrades.

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
in that wave is dead, then starts the next wave. A wave can name an
optional reward pickup that appears when the wave is cleared (this is how
the weapon upgrades are guaranteed). When the last wave is cleared it
unlocks that level's `ExitDoor`.

**Pickups (`Pickup`, `PowerUpPickup`, `WeaponPickup`)** — `Pickup` is a
small abstract base class that handles "the player walked over me": it
asks the subclass to apply itself, raises the static `OnCollected` event
(or `OnRejected` if it couldn't be used) and removes itself from the room.
`PowerUpPickup` reads its `PowerUpData` ScriptableObject asset (type,
value, duration) and calls the matching player component: Health →
`PlayerHealth.Heal`, SpeedBoost → `PlayerController.ApplySpeedBoost`,
DamageBoost → `PlayerCombat.ApplyDamageBoost`. `WeaponPickup` asks
`PlayerCombat` to equip its weapon.

**Weapons (`WeaponData`, `PlayerCombat`, `WeaponVisual`)** — each weapon
is a `WeaponData` ScriptableObject asset (name, tier, damage, attack
range, attack cooldown, knockback, icon). `PlayerCombat` has one attack
routine that reads the equipped weapon's numbers, so there is no
per-weapon code; a new weapon is a new asset. `PlayerCombat.TryEquipWeapon`
only accepts a higher tier, so the player can't swap down by accident, and
raises `OnWeaponChanged` for the HUD. `WeaponVisual` draws the weapon in
the player's hand: it copies the character's `flipX` every frame (so it
never fights the facing system) and plays a short swing on each attack.
Each level scene sets the player's starting weapon on its Player
instance, so restarting a level is always fair.

**Loot (`LootTable`, `LootDropper`)** — a `LootTable` ScriptableObject
holds a drop chance and a weighted list of pickup prefabs. `LootDropper`
sits on each enemy prefab and listens to that enemy's `EnemyHealth.OnDied`
event, so neither `EnemyHealth` nor the wave system knows about loot.
Grunts drop power-ups (25%); Brutes drop more often (40%) and can drop an
Axe.

**UI (`UIStateController`, `HUDController`, `MainMenuButtons`,
`InGameMenuButtons`)** — `UIStateController` listens to
`GameStateManager.OnGameStateChanged` and shows/hides the HUD, Pause, Game
Over and Victory panels accordingly. `HUDController` only listens to
events: `PlayerHealth.OnHealthChanged` (health bar and "5/5"),
`PlayerCombat.OnWeaponChanged` (weapon icon, name and damage),
`Pickup.OnCollected` (pickup message and boost countdown),
`WaveSpawner.OnWaveChanged` / `OnAllWavesCleared` / `OnRewardSpawned`
(level and wave counter, "Wave 2/3", "Room Cleared!", "New weapon").
Buttons call small handler scripts that forward to `GameManager`.

**Effects (`DamagePopup`, `PickupBob`)** — `EnemyHealth` spawns a floating
damage number on every hit (created from code with Unity's built-in
font); weapon pickups bob gently so they stand out.

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

| Level | Waves | Enemy mix | Weapon progression |
|---|---|---|---|
| Level 1 | 1 | 3 Grunts | Start with the **Sword**. Health and Speed pickups in the room. |
| Level 2 | 2 | Grunts + Brutes | Start with the Sword. Clearing wave 1 always drops an **Axe**. 3 Health pickups and a Speed pickup in the room. |
| Level 3 | 3 | Grunts + Brutes, final wave is the toughest | Start with the Axe. Clearing wave 1 always drops the **Hammer**, before the two hardest waves. |

Weapon stats (`Assets/Data/Weapons`):

| Weapon | Tier | Damage | Range | Cooldown | Knockback |
|---|---|---|---|---|---|
| Sword | 1 | 1 | 0.8 | 0.4 s | 2.5 |
| Axe | 2 | 2 | 0.7 | 0.6 s | 4 |
| Hammer | 3 | 4 | 0.6 | 0.9 s | 6 |

The Sword matches the original attack exactly. Grunts have 2 HP and
Brutes 4 HP, so the Axe kills a Grunt in one hit and the Hammer kills a
Brute in one hit.

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
- The pack has no separate weapon sprites (the only weapon art is baked
  into the player's attack frames), so the Sword, Axe and Hammer icons
  (`Assets/Art/Weapons/`) are small pixel-art sprites we drew ourselves
  in the pack's palette and 32 pixels-per-unit scale. They are used for
  the pickup, the weapon in hand and the HUD icon, and can be replaced by
  changing the `icon` field of each `WeaponData` asset.
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
| ScriptableObjects | Done | `EnemyData.cs`, `PowerUpData.cs`, `WeaponData.cs`, `LootTable.cs` + 10 data assets |
| New Input System only | Done | `GameControls.inputactions`; no `UnityEngine.Input` usage anywhere in the project |
| Prefabs | Done | 11 prefabs under `Assets/Prefabs` |
| Scalable code | Done | New enemy/power-up/weapon/loot table = new asset, not new code; new level = new scene + wave config |
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
