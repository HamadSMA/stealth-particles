## Prefabs & Reusable Level Pieces

A scene built from loose GameObjects has no single source of truth: two guards set
up by hand will drift apart, and fixing the guard's collider means editing it once
per copy. A **prefab** is an asset that *is* the definition of an object; every
placement in a scene is an *instance* that points back at it. Edit the asset and all
instances update. You reach for one the moment an object is either duplicated or
reused across scenes, and carries enough setup (components, wired references,
child hierarchy) that re-doing it by hand is error-prone.

Not everything earns a prefab. The test is **reuse × setup cost**, not "is it
important". The `Main Camera`, `GameManager`, and a level's `Floor` are one-per-scene
with nothing to reuse, so a prefab only adds indirection. The two guards — identical
multi-component stacks (`GuardController`, `NavMeshAgent`, a `VisionCone` with wired
references, a cone-mesh child) — are the opposite case, and so is the `Wall` used to
build every level.

| | Loose GameObjects | Prefab + instances |
| --- | --- | --- |
| Editing N copies | edit each by hand; they drift | edit the asset once; all update |
| Per-placement tweaks | trivially free | expressed as explicit *overrides* |
| Cost | none upfront | one asset; a layer of indirection |
| Best for | true one-offs (camera, floor, managers) | anything duplicated or reused with real setup |

---
### Instances and overrides: one definition, many variations

Reuse rarely means "identical". The two guards differ only in spawn position and
which `PatrolPattern` they run; everything else is shared. A prefab handles exactly
this: the asset holds the common setup, and each instance records just the fields it
*overrides*. Make a `Guard.prefab` whose default carries the `Square` pattern, and
`Guard_Loop` is a plain instance while `Guard_PingPong` is an instance with a single
override — its `patrolPattern` field set to `PingPong`. The shared parts (config,
the holdup VFX reference, the whole vision-cone child) stay sourced from the asset,
so a change there reaches both guards.

**In code (`GuardController.cs`):**

```csharp
[SerializeField] private GuardConfig config;
[SerializeField] private PatrolPattern patrolPattern;
[SerializeField] private Transform playerTransform;
```

- `config` and `patrolPattern` are **asset** references (a `GuardConfig` / `PatrolPattern` ScriptableObject); a prefab stores these cleanly, and overriding `patrolPattern` on one instance is how `PingPong` differs from `Loop`.
- The mental model: the asset is the baseline, an instance is "baseline + a small diff". Keep the diff small — if two instances override most fields, they probably want to be two prefabs (or a [prefab variant](https://docs.unity3d.com/Manual/PrefabVariants.html)).

---
### The scene-reference limitation

A prefab asset lives on disk with no scene around it, so it **cannot store a
reference to a scene object**. `playerTransform` above points at the `Player` in the
scene; the moment the guard becomes a prefab, the asset drops that reference to
`null`. The value survives only as a per-instance override on guards already in the
scene — a fresh instance dragged into a *new* scene comes in with `playerTransform`
empty.

The robust answer is to not depend on the serialized scene link at all: resolve it at
runtime. `GuardController` looks the player up by the `"Player"` tag if its field is
unset, so the prefab works anywhere without manual wiring. The rule generalizes — a
prefab may reference **assets** (materials, other prefabs, ScriptableObjects) freely,
but any **scene** dependency must be either a per-instance override you accept
maintaining, or a runtime lookup (`FindWithTag`, an event, a service locator).

---
### Modular pieces: keep per-instance detail out of the geometry

The `Wall` is the level-design primitive — stamp it, scale it, rotate it. That goal
collides with how decorative detail is usually built. The first version gave each wall
a glowing edge frame made of **thin child cubes**, one per box edge. It looked right,
but the thinness was hand-tuned to each wall's scale, and children inherit a parent's
non-uniform scale: stretch the wall and every edge cube stretches with it, so the
frame turns thick on one axis and thin on another. Detail built from child geometry
is *bound to the instance's scale*, which is exactly the thing a modular piece is
meant to vary.

The fix is to move the detail into the **material**, computed in world units so it
ignores the transform's scale entirely. `WallNeon.shader` draws the edge frame
analytically: for each fragment it measures the world-space distance to the box's
boundary on each axis and lights the fragments near an *edge* (where two of those
distances are small).

| Detail as child geometry | Detail in a shader |
| --- | --- |
| Distorts under non-uniform instance scale | Constant world thickness at any scale |
| Extra GameObjects + draw calls per piece | One material, one draw call |
| Tweak = move/scale many child objects | Tweak = one material property |
| Fine for fixed-size props | The right call for a scalable kit piece |

**In code (`WallNeon.shader`):**

```hlsl
float3 op = IN.positionOS;
float3 scl = float3(
    length(unity_ObjectToWorld._m00_m10_m20),
    length(unity_ObjectToWorld._m01_m11_m21),
    length(unity_ObjectToWorld._m02_m12_m22));
float3 dist = (0.5 - abs(op)) * scl;
float mn = min(dist.x, min(dist.y, dist.z));
float mx = max(dist.x, max(dist.y, dist.z));
float mid = dist.x + dist.y + dist.z - mn - mx;
float border = 1.0 - smoothstep(inner, _BorderWidth, mid);
float3 col = lerp(_FillColor.rgb, _BorderColor.rgb, border);
```

- `op` is the object-space position; a Unity cube spans `-0.5..0.5`, so `0.5 - abs(op)` is the fractional distance to the boundary on each axis.
- `scl` recovers the instance's world scale from the lengths of the `unity_ObjectToWorld` columns (rotation preserves column length, so this is the true per-axis scale). Multiplying converts the fractional distances into **world units** — this is what makes the result scale-independent.
- On any face the *smallest* of the three distances is ~0 (the face you're standing on); the **second-smallest** (`mid`) is the distance to the nearest box edge. Thresholding `mid` against `_BorderWidth` lights a constant-thickness frame on all twelve edges at once, including the bottom seam where the wall meets the floor.
- `_BorderColor` is an HDR value, so the frame blooms; `lerp` keeps the interior `_FillColor`. The whole effect is unlit on purpose — the look is emissive neon, not lit surface (compare the procedural [[synthwave-skybox]], another all-math fragment shader).

With detail in the shader, `Wall.prefab` is a unit cube plus this material: scale an
instance to any dimensions and the border stays crisp, no per-instance fix-up. That is
what makes it usable as the level-building block. Prefabs and shaders compose here —
the prefab gives one editable definition, the shader keeps that definition correct
under the scaling the prefab exists to allow.
