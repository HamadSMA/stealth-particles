## Summary
**Inspired by Metal Gear Solid VR Missions**

Stealth Particles is essentially a mission based stealth game streamlined for mobile devices. The core loop is simple:

- Avoid the guards
- Collect the loot
- Head for the goal

There are 4 levels in this game, the idea is building interconnected systems in a modular and scalable way, and showcase how they build on top one another. In addition, the game has a templated powerup system and hazards. More details on how the game is structured → Technical Document


Halfway through development I even came up with some lore to make it interesting. If you have played Streets of Rage before on SEGA Genesis, this will sound familiar:

>The galaxy was once a peaceful and stable place… until the Day of Collapse. A powerful black hole, fueled by unparalleled dominance, emerged at the galactic core. This vicious anomaly quickly began destabilizing its neighboring systems. With its absolute gravitational pull, no cosmic entity is safe.
>
>Amid this turmoil, a group of determined young neutrons have sworn to stabilize their home atoms. To do so, they must put their lives at risk by becoming positively charged—instantly placing themselves on the radar of the delinquent electrons hunting them.
>
>Will they be able to save Rigel Kentaurus?


In this section I will go into details on what the game is, how it plays, and the systems and mechanics underneath. 

For a quick demo →  Play through WebGL <br>
For setup → Build with Unity and Xcode.


## Gameplay

Each level loads into a briefing screen with the objective and a time limit. Tap **Start** to begin. From there:

1. Tap the floor to path-find your capsule around walls toward that point.
2. Collect all the glowing loot spheres by walking into them and then the goal will appear.
3. Slip past or neutralize guards, and disable any lasers blocking your route.
4. Touch the goal to win. Your time decides your score and rank (S/A/B/C).

## Controls

All input is a single tap (mouse in the editor and WebGL, touch on device). One tap is interpreted in priority order by what it hits:

| Tap target | Action |
|---|---|
| A **guard** | Hold it up, but only if you're within holdup range and tapping from roughly behind it.  |
| A **panel** | Disable its linked laser, only if you're within the panel's activation range. |
| **Walkable floor** | Move: the tapped point is snapped to the navmesh and your capsule paths there. |


## Powerups

Powerups are picked up by walking into them. There are two, introduced in the later levels:

| Powerup | Effect | Duration | 
|---|---|---|
| **Eliminate** | Stores one charge. Tap any guard to destroy it instantly, no range or angle needed. | Held until used. |
| **Cloak**  | Lets you pass through a guard's vision cone without being caught (your capsule dims as a tell). | Drops the moment you leave a cone after entering one.| 

Cloak only fools vision cones. Physically touching a guard, or crossing an active laser, still fails the run even while cloaked.

## Timer & Scoring


**Score** (max 2000): `floor(2000 × remaining²)` where `remaining = 1 − elapsed / budget`. Finishing instantly approaches 2000; finishing at the buzzer scores 0.

**Rank**, by fraction of the budget used:

| Rank | Finish within | (at 120 s) |
|---|---|---|
| S | 20% of budget | ≤ 24 s |
| A | 35% | ≤ 42 s |
| B | 65% | ≤ 78 s |
| C | 100% | ≤ 120 s |


## Fail Conditions

The run fails when:

- A guard's **vision cone** sees the uncloaked player with line of sight.
- The player **physically touches** a guard.
- The player enters an **active laser's** detection zone.
- The **timer reaches 0**

## Levels

| # | Name | Introduces | Contents |
|---|---|---|---|
| 1 | **Cyber Space** | Tap-to-move, vision cones, holdups, loot, goal | 2 guards, 3 loot |
| 2 | **Laser Injection** | Lasers and panels | 3 guards, 2 lasers + 2 panels, 3 loot |
| 3 | **Atomic Telekinesis** | Eliminate powerup | 4 guards, 1 laser + 1 panel, Eliminate powerup, 3 loot |
| 4 | **Quantum Stealth** | Cloak powerup | 4 guards, 2 lasers + 2 panels, Cloak + Eliminate powerups, 3 loot |



## Project Structure

All first-party content lives under `Assets/_Project/`, with code grouped by domain and content split by type. Unity-generated folders (`Library/`, `Temp/`, `obj/`, `Builds/`) are not tracked.

