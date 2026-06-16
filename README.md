# Stealth Particles

*A mission-based stealth game built for mobile, where you can't just sneak to the exit.*

**[▶ Play the WebGL build](https://hamadalaslani.dev/game)** · or [run in the editor](#run-in-the-editor)

<div align="center">
 <img width="300" alt="level-3" src="https://github.com/user-attachments/assets/2deb0977-4435-4cdc-8c02-37808fa6cc6a" /> <img width="300"  alt="level-4" src="https://github.com/user-attachments/assets/32405433-221d-4020-97a9-442ad61ad948" />
</div>

<div align="center"> 
   <video src="[https://github.com](https://github.com/user-attachments/assets/c19311a8-9fb0-4836-aabe-ca5be570158a)" width="600px" controls></video>

https://github.com/user-attachments/assets/c19311a8-9fb0-4836-aabe-ca5be570158a
</div>

---

Inspired by the **Metal Gear Solid** VR Missions. The loop is simple: avoid the guards, collect the loot, reach the goal. The focus is the systems underneath, so everything renders as primitives and the architecture stays modular and scalable across four levels.

The twist: you can't beeline the exit. Every level gates the goal behind mandatory loot, and you have two single-use powerups to give you an edge.

Halfway through development I came up with some lore. If you've played Streets of Rage before (rhymes!), this will sound familiar:

> The galaxy was once a peaceful and stable place… until the Day of Collapse. A powerful black hole, fueled by unparalleled dominance, emerged at the galactic core. This vicious anomaly quickly began destabilizing its neighboring systems. With its absolute gravitational pull, no cosmic entity is safe.
>
> Amid this turmoil, a group of determined young neutrons have sworn to stabilize their home atoms. To do so, they must put their lives at risk by becoming positively charged, instantly placing themselves on the radar of the delinquent electrons hunting them.
>
> Will they be able to save Rigel Kentaurus?

[Original Streets of Rage intro - YouTube](https://www.youtube.com/watch?v=d5EU7EE8Hc4)

## How to Play

One tap does everything. It's read in priority order by what it hits:

| Tap | Action |
|---|---|
| **Guard** | Hold up, if you're in range and roughly behind it. |
| **Panel** | Disables its linked laser, if you're in range. |
| **Floor** | Move. The point snaps to the NavMesh and your capsule paths there. |

Walk into a powerup to pick it up:

| Powerup | Effect |
|---|---|
| **Eliminate** | One stored charge. Tap any guard to destroy it instantly, no range or angle. |
| **Cloak** | Slip through vision cones. Drops the moment you leave a cone you entered. |

Cloak only fools vision. Touching a guard or crossing a live laser still fails the run.

**You fail** if a cone catches you with line of sight, you touch a guard, you enter an active laser, or the timer hits 0. Finish fast: your time sets your score and your S/A/B/C rank.

## Architecture

Everything communicates through a static **GameEvents** pub/sub hub, so systems never reference each other directly. Adding a subscriber touches no existing code; the cost is discipline (every `+=` in `OnEnable` has a matching `-=` in `OnDisable`).

```
                      ┌──────────────────┐
       GameManager ───┤                  ├─── GuardController
   ScoreCalculator ───┤    GameEvents    ├─── Laser / Panel
      UIController ───┤                  ├─── SfxPlayer
                      └──────────────────┘
```

| System | Pattern | What it does |
|---|---|---|
| **Game flow** | Enum FSM (`GameManager`) | Explicit transition table. Success/Fail are terminal, so a win can't overwrite a loss on the same frame. |
| **Guard AI** | GoF State (`IGuardState`) | `Enter / Tick / Exit`, with `PatrolState` + `DeadState`. New behavior = new class. |
| **Detection** | `VisionCone` | Range + view angle + wall line-of-sight, with a cone mesh generated at runtime. |
| **Input** | Pointer + NavMesh | One abstraction over mouse and touch; a tap raycasts and resolves guard > panel > floor. |
| **Powerups** | Template Method (`Powerup`) | Base runs the shared steps (pick up, announce, disappear); each subclass adds only its effect. |
| **Hazards** | Lasers + panels | A panel toggles its linked laser. |

## Run in the Editor

Clone → open in **Unity 6000.3.16f1 (6.3)** via Unity Hub → open `Assets/_Project/Scenes/MainMenu` → press **Play**.

 Required: URP 17.3.0, Input System 1.19.0, AI Navigation 2.0.12, TextMeshPro.

## Build for iOS

**Need:** a Mac with the latest Xcode, Unity 6.3 with the **iOS Build Support** module, an Apple ID, and a cable (or both on the same network).

1. **File → Build Profiles → iOS → Switch Platform.** Add scenes in play order (MainMenu first).
2. **Player Settings:** set Company / Product name, a unique Bundle ID (`com.yourname.stealthparticles`), min iOS version, Target Device iPhone + iPad, and **Default Orientation → Portrait** (19.5:9).
3. **Build →** pick an empty folder (`Builds/iOS`). Unity generates the Xcode project.
4. Open **Unity-iPhone.xcodeproj → Unity-iPhone target → Signing & Capabilities.** Tick **Automatically manage signing**, choose your **Team** (add your Apple ID under Xcode → Settings → Accounts), confirm the Bundle ID.
5. Connect and unlock the iPhone, tap **Trust**, then **Settings → Privacy & Security → Developer Mode → On.** Select the device in Xcode and press **Run**.
6. First install only: on the iPhone, **Settings → General → VPN & Device Management → [your cert] → Trust**, then relaunch.

## Project Layout

```
Assets/_Project/
├── Scripts/             Core · Guards (+States) · Player · Powerups · Hazards · Loot · Audio · UI
├── Scenes/              MainMenu + Level_01–04
├── ScriptableObjects/   Per-level configs (Level_01–04)
└── Prefabs/ · VFX/ · Art/ · Audio/ · Settings/
```

## Credits

- Author: Hamad Alaslani.
- Inspired by the VR Missions of **Metal Gear Solid**.
- Built with Unity, URP, New Input System, AI Navigation, and TextMeshPro.
- Sound effects: PixaBay.
- Music: Stage 1 (Five Hours by Doerro) - Stage 2 (Hearts in Standby by Moebius FM) - Stage 3 (Blue Fear by Lowland) - Stage 4 (Sunset by Adept)
