# Stealth Particles

**A neon top-down stealth run — slip past the cones, grab the loot, beat the clock.**

Stealth Particles is a top-down stealth game built in Unity, inspired by the VR Missions of Metal Gear Solid and streamlined for touch with an added twist: powerups. You tap to move a cyan capsule through a walled, synthwave-lit level, avoid patrolling guards and laser hazards, collect glowing loot, and reach the goal before the time budget runs out. Finish faster for a better rank. Levels are rendered in 3D with deliberately primitive shapes, so the look comes from lighting, emissive materials, post-processing, and particles rather than detailed art.

## Status / Platform

- **Engine:** Unity 6000.3.16f1 (Unity 6.3), Universal Render Pipeline 17.3.0.
- **Target platforms:** iOS (iPhone/iPad) and WebGL. Developed on macOS.
- **Build status:** Playable in-editor. Four levels plus an intro and a level-select menu are wired into the build. Project identifiers (product name, bundle ID) are still at their Unity/URP-template defaults and the project is not yet orientation-locked — both must be set before an iOS build.

## Gameplay

Each level loads into a briefing screen with the objective and a time limit. Tap **Start** to begin. From there:

1. Tap the floor to path-find your capsule around walls toward that point.
2. Collect all the glowing loot spheres by walking into them — this reveals the goal.
3. Slip past or neutralize guards, and disable any lasers blocking your route.
4. Touch the goal to win. Your time decides your score and rank (S/A/B/C).

The clock is always running. Get seen, get touched, cross an active laser, or run out of time and the run fails.

## Controls

All input is a single tap (mouse in the editor, touch on device). One tap is interpreted in priority order by what it hits:

| Tap target | Action |
|---|---|
| A **guard** | Hold it up — only if you're within holdup range and tapping from roughly behind it. With an Eliminate charge, instead destroys the guard outright (no range/angle needed). |
| A **panel** | Disable its linked laser — only if you're within the panel's activation range. |
| **Walkable floor** | Move: the tapped point is snapped to the navmesh and your capsule paths there. |

There is no manual rotation, health, or shooting — facing follows your movement automatically.

## Guards

Guards patrol fixed waypoint routes (looping or ping-ponging between points), pausing briefly at each waypoint. Each guard projects a **vision cone** (range 8, total angle 60°) that respects walls — line of sight is blocked by cover. You are caught if the cone sees you, **or** if you physically bump into a guard (a separate contact check, ~0.6 unit radius, that catches you even from behind or while cloaked). Either way the run fails immediately.

You can neutralize a guard by tapping it from behind within holdup range, or instantly with an Eliminate powerup. A neutralized guard plays a burst effect and fades out.

## Obstacles: Lasers & Panels

**Lasers** are beam volumes that fail the run the instant the player enters their danger zone (a thin, wide detection box along the beam). Active beams glow acid pink.

**Panels** are tap-to-use switches, each linked to one laser. Tap a panel while standing within its activation range (3.5 units) and its laser shuts off for the rest of the run. The panel itself flips from magenta (armed) to cyan (used) and is single-use. A disabled laser's beam simply turns off.

## Powerups

Powerups are picked up by walking into them. There are two, introduced in the later levels:

| Powerup | Effect | Duration | Pickup |
|---|---|---|---|
| **Eliminate** (Atomic Telekinesis) | Stores one charge. Tap any guard to destroy it instantly — no range or angle needed. | Held until used (no timer). | Walk into the red pickup. |
| **Cloak** (Quantum Stealth) | Lets you pass through a guard's vision cone without being caught (your capsule dims as a tell). | Drops the moment you leave a cone after entering one — not a fixed timer. | Walk into the blue pickup. |

Cloak only fools vision cones. Physically touching a guard, or crossing an active laser, still fails the run even while cloaked.

## Loot, Goal, Timer & Scoring

Every level places loot spheres and requires you to collect all of them (3 of 3 in each shipped level) before the goal becomes reachable. The goal stays hidden until the loot requirement is met; touching it then wins the level.

The level timer counts up from zero and is shown as a remaining-time countdown in the HUD. Your finish time drives both score and rank, measured against the level's time budget (120 s in every level):

**Score** (max 2000): `floor(2000 × remaining²)` where `remaining = 1 − elapsed / budget`. Finishing instantly approaches 2000; finishing at the buzzer scores 0.

**Rank**, by fraction of the budget used:

