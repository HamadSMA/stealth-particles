# Stealth Particles — Documentation


### Project Foundations

- [Project Setup](Notes/project-setup.md): git repo setup, git workflow (branch-per-feature), naming conventions (assets + C# casing), feature-based project structure, working with Claude & MCP

### Unity Systems

- [Unity Base Classes & Interfaces](Notes/unity-classes-and-interfaces.md): MonoBehaviour lifecycle, OnEnable/OnDisable, ScriptableObject, SO vs hardcoding, [SerializeField], [ContextMenu], [RequireComponent], [CreateAssetMenu]
- [Unity Design Patterns](Notes/design-patterns.md): singleton, observer/event hub, state machine, enum+switch vs State pattern (guard FSM: IGuardState/PatrolState/DeadState terminal state/GuardController), ScriptableObject architecture, composition, pooling — with tradeoffs
- [NavMesh Navigation](Notes/navmesh-navigation.md): NavMeshSurface baking, Collect Objects modes, agent types, voxel size & carve aliasing, NavMeshAgent tuning, NavMesh.SamplePosition
- [Unity Input System (new)](Notes/input-system.md): old vs new stack, activeInputHandler, Mouse.current/Touchscreen.current, wasPressedThisFrame vs isPressed (tap vs hold), ReadValue
- [Raycasting & LayerMasks](Notes/raycasting-and-layermasks.md): ScreenPointToRay, Physics.Raycast, layers, LayerMask bitmask test, masked-raycast tunneling bug & fix
- [Top-Down Follow Camera](Notes/top-down-camera.md): LateUpdate timing, Vector3.SmoothDamp smoothing & stored velocity, fixed cameraAngle rotation, perspective vs orthographic, ApplyLevelConfig per-level hook
- [Vision Cone & Detection](Notes/vision-cone.md): range/angle/occlusion line-of-sight test, per-frame cone mesh clipped by walls, Wall layer mask, detection wired into the FSM (OnPlayerDetected → Fail), two fail routes (cone + physical contact) through one event
- [Procedural Synthwave Skybox](Notes/synthwave-skybox.md): shading by view direction, height gradient + horizon glow, sun as a perspective projection (disk/gradient/bands/halo), hashed star field, single-pass region masking to keep stars off the sun
- [Prefabs & Reusable Level Pieces](Notes/prefabs.md): when to prefab (reuse × setup cost), instances + overrides, scene references not serializing into prefab assets (runtime tag lookup), keeping per-instance detail out of child geometry via a scale-independent world-space border shader (WallNeon)

### Game Design

- [Level Flow — Objective, Win/Lose, Results](Notes/level-flow.md): objective chain (loot → all-loot event → hidden goal reveals/unlocks → goal reached → Success), one Fail funnel for vision/contact/timeout, score+rank computed at the win from a ScoringConfig, ordering a shared event with DefaultExecutionOrder (ScoreCalculator before ResultsScreen)
- [Guard Holdup & Death](Notes/holdup-mechanic.md): tap-vs-hold input paths (fresh press for holdup, hold for movement), Guard layer + guardMask tap targeting, range + rear-arc takedown rule, DeadState terminal state, one-shot HoldupBurstFX particle (Play On Awake + Stop Action Destroy)
- [Laser Gates & Disable Panels](Notes/laser-gates-and-panels.md): toggleable hazard + remote switch pair, OverlapBox detection on the shared OnPlayerDetected→Fail funnel, once-latch (hasDetected), SetActive hides beam renderers, Panel layer + panelMask as a third tap-target after the holdup ray, range-gated one-shot TryDisable, decoupled linkedLaser reference, building a level out of independent pairs