```
StealthParticles/
├── Assets/
│   ├── _Project/                  All first-party game content
│   │   ├── Scenes/                MainMenu + the four level scenes
│   │   ├── Scripts/               Gameplay and systems code, grouped by domain
│   │   │   ├── Core/              Game flow (GameManager/GameState), GameEvents hub,
│   │   │   │                      SceneLoader, scoring, timer, progression
│   │   │   ├── Guards/            Guard AI (State pattern) + States/, vision, patrols
│   │   │   ├── Player/            Movement and contact detection
│   │   │   ├── Powerups/          Template-method Powerup base + Cloak/Eliminate
│   │   │   ├── Hazards/           Lasers and panels
│   │   │   ├── Loot/              Loot pickups and tracking
│   │   │   ├── Audio/             Audio manager, SFX player and bank
│   │   │   ├── Config/            ScriptableObject definitions (LevelConfig, etc.)
│   │   │   └── UI/                Main menu and in-game HUD controllers
│   │   ├── ScriptableObjects/     Config assets, one folder per level (Level_01–04)
│   │   ├── Prefabs/               Player, Guard, hazards, loot, powerups, GameplayUI
│   │   ├── VFX/                   Particle prefabs (HoldupBurstFX, GoalBurstFX)
│   │   ├── Resources/             Runtime-loaded assets (SFX bank)
│   │   ├── Art/ · Textures/ · Materials/ · Meshes/ · Shaders/   Visual assets
│   │   ├── Audio/                 Music and SFX clips
│   │   └── Settings/              URP and post-processing settings
│   ├── Settings/                  Project-wide render pipeline assets
│   └── TextMesh Pro/              TMP package resources
├── docs/                          GDD, study notes, design-pattern docs, research
├── Notes/                         Personal topic notes (+ NOTES.md index)
├── Packages/                      UPM manifest and lockfile
└── ProjectSettings/               Unity project settings
```

## Getting Started

1. Install **Unity 6000.3.16f1 (Unity 6.3)** via Unity Hub.
2. Clone the repo and open the project folder in Unity. Required packages (URP 17.3.0, Input System 1.19.0, AI Navigation 2.0.12, uGUI/TextMeshPro).
3. Open `Assets/_Project/Scenes/MainMenu` (or any level scene) and press **Play**.

### Build with Unity and Xcode
- A Mac with the latest Xcode installed.
- Unity 6.3 LTS with the **iOS Build Support** module (add via Unity Hub → Installs → your editor → Add Modules if missing).
- An Apple ID. 
- A connector cable (USB-C or Thunderbolt depending on the phone), or both Mac and device on the same network for wireless deployment.

### Step 1: Switch the build target to iOS
1. Open the project in Unity 6.3 LTS.
2. Go to **File → Build Profiles**.
3. Select **iOS** and click **Switch Platform**. Wait for the reimport to finish.
4. Add the scenes to the build list in play order (main menu first, then levels) under the scene list.

### Step 2: Configure Player Settings
1. In Build Profiles, open **Player Settings** (or **Edit → Project Settings → Player**).
2. Under the iOS tab, set Company Name and Product Name, a unique **Bundle Identifier** (e.g. com.yourname.stealthparticles), the minimum iOS version, and Target Device to iPhone + iPad.
3. Set **Default Orientation** to **Portrait** (the game is designed for 19.5:9 portrait).

### Step 3: Build the Xcode project
1. In Build Profiles, click **Build**.
2. Choose an empty output folder (e.g. Builds/iOS).
3. Unity generates a native Xcode project in that folder.

### Step 4: Open and sign in Xcode
1. Open the output folder and double-click **Unity-iPhone.xcodeproj**.
2. Select the **Unity-iPhone** project, then the **Unity-iPhone** target.
3. Open the **Signing & Capabilities** tab.
4. Tick **Automatically manage signing**.
5. Choose your **Team** (add your Apple ID under Xcode → Settings → Accounts if it is not listed).
6. Confirm the Bundle Identifier matches the one set in Unity.

### Step 5: Deploy to your device
1. Connect and unlock your iPhone and tap **Trust** if prompted.
On the the iPhone: **Settings → Privacy & Security → Developer Mode: On**.
2. Back to Xcode, select your device in the run-destination dropdown in the Xcode toolbar.
3. Press **Run** (▶) or **Product → Run**. Xcode builds and installs the app.

### Step 6: Trust the developer certificate (first install only)
1. On the iPhone: **Settings → General → VPN & Device Management**.
2. Under **Developer App**, tap your certificate and choose **Trust**.
3. Relaunch the app from the home screen.




## Credits / Acknowledgements

- Design, code, and assembly: project author (Hamad Alaslani).
- Inspired by the VR Missions of **Metal Gear Solid**.
- Built with Unity, the Universal Render Pipeline, Input System, AI Navigation, and TextMeshPro.
- Music tracks will be credited here once implemented.
