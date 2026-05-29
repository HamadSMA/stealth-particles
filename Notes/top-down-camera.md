## Top-Down Follow Camera

A top-down action game needs a camera that hangs over the player, tracks it as it
walks, and never tilts or spins with the character. Parenting the camera to the
player would do the tracking for free, but it also drags the player's Y-rotation
onto the camera — the moment the NavMeshAgent turns the capsule to face its travel
direction, the whole view would swing with it. So the camera is its own object,
driven by a small script that copies *only* the target's position and keeps its own
fixed orientation. The script also exposes a per-level hook so a `LevelConfig` can
push a different height and offset for each level without editing the scene.

---
### LateUpdate, not Update

A follow camera reads the target's position *after* the target has finished moving
for the frame. The NavMeshAgent updates the player in its own step during `Update`;
if the camera also moved in `Update`, the order of the two would be undefined and
the camera could chase a stale position, producing a one-frame lag that reads as
jitter. `LateUpdate` runs after every `Update` has completed, so the player's
position is final before the camera samples it.

| Hook | When it runs | Fit for a follow camera |
| --- | --- | --- |
| `Update` | Per frame, order vs. other scripts undefined | Risk of sampling the target before it has moved |
| `LateUpdate` | Per frame, after all `Update` calls | Target's position is settled — correct place to follow |

---
### Smoothing with Vector3.SmoothDamp

Snapping the camera straight onto the target each frame is rigid and amplifies any
hitch in the player's motion. The camera instead eases toward the desired spot.
`Vector3.SmoothDamp` is built for exactly this: it's a critically damped spring that
glides to the goal and settles without overshooting, and it adapts to a moving goal
rather than restarting each frame.

The catch is that it needs to remember how fast it was moving between frames, so it
takes a `ref` velocity the caller owns. That's the private `velocity` field — the
script hands the same one in every frame and `SmoothDamp` reads and rewrites it.

| Option | Pros | Cons |
| --- | --- | --- |
| Direct assign (`transform.position = desired`) | Zero lag, trivial | Rigid; every player hitch shows in the camera |
| `Vector3.Lerp` toward target | Simple smoothing | Framerate-dependent unless hand-tuned; never quite arrives |
| `Vector3.SmoothDamp` | Critically damped, settles cleanly, handles a moving goal | Must store and pass a velocity field |

---
### Keeping the camera upright

The whole point of the separate-object setup is that the camera's rotation is
*never* derived from the target. Position is copied; orientation is set from the
serialized `cameraAngle` and nothing else. At `90` the camera looks straight down
the world Y axis; lowering it tilts the view forward for a more three-quarter feel.
Because rotation is rebuilt every `LateUpdate` from `Quaternion.Euler(cameraAngle,
0, 0)`, the camera can't accumulate drift on the X or Z axes and can never roll or
tip — it only ever pitches by the one value you set.

This pairs with the player rig, where the NavMeshAgent is configured to keep the
capsule upright and yaw-only (see [[navmesh-navigation]]). The character turns to
face where it walks; the camera stays level and overhead regardless.

---
### Perspective vs. orthographic

Top-down games run with either projection, and the choice changes the feel. This
camera uses **Perspective** with a Field of View of 50, positioned overhead at a
height that frames the play area. Perspective keeps a slight sense of depth and lets
the tilt from `cameraAngle` read as a real lean over the scene; orthographic flattens
everything to a clean schematic look with no convergence.

| Projection | Pros | Cons |
| --- | --- | --- |
| Perspective | Depth cues, height/FOV give framing control, tilt looks natural | Objects shrink with distance — less exact for grid-precise layouts |
| Orthographic | No distortion, uniform scale, crisp top-down readout | Flat, no depth; tilting `cameraAngle` looks odd |

---
### Per-level config via ApplyLevelConfig

Different levels want the camera at different heights and horizontal offsets — a
tight room reads better up close, an open arena needs to pull back. Rather than bake
those into the prefab, `ApplyLevelConfig(float height, Vector3 offset)` lets a
`LevelConfig` drive them at load time. It sets the same `cameraHeight` and
`followOffset` the inspector exposes, so the editor values are just the default and a
level can override them without touching the scene.

---
**In code (`TopDownCamera.cs`):**

```csharp
[SerializeField] private Transform target;
[SerializeField] private float cameraHeight = 18f;
[SerializeField] private Vector3 followOffset = Vector3.zero;
[SerializeField] private float smoothTime = 0.25f;
[SerializeField] private float cameraAngle = 90f;

private Vector3 velocity;

private void LateUpdate()
{
    if (target == null)
    {
        return;
    }

    Vector3 desiredPosition = target.position + Vector3.up * cameraHeight + followOffset;
    transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
    transform.rotation = Quaternion.Euler(cameraAngle, 0f, 0f);
}

public void ApplyLevelConfig(float height, Vector3 offset)
{
    cameraHeight = height;
    followOffset = offset;
}
```

- `target` is the Transform to follow (the Player); the `LateUpdate` early-out on `target == null` keeps the camera inert until something is assigned, so an unconfigured scene doesn't throw.
- `cameraHeight` lifts the desired position straight up the world Y axis; `followOffset` shifts it horizontally to lead or trail the target.
- `desiredPosition` is the goal each frame — target position plus the height and offset — and `Vector3.SmoothDamp` eases the camera toward it, reading and rewriting `velocity` so motion carries across frames.
- `transform.rotation` is set fresh from `cameraAngle` every frame and is fully independent of the target, which is what keeps the view fixed and upright.
- `ApplyLevelConfig` overwrites `cameraHeight` and `followOffset` so a per-level config can reframe the shot without scene edits.
