## NavMesh Navigation

Unity's built-in pathfinding. Instead of moving a character by hand and writing
your own obstacle avoidance, you bake a **NavMesh** — a simplified walkable
surface generated from your level geometry — and let an agent figure out how to
get from A to B across it. This note covers baking the surface, the agent that
walks it, and snapping arbitrary world points back onto it.

The pieces split cleanly by responsibility:

| Piece | Role |
| --- | --- |
| `NavMeshSurface` | bakes level geometry into a walkable mesh (editor-time) |
| `NavMeshAgent` | a component that pathfinds and moves across the baked mesh |
| `NavMesh.SamplePosition` | snaps a world point to the nearest valid mesh point |

---
### NavMeshSurface — baking the walkable area

A NavMesh isn't computed every frame; it's **baked** once from static level
geometry into an asset, then queried cheaply at runtime. `NavMeshSurface` (from
the **AI Navigation** package, `com.unity.ai.navigation`) is the component that
does the baking. You add it to a GameObject, point it at the geometry to
consider, and it voxelizes that geometry, finds the flat-enough surfaces an agent
can stand on, and erodes the result inward by the agent's radius so the agent's
*body* never clips a wall.

Two settings decide *what* gets baked:

- **Collect Objects** — which geometry the surface gathers. `All` scans the whole
  scene; `Volume` limits to a box; `Children` only collects geometry parented
  under the surface's own GameObject. Putting the surface on a `Level` root and
  using `Children` is the tidy choice — the floor and walls live under `Level`,
  so the bake sees exactly them and nothing from unrelated scene objects.
- **Agent Type** — which agent the mesh is built for. The built-in **Humanoid**
  type (radius `0.5`, height `2`, max slope `45`, step `0.75`) defines the
  erosion and clearance. The agent that later walks the mesh must use the *same*
  agent type, or it won't recognize the surface as its own.

Walls don't need any special "non-walkable" marking. Because the surface erodes
the walkable area by the agent radius, a tall obstacle simply has no flat top the
agent can reach and pushes the floor mesh back around its base — the wall becomes
a hole in the NavMesh that paths route around.

**In code (`Level1` bake, via the editor / MCP):**

```csharp
var surface = level.AddComponent<NavMeshSurface>();
surface.agentTypeID = NavMesh.GetSettingsByIndex(0).agentTypeID;
surface.collectObjects = CollectObjects.Children;
surface.BuildNavMesh();
```

- `agentTypeID` is set from settings index `0`, the default **Humanoid** profile, so the mesh matches the agent that will use it.
- `collectObjects = Children` restricts the bake to geometry under this GameObject — the floor and walls parented to `Level`.
- `BuildNavMesh()` runs the bake and assigns the result to `surface.navMeshData`; persisting that data as an `.asset` is what lets it survive a scene reload.

---
### Voxel size and carve aliasing

The bake works by **voxelizing** geometry — rasterizing it into a 3D grid of
cells, then rebuilding a surface from the filled cells. The Humanoid default
voxel size is `~0.167` units (roughly agent radius / 3). That resolution is the
hidden cause of a subtle artifact: the walkable border around an obstacle can
look **uneven along its length**, hugging tighter in some spots than others.

The reason is quantization. The carved border sits one agent-radius back from the
wall, but that distance gets snapped to the nearest voxel cell. When a long wall's
edges fall on an awkward sub-voxel phase, the snapped border steps between, say,
`0.30` and `0.50` units of clearance at different points along the wall — a
staircase, not a clean line. A shorter or differently-positioned wall can happen
to land on a uniform phase and look perfectly even, which is why two identical
walls can carve differently.

The fix is resolution: override the voxel size to something finer (halving it to
`~0.083` roughly halves the stepping). The trade is bake time and memory, which
quadruple-ish as you halve the cell size — cheap for a small level, not free for
a huge one.

| | Default voxel (`0.167`) | Finer voxel (`0.083`) |
| --- | --- | --- |
| Carve evenness | visible stepping on long walls | sub-voxel, visually even |
| Bake cost | low | higher (≈4× cells per area) |
| When to use | prototyping, forgiving layouts | tight corridors, visible borders |

---
### NavMeshAgent — moving across the mesh

`NavMeshAgent` is the component that actually pathfinds. You give it a
destination and it computes a path across the baked mesh and steers the transform
along it, handling corners and avoidance for you. The tuning fields control how
that movement *feels*:

| Field | What it controls |
| --- | --- |
| `speed` | top movement speed along the path |
| `angularSpeed` | how fast it rotates to face the next corner (deg/s) |
| `acceleration` | how quickly it reaches `speed` |
| `stoppingDistance` | how far from the destination it halts |

A high `angularSpeed` (e.g. `999`) makes the agent pivot near-instantly instead
of arcing, which reads as crisp, responsive control for a top-down mover; a low
`stoppingDistance` (`0.1`) makes it stop almost exactly on the target.

Exposing `speed` through a property rather than letting other code touch the
agent directly keeps a single seam a power-up can modify later without knowing
the agent exists.

**In code (`PlayerMovement.cs`):**

```csharp
public float Speed
{
    get => _agent.speed;
    set => _agent.speed = value;
}
```

- The getter/setter proxy straight through to `NavMeshAgent.speed`, so a buff can read and restore the base value through one public surface.

One editor gotcha: `NavMeshAgent.isOnNavMesh` reads `false` in edit mode because
the agent doesn't simulate until Play. An agent placed over valid mesh will snap
onto it on the first frame of Play — a `false` reading at edit time is not a
setup error.

---
### NavMesh.SamplePosition — snapping a point to the mesh

A raw world point (say, where a ray hit the floor) isn't guaranteed to be exactly
on the NavMesh — it might be a hair off the edge, or just inside a carved
region. `NavMesh.SamplePosition` searches outward from a point up to a max
distance and returns the nearest point that *is* on the mesh. Gating
`SetDestination` on a successful sample means the agent is only ever sent to a
reachable spot, never told to walk to an invalid one.

**In code (`PlayerMovement.cs`):**

```csharp
if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 1f, NavMesh.AllAreas))
{
    _agent.SetDestination(navHit.position);
}
```

- `hit.point` is the raw target; `SamplePosition` looks within `1f` units of it for valid mesh.
- `navHit.position` is the snapped, guaranteed-on-mesh point; `SetDestination` only runs when the sample succeeds, so an off-mesh click is silently ignored.
- `NavMesh.AllAreas` accepts any area type as valid; a mask here would restrict to specific area types.

The input side of this — turning a click into `hit.point` — is covered in
[[raycasting-and-layermasks]] and [[input-system]].
