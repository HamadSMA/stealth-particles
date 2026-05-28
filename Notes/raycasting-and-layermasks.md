## Raycasting & LayerMasks

A screen tap is a 2D point; the world is 3D. **Raycasting** bridges them: you
shoot a ray from the camera through the tapped pixel into the scene and ask the
physics system what it hits. **LayerMasks** then let you filter *what counts* —
so a click only registers on the ground, not on an enemy or a wall. Together
they're the backbone of click-to-move, object selection, and shooting.

---
### Camera ray + Physics.Raycast

`Camera.ScreenPointToRay` converts a screen position into a world-space `Ray`
(an origin and a direction) accounting for the camera's perspective.
`Physics.Raycast` then traces that ray and reports the **first collider** it
hits, filling a `RaycastHit` with the contact point, the collider, the surface
normal, and distance.

The "first collider" part matters: a raycast returns the *closest* hit along the
ray, so nearer geometry naturally blocks what's behind it — unless you tell the
ray to ignore that geometry (see the gotcha below).

**In code (`PlayerMovement.cs`):**

```csharp
Ray ray = _camera.ScreenPointToRay(pointerPosition);
if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
{
    return;
}
```

- `pointerPosition` is the screen-space tap from [[input-system]]; `ScreenPointToRay` turns it into a world ray.
- `out RaycastHit hit` receives the closest hit; `hit.point` is the world position, `hit.collider` the thing struck.
- `Mathf.Infinity` is the max ray distance — unbounded here. With no layer-mask argument the ray hits *every* collider on Unity's default raycast layers, so the nearest object always wins.

---
### Layers and the LayerMask bitmask

Every GameObject sits on one of 32 **layers**. A `LayerMask` is a 32-bit integer
where each set bit means "include this layer" — it's a filter, not a single
layer. You create a layer in **Edit ▸ Project Settings ▸ Tags and Layers**,
assign objects to it, and expose a `LayerMask` field so the filter is editable in
the Inspector.

Because a mask is a bitmask, testing whether a specific layer is in it is a bit
operation: shift `1` left by the layer's index to get that layer's bit, then AND
it against the mask. Non-zero means "included."

**In code (`PlayerMovement.cs`):**

```csharp
[SerializeField]
private LayerMask walkableMask;

if ((walkableMask.value & (1 << hit.collider.gameObject.layer)) == 0)
{
    return;
}
```

- `walkableMask` is set in the Inspector to the **Walkable** layer; the floor is assigned to that layer so only it qualifies as a move target.
- `hit.collider.gameObject.layer` is the struck object's layer *index* (e.g. `8`); `1 << 8` builds that layer's bit.
- `& walkableMask.value` keeps only the bits both share; `== 0` means the hit object's layer isn't in the mask, so the click is rejected.

---
### Gotcha: a masked raycast tunnels through excluded colliders

The intuitive way to filter is to pass the mask straight to `Physics.Raycast` as
its layer argument: "only raycast against walkable things." It compiles, it looks
right, and it introduces a bug — **clicking a wall moves the character anyway**.

The reason is what the mask argument actually does: it makes the ray *ignore* any
collider not in the mask, rather than *stop* at it. With walls excluded, a click
aimed at a wall doesn't hit the wall — the ray passes straight through it and
lands on the floor on the far side (the floor extends underneath and beyond the
walls). The raycast succeeds, returns a valid floor point behind the wall, and
the agent dutifully walks there.

The fix is to separate *blocking* from *validating*. Let the ray hit everything
so walls block it, then check the layer of whatever it actually hit:

| Approach | Behavior | Result |
| --- | --- | --- |
| `Raycast(ray, out hit, dist, walkableMask)` | ray skips walls, hits floor behind them | wall clicks tunnel through — bug |
| `Raycast(ray, out hit)` then test `hit`'s layer | wall blocks the ray; layer check rejects it | wall clicks ignored — correct |

So the unfiltered raycast plus the manual bitmask test from the previous section
is the deliberate fix, not redundant work: the raycast decides *what the ray
reaches first*, and the layer test decides *whether that thing is a valid
target*. Collapsing the two into the mask argument throws away the blocking,
which is exactly what walls need.
