## Vision Cone & Line-of-Sight Detection

A stealth guard has to "see" the player the way a person would: only what's in
*front* of it, only out to a certain *distance*, and only if nothing is *in the
way*. A plain trigger volume can't express the directional or occlusion parts — it
fires the moment anything overlaps, regardless of facing or walls. So detection here
is a per-frame math test against the player's position, paired with a cone mesh drawn
on the floor so the player can read exactly how far the threat reaches.

The work splits cleanly: `VisionCone` is the *sensor* (can I see this point? + draw
the cone), and `GuardController` is the *decider* (act on what the sensor reports).

---
### The three gates of a "can I see it?" test

`ContainsPoint` runs the candidate point through three checks, cheapest first, and
bails at the first failure — no point raycasting if the target is behind you or out
of range. Everything is projected onto the **XZ plane** (Y zeroed) because the level
is flat and top-down; the guard's height relative to the player is irrelevant.

1. **Range** — planar distance to the point vs `config.visionRange`.
2. **Angle** — `Vector3.Angle` between the eye's forward and the direction to the point vs `visionAngle * 0.5` (the cone is symmetric, so half the spread on each side of forward).
3. **Occlusion** — a ray from the eye toward the point on `wallMask`, capped at the point's distance. A wall hit before reaching the point means the line of sight is blocked.

**In code (`VisionCone.cs`):**

```csharp
public bool ContainsPoint(Vector3 worldPoint, out bool blockedByWall)
{
    blockedByWall = false;

    Vector3 origin = eyeOrigin.position;
    Vector3 toPoint = worldPoint - origin;
    toPoint.y = 0f;
    float distance = toPoint.magnitude;

    if (distance > config.visionRange)
    {
        return false;
    }

    Vector3 forward = eyeOrigin.forward;
    forward.y = 0f;

    if (Vector3.Angle(forward, toPoint) > config.visionAngle * 0.5f)
    {
        return false;
    }

    if (Physics.Raycast(origin, toPoint.normalized, distance, wallMask))
    {
        blockedByWall = true;
        return false;
    }

    return true;
}
```

- `toPoint.y = 0f` flattens the vector so range and angle are measured on the ground plane only.
- The range gate returns early with `blockedByWall = false` — it failed by distance, not a wall.
- `Vector3.Angle` normalizes its inputs internally, so passing the raw `forward` and `toPoint` is fine; comparing to the *half*-angle is what makes the full cone `visionAngle` wide.
- The raycast distance is capped at `distance` so a wall *behind* the player doesn't count as blocking; `blockedByWall` is reported via `out` so callers can tell "didn't see it" from "a wall saved them".

---
### Drawing the cone (a mesh rebuilt every frame)

The visual is regenerated each `Update` so it bends around walls in real time. A fan
of `meshResolution + 1` rays sweeps from `-visionAngle/2` to `+visionAngle/2` around
the eye's forward; each ray raycasts walls up to `visionRange`, and its endpoint is
the **hit point** (so the cone is visibly cut by cover) or the full range if it hits
nothing. Those endpoints plus the eye as apex form a triangle fan.

**In code (`VisionCone.cs`):**

```csharp
for (int i = 0; i <= meshResolution; i++)
{
    float angle = -halfAngle + config.visionAngle * (i / (float)meshResolution);
    Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * forward;

    Vector3 endpoint;
    if (Physics.Raycast(origin, dir, out RaycastHit hit, config.visionRange, wallMask))
    {
        endpoint = hit.point;
    }
    else
    {
        endpoint = origin + dir * config.visionRange;
    }

    Vector3 local = coneTransform.InverseTransformPoint(endpoint);
    local.y = 0f;
    vertices[i + 1] = local;
}
```

- `Quaternion.AngleAxis(angle, Vector3.up) * forward` rotates the forward vector around the vertical axis to aim each ray across the spread.
- The hit-or-full-range branch is what makes the rendered cone match the *sensed* cone — both stop at walls.
- `InverseTransformPoint` converts each world endpoint into the cone object's local space, and `local.y = 0f` flattens it so the wedge lies on the floor plane regardless of where the guard's pivot sits. The triangle winding is ordered so the face normal points up, which is what the top-down camera needs to see it.

---
### Layers carry the occlusion

The raycasts only care about walls, so walls live on a dedicated **Wall** layer and
`wallMask` is set to that layer alone. That keeps the rays from stopping on the floor,
the player, or the guard's own collider — they pass through everything *except* walls.
The bitmask mechanics behind a `LayerMask` are covered in
[[raycasting-and-layermasks]]. `playerMask` is serialized but unused for now; it's
reserved for a later detection refinement.

---
### Turning "I see you" into a game-over

Sensing is continuous; the *decision* happens once. While the game is in `Playing`,
`GuardController` tests the player against its cone every frame and, on the first
positive, raises the detection event a single time — a latch (`alreadyDetected`)
stops it re-firing while the state machine tears down.

**In code (`GuardController.cs`):**

```csharp
private void CheckDetection()
{
    if (!isPlaying || alreadyDetected)
    {
        return;
    }

    if (visionCone == null || playerTransform == null)
    {
        return;
    }

    if (visionCone.ContainsPoint(playerTransform.position, out _))
    {
        alreadyDetected = true;
        GameEvents.RaisePlayerDetected();
    }
}
```

- The `isPlaying` gate means guards don't detect during Briefing or after the game has ended; it's tracked off the same `OnGameStateChanged` subscription that kicks off patrol.
- `out _` discards the `blockedByWall` flag — the guard only needs the yes/no here.
- `RaisePlayerDetected()` is fire-and-forget through the event hub; `GameManager` is subscribed and transitions to `Fail`, which is how detection ends the run (see the observer + state machine sections in [[design-patterns]]).

While there's no real UI yet, a temporary hook in `EventLogger` makes detection
obvious: on `Fail` it logs `BUSTED` and sets `Time.timeScale = 0f` to freeze the
frame so the catch can be seen. That's debug scaffolding to be replaced by a proper
Fail screen later.
