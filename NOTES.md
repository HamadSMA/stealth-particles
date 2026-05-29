# Stealth Particles — Documentation


### Project Foundations

- [Project Setup](Notes/project-setup.md): git repo setup, git workflow (branch-per-feature), naming conventions (assets + C# casing), feature-based project structure, working with Claude & MCP

### Unity Systems

- [Unity Base Classes & Interfaces](Notes/unity-classes-and-interfaces.md): MonoBehaviour lifecycle, OnEnable/OnDisable, ScriptableObject, SO vs hardcoding, [SerializeField], [ContextMenu], [RequireComponent], [CreateAssetMenu]
- [Unity Design Patterns](Notes/design-patterns.md): singleton, observer/event hub, state machine, enum+switch vs State pattern (guard FSM: IGuardState/PatrolState/GuardController), ScriptableObject architecture, composition, pooling — with tradeoffs
- [NavMesh Navigation](Notes/navmesh-navigation.md): NavMeshSurface baking, Collect Objects modes, agent types, voxel size & carve aliasing, NavMeshAgent tuning, NavMesh.SamplePosition
- [Unity Input System (new)](Notes/input-system.md): old vs new stack, activeInputHandler, Mouse.current/Touchscreen.current, wasPressedThisFrame vs isPressed (tap vs hold), ReadValue
- [Raycasting & LayerMasks](Notes/raycasting-and-layermasks.md): ScreenPointToRay, Physics.Raycast, layers, LayerMask bitmask test, masked-raycast tunneling bug & fix
- [Top-Down Follow Camera](Notes/top-down-camera.md): LateUpdate timing, Vector3.SmoothDamp smoothing & stored velocity, fixed cameraAngle rotation, perspective vs orthographic, ApplyLevelConfig per-level hook
- [Vision Cone & Detection](Notes/vision-cone.md): range/angle/occlusion line-of-sight test, per-frame cone mesh clipped by walls, Wall layer mask, detection wired into the FSM (OnPlayerDetected → Fail), temporary BUSTED freeze
