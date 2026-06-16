# Stealth Particles — GDD

Top-down stealth puzzler. Rendered in 3D with primitive shapes. Inspired by MGS VR Missions with an added twist. Hint I have Mariofied this game [ =<

> [!NOTE]  
> I have deviated a bit from the GDD. I will note with this label where changes happened.

**Target platform:** Mobile (primary)

## Loop
Tap level → stage starts → make contact with loot to acquire it → rank reveal → tap replay or next level.

**Win:** make contact with the goal object. **Fail:** any guard cone touches you, or you cross an active laser, or you don't finish the stage within the time budget.

> [!NOTE]  
> Also fail when you touch a guard.

## Visual Direction

The game is rendered in 3D with primitive shapes viewed through a top-down camera. Characters are capsules, walls are stretched cubes, goal object and interactables are spheres or cubes with emissive materials.

**Intentional primitive aesthetic.** Visuals are deliberately primitive to focus development on systems architecture, gameplay feel, and clean code. The codebase is structured so character meshes and animations can be swapped in later without modifying gameplay logic. Cyberpunk feel is achieved through lighting, emissive materials, post-processing (bloom, color grading, vignette), and particle effects.

> [!NOTE]  
> Opted for a synthwave look instead of cyberpunk.

**Color language:**
Player: cyan
Guards: magenta
Lasers: acid pink (hazard) - acid green (disabled)
Panels: magenta active → cyan disabled
Loot: acid yellow
Walls: near-black with cyan edge highlight
Floor: near-black base with thin cyan grid lines at low opacity
Vision cones: translucent magenta

> [!NOTE]  
> Guards, walls, floor have a different a color than what was specified.

## Player
Cyan capsule with a small forward indicator (extruded cube or particle trail) so facing direction is readable from above. Tap to move (auto-pathfind around walls). Tap a guard (when close + outside their cone) to hold up. Tap a panel (when close) to disable its laser. No health, no shooting.

## Guards
Red capsule with translucent red vision cone projected on the ground plane. Patrol via waypoints (loop, cycle, or ping-pong patterns, ScriptableObject-driven). Cone is occluded by walls (raycast). No alert/investigation states — you're either detected and it's game over, or you're not.

> [!NOTE]  
> This part has contradictory information likely through generative AI iteration. And game only has loop and ping-pong, no cycle pattern.

**Hold-up:** tap a guard from behind/side within range. Particle burst, sting, guard fades. Does not affect score; purely a tool to clear paths.

## Walls
Axis-aligned rectangular prisms (stretched cubes). Block movement and vision. Dark fill with thin gold edge highlight.

> [!NOTE]  
> Gold edge was a leftover when the game was supposed to have a heist theme. it is now dark magenta with emissive lines

## Floor
Flat ground plane covering the play area. Dark base color with thin neon grid lines forming a tiled pattern.

> [!NOTE]  
> Just a basic grid now, nothing tilted. 

Grid line color matches the cyberpunk palette (cyan or magenta) at low opacity so it reads as ambient detail, not foreground. Grid is part of the level mesh, not a runtime effect, so it has no performance cost.

> [!NOTE]  
> Again no longer cyberpunk.

## Lasers (Level 3 only)
Acid pink beams stretched between two emitter posts. Always on. Crossing one = instant detection (game over).

Each laser is paired with a **panel** placed somewhere in the level. Tap the panel when adjacent to disable that laser for the rest of the run. The beam fades out, the panel turns green, no further effect.

> [!NOTE]  
> Panels turn blue when disabled now.

Lasers and panels are placed manually in the editor. One panel disables one laser (1:1 mapping by reference).

## Powerups
Manually placed per level.

- **Sonic** (Level 3): Increase movement speed for a set amount of time. Speed multiplier will be calibrated when testing.
- **Cloak** (Level 4): Make you invisible for set amount of time. Doesn't work with lasers. Must not touch enemies.

> [!NOTE]  
> There was supposed to be a powerup the gives the player a speed boost. but I decided against it since the levels are so small and the controls are imprecise. Replaced it with the Eliminate powerup.

## Levels
Each is one Unity scene. Each introduces one new system. Sequential unlock (clear N to unlock N+1, stored in PlayerPrefs).

| # | Name | Teaches |
|---|------|---------|
| 1 | CyberSpace | base rules |
| 2 | LaserInjection | lasers + panels |
| 3 | QuectoCheetah | speed |
| 4 | QuantumStealth | invisibility |

## Scoring
Score = floor(2000 * (1 - elapsed/budget)^2)

Time budgets: depends on the level. All values in `ScoringConfig` ScriptableObject with per-level overrides.

> [!NOTE]  
> Wasn't the best idea, since each level needs a reference to the SO now. Easier to just expose the parameters for each level with serialized fields.

## Rank
| Rank | Requirement     |
| ---- | --------------- |
| S    | ≤20% of budget  |
| A    | ≤35% of budget  |
| B    | ≤65% of budget  |
| C    | ≤100% of budget |

Letter shown large with rank-color glow (S=pink, A=gold, B=cyan, C=white). Colors subject to change.

## Camera
Top-down follow camera positioned directly above the player, looking straight down (90°). Smooth follow using SmoothDamp for position. Camera height and offset configurable per level via `LevelConfig`.

> [!NOTE]  
> Camera was supposed to follow the player, but then I decided to make it a one screen level to reduce the scope.

## UI
- **HUD:** timer top-center
**Briefing:** title card with level name, objective, time limit and available powerups. "TAP TO START" indicator. Stays on screen until the player taps, then fades into gameplay.
- **Results:** big letter rank, score breakdown, time, replay/next/menu buttons
- **Fail:** "BUSTED" red overlay, sting, retry/quit

UI support for mobile portrait only.

## Visual Effects
Cyberpunk aesthetic delivered through:
- Emissive materials on player, guards, lasers, loot, panels
- Post-processing volume: bloom, color grading, vignette, slight chromatic aberration
- Dark base lighting with colored accent point lights
- Particle effects for hold-up burst, panel activation, loot collection, rank reveal
- Subtle camera shake on detection and loot pickup

> [!NOTE]  
> No cyberpunk, lightning colors or camera shake in the final game.

## Audio
One looping background track. SFX: tap-to-move (subtle), hold-up sting, powerup pickup (per type), panel-disable confirmation, detected by enemies, success jingle, rank slam.

> [!NOTE]  
> Music are not looping, I had to cut this part because I did not want use my time editing sound files to make the loop perfect, I decided to refine the code and the game design instead. Success jingle was not implemented.

## Controls

The game is designed for mobile (touch input). When playing on PC or in a browser, mouse input maps to touch input automatically.

| Action | Mobile | PC / Browser |
|--------|--------|--------------|
| Move | Tap the ground | Left-click the ground |
| Interact with guard (hold-up) | Tap the guard | Left-click the guard |
| Interact with panel (disable laser) | Tap the panel | Left-click the panel |
| Collect objects | Tap the object | Left-click the object |
| Start level (from briefing screen) | Tap anywhere | Left-click anywhere |

No keyboard input is required. All gameplay is single-input (tap or click).

> [!NOTE]  
> Now you collect loot just by simply walking into it.

## Architecture

### Core systems
- `GuardController` — owns the guard's FSM. States implemented as classes via `IGuardState` interface (PatrolState, ReactingToHoldupState, FrozenState, DeadState). State pattern, not enum switch, for clean separation and easy extension.
- `VisionCone` — exposes `ContainsPoint(point, out blockedByWall)`. Raycast-based wall occlusion. Renders translucent mesh fan on the ground for visualization.
- `PatrolPattern` — ScriptableObject (waypoints + pattern enum: Loop, Cycle, PingPong).
- `GuardConfig` — ScriptableObject (vision range, vision angle, patrol speed, hold-up range, hold-up angle, fade duration). Decouples tunables from prefabs so designers can iterate without code changes.
- `PowerupSystem` — handles powerup pickup, activation, and lifetime.
- `Laser` — line segment with active/inactive state, references its `Panel`.
- `Panel` — interactable, sends disable signal to its linked Laser on tap.
- `LevelConfig` — per-level SO (time budget, rank thresholds, music, briefing text, camera height and offset).
- `ScoringConfig` — project-wide SO with per-level overrides.

> [!NOTE]  
> Implemented only PatrolState and DeadState. No cycle pattern.

### Design principles
- **Separation of gameplay logic and presentation.** FSM drives state, presentation layer (particles, materials, audio) listens via events.
- **ScriptableObject-driven configuration.** Tunables live in assets, not in prefabs or code.
- **Event-driven communication.** Systems publish events rather than directly referencing each other where possible.
- **No singleton-heavy GameManager.** GameManager owns game state transitions; other systems are independent and communicate via events.

## Stretch by order of importance (only if ahead by day 20)
- Backend leaderboards 
- Locked panels with key-carrying guards
- Pulsing lasers objects 
- 5th level combining all mechanics introduced 
- Haptic feedback on certain actions 
- Replace primitives with modal assets and add animations

> [!NOTE]  
> Stretch goals are not considered anymore, the game needs enhancements such as removing inefficient SOs, adding More Guard States, and a new control solution so that playing the game on mobile phones isn't as awkward.