| Rank | Finish within | (at 120 s) |
|---|---|---|
| S | 20% of budget | ≤ 24 s |
| A | 35% | ≤ 42 s |
| B | 65% | ≤ 78 s |
| C | 100% | ≤ 120 s |

Best rank and score per level are saved, and clearing a level unlocks the next.

## Fail Conditions

The run fails (a single internal "detected" event drives all of them) when:

- A guard's **vision cone** sees the uncloaked player with line of sight.
- The player **physically touches** a guard (independent of the cone; ignores cloak).
- The player enters an **active laser's** detection zone.
- The **timer reaches the level's time budget** (time-out).

## Levels

| # | Name | Introduces | Contents |
|---|---|---|---|
| 1 | **Cyber Space** | Tap-to-move, vision cones, holdups, loot, goal | 2 guards, 3 loot |
| 2 | **Laser Injection** | Lasers and panels | 3 guards, 2 lasers + 2 panels, 3 loot |
| 3 | **Atomic Telekinesis** | Eliminate powerup | 4 guards, 1 laser + 1 panel, Eliminate powerup, 3 loot |
| 4 | **Quantum Stealth** | Cloak powerup | 4 guards, 2 lasers + 2 panels, Cloak + Eliminate powerups, 3 loot |

All levels share a 120 s budget and a 3-loot requirement; difficulty scales through layout, guard routes, and the hazard/powerup mix.

## Visual Style

A synthwave / cyberpunk aesthetic built from lighting and post rather than art assets. The palette as set in the materials: player cyan, guards magenta, armed panels magenta flipping to cyan when used, active lasers acid pink, loot green, goal acid yellow, vision cones translucent orange, walls and floor near-black purple with neon edge glow (custom `WallNeon` shader). Cyan and magenta point lights accent each room, over a synthwave skybox.

Post-processing (URP `CyberpunkMoodyProfile`): bloom (the glow on every emissive surface), a cool white-balance shift, boosted contrast and saturation, an edge vignette that flashes red when you're caught, and subtle film grain.

## Tech Overview & Architecture

- **Event hub (`GameEvents`):** a static pub/sub class. Systems never reference each other directly — they raise and subscribe to events (state changes, detection, goal reached, loot collected, timer ticks, etc.), subscribing in `OnEnable` and unsubscribing in `OnDisable`.
- **Game state machine (`GameManager`):** owns `Briefing → Playing → Success/Fail` with validated transitions, and broadcasts state changes that drive everything else.
- **Guard AI:** a small `IGuardState` pattern (`PatrolState`, `DeadState`, `FrozenState`) per guard, with vision handled by a dedicated `VisionCone` that does both the logical detection test and a wall-clipped cone mesh.
- **ScriptableObject-driven config:** `LevelConfig`, `ScoringConfig`, `GuardConfig`, and `PatrolPattern` hold tunable data as assets, so levels, scoring, and guard behavior are edited without touching code.
- **Progression:** `ProgressionManager` persists unlocks, best rank, and best score in PlayerPrefs; `SceneLoader` wraps scene navigation by build index.

Detailed per-system documentation lives in [`docs/research/`](docs/research/).

## Project Structure

```
Assets/_Project/
├── Art/Intro/            Synthwave intro backdrop (Sky, Sun, Grid)
├── Audio/                Music tracks (SFX bank clips not yet assigned)
├── Materials/            Emissive neon materials for every object
├── Prefabs/              Player, Guard, Laser, Panel, Loot, Goal, Wall, Level, GameplayUI, powerups
├── Scenes/               Intro, MainMenu, and the four level scenes (each with its NavMesh)
├── ScriptableObjects/    Level configs, scoring config, guard config, patrol patterns, SFX bank
├── Scripts/
│   ├── Audio/            AudioManager, SfxPlayer, SfxBank
│   ├── Config/           LevelConfig, ScoringConfig, Rank
│   ├── Core/             GameEvents, GameManager, GameState, Goal, LevelTimer,
│   │                     ScoreCalculator, ProgressionManager, SceneLoader, camera/vignette/logging
│   ├── Guards/           GuardController, VisionCone, GuardConfig, PatrolPattern + States/
│   ├── Hazards/          Laser, Panel
│   ├── Loot/             Loot, LootManager
│   ├── Player/           PlayerMovement, PlayerContactDetector, PlayerTrailEmitter
│   ├── Powerups/         Powerup base, CloakPowerup, EliminatePowerup, PowerupSystem, PowerupType
│   └── UI/               UIController, MainMenuController, IntroController
├── Settings/             Post-processing profile, synthwave skybox
├── Shaders/              WallNeon, SynthwaveSkybox
├── Textures/             FloorGrid, CircuitTrace
└── VFX/                  GoalBurst, HoldupBurst, LootPop, PanelPuff, PlayerTrail
```

## Getting Started

1. Install **Unity 6000.3.16f1 (Unity 6.3)** via Unity Hub.
2. Clone the repo and open the project folder in Unity. Required packages (URP 17.3.0, Input System 1.19.0, AI Navigation 2.0.12, uGUI/TextMeshPro, ProBuilder) are declared in `Packages/manifest.json` and resolve automatically on first open.
3. Open `Assets/_Project/Scenes/Intro.unity` (or any level scene) and press **Play**. In a level, click **Start** in the briefing, then click the floor to move, a guard to hold up, or a panel to disable a laser.
4. To play the full flow from the menu, start from `Intro` so the build-order scene navigation and level unlocks work as intended.

## Building for iOS (iPhone / iPad)

> The committed project still uses the default Product Name (`My project`) and bundle identifier (`com.Unity-Technologies.com.unity.template.urp-blank`), with iOS minimum version 15.0. Set your own Product Name and a unique Bundle Identifier in Step 2 before building.

### Prerequisites
- A Mac with the latest Xcode installed (Mac App Store).
- Unity 6.3 LTS with the **iOS Build Support** module (add via Unity Hub → Installs → your editor → Add Modules if missing).
- An Apple ID. A free Apple ID works for installing on your own device; a paid Apple Developer Program membership is required for TestFlight / App Store distribution.
- A USB cable, or both Mac and device on the same network for wireless deployment.

### Step 1 — Switch the build target to iOS
1. Open the project in Unity 6.3 LTS.
2. Go to **File → Build Profiles**.
3. Select **iOS** and click **Switch Platform**. Wait for the reimport to finish.
4. Add the scenes to the build list in play order (main menu first, then levels) under the scene list.

### Step 2 — Configure Player Settings
1. In Build Profiles, open **Player Settings** (or **Edit → Project Settings → Player**).
2. Under the iOS tab, set Company Name and Product Name, a unique **Bundle Identifier** (e.g. com.yourname.stealthparticles), the minimum iOS version, and Target Device to iPhone + iPad.
3. Set **Default Orientation** to **Portrait** (the game is designed for 19.5:9 portrait).

### Step 3 — Build the Xcode project
1. In Build Profiles, click **Build**.
2. Choose an empty output folder (e.g. Builds/iOS).
3. Unity generates a native Xcode project in that folder.

### Step 4 — Open and sign in Xcode
1. Open the output folder and double-click **Unity-iPhone.xcodeproj**.
2. Select the **Unity-iPhone** project, then the **Unity-iPhone** target.
3. Open the **Signing & Capabilities** tab.
4. Tick **Automatically manage signing**.
5. Choose your **Team** (add your Apple ID under Xcode → Settings → Accounts if it is not listed).
6. Confirm the Bundle Identifier matches the one set in Unity.

### Step 5 — Deploy to your device
1. Connect and unlock your iPhone/iPad; tap **Trust** if prompted.
2. Select your device in the run-destination dropdown in the Xcode toolbar.
3. Press **Run** (▶) or **Product → Run**. Xcode builds, installs, and launches the app.

### Step 6 — Trust the developer certificate (first install only)
1. On the device: **Settings → General → VPN & Device Management**.
2. Under **Developer App**, tap your certificate and choose **Trust**.
3. Relaunch the app from the home screen.

### Notes
- Free Apple IDs allow on-device installs, but the provisioning profile expires after 7 days; rebuild from Xcode to reinstall.
- For TestFlight or App Store distribution, use **Product → Archive** with a paid Apple Developer Program account.

## Play in the Browser (WebGL)
A WebGL build is available here: **[WebGL build — link coming soon]**

*(Placeholder — replace with the hosted URL, e.g. an itch.io page, once published.)*

## Credits / Acknowledgements

- Design, code, and assembly: project author (Hamad Alaslani).
- Inspired by the VR Missions of **Metal Gear Solid**.
- Built with Unity, the Universal Render Pipeline, Input System, AI Navigation, ProBuilder, and TextMeshPro.
- Music tracks are third-party synthwave pieces used during development; replace or clear rights before public distribution.
