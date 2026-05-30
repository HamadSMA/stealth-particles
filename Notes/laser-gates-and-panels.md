## Laser Gates & Disable Panels

A stealth puzzle needs obstacles that block a route but can be *turned off* — otherwise
every level is just "find the gap." The laser is that obstacle: a beam across the path
that fails the run the instant the player touches it while it's live. The disable panel
is its off-switch, placed deliberately off to one side so reaching it costs a detour.
Together they're a **hazard + remote switch** pair: the laser knows nothing about how it
gets disabled, the panel knows nothing about what a laser does — the panel just holds a
reference to one laser and calls `SetActive(false)`. That decoupling is what lets a level
be built out of arbitrarily many pairs.

Both pieces lean on patterns already in the project: the laser routes into the same
`OnPlayerDetected → Fail` funnel as the vision cone and physical contact
([[level-flow]], [[vision-cone]]), and the panel is targeted by the exact tap-raycast
trick the holdup uses ([[holdup-mechanic]]).

---
### The laser: a toggleable hazard on the shared Fail funnel

The laser doesn't need its own failure concept — being lasered *is* being detected, so it
raises `GameEvents.RaisePlayerDetected()` and lets the GameManager turn that into `Fail`,
same as a guard seeing you. What's laser-specific is three things: it only bites while
**active** and while the game is **Playing**, and it must fire **once** (a beam the player
is standing in would otherwise re-raise the event every physics step).

Detection is an `OverlapBox` swept each `FixedUpdate` rather than a trigger collider. A
trigger would depend on the player carrying a Rigidbody, on the collision layer matrix
lining up, and on `OnTriggerEnter` semantics; the box is self-contained — it owns its own
dimensions, runs on physics ticks, and just tag-checks whatever it overlaps.

**In code (`Laser.cs`):**

```csharp
private void FixedUpdate()
{
    if (!isActive || !isPlaying || hasDetected)
    {
        return;
    }

    int count = Physics.OverlapBoxNonAlloc(
        transform.position,
        detectionHalfExtents,
        overlapResults,
        transform.rotation
    );

    for (int i = 0; i < count; i++)
    {
        if (overlapResults[i].CompareTag("Player"))
        {
            hasDetected = true;
            GameEvents.RaisePlayerDetected();
            return;
        }
    }
}
```

- The guard clause folds all three conditions into one early-out: an inactive laser, the
  briefing/results screens, or an already-fired beam all skip the work entirely.
- `OverlapBoxNonAlloc` writes hits into a reused `overlapResults` buffer instead of
  allocating an array every physics frame — the box runs forever, so the garbage matters.
- `transform.position` / `transform.rotation` are the box's centre and orientation, so the
  detection volume rides the laser object; `detectionHalfExtents` is authored to span the
  beam (long on X, thin on Z, tall enough to catch the player capsule).
- `hasDetected` is the once-latch — the same idea as `alreadyDetected` on the vision path
  ([[vision-cone]]) — so a player frozen inside the beam triggers exactly one Fail.

Turning the laser off (or on) is one public method; the visuals follow the flag. The beam
is drawn as several thin emissive line meshes (a `Renderer[]`), and the "disabled" look is
simply *gone* — the renderers switch off rather than swapping to a dim material.

**In code (`Laser.cs`):**

```csharp
public void SetActive(bool value)
{
    isActive = value;
    hasDetected = false;
    ApplyVisualState();
}

private void ApplyVisualState()
{
    if (beamRenderers == null)
    {
        return;
    }

    for (int i = 0; i < beamRenderers.Length; i++)
    {
        if (beamRenderers[i] == null)
        {
            continue;
        }

        beamRenderers[i].enabled = isActive;

        if (isActive && activeMaterial != null)
        {
            beamRenderers[i].sharedMaterial = activeMaterial;
        }
    }
}
```

- `SetActive` clears `hasDetected` as well as flipping `isActive`, so a laser that is
  switched back on later starts fresh and can catch the player again.
- `ApplyVisualState` is called from both `SetActive` and `Start`, so the beam's appearance
  is always derived from the flag — there's no second source of truth to keep in sync.
- Driving `Renderer.enabled` per-line keeps detection and visuals orthogonal: the
  `OverlapBox` doesn't care whether the beams are drawn, it only reads `isActive`.

`isActive` and `isPlaying` are tracked the standard way — `isPlaying` flips on
`GameEvents.OnGameStateChanged`, subscribed in `OnEnable` and dropped in `OnDisable`
([[unity-classes-and-interfaces]]).

---
### The panel: a third tap-target layer, plus the link and the rule

The panel is hit the same way a guard is held up — a fresh-press raycast against a
dedicated `LayerMask` ([[raycasting-and-layermasks]], [[input-system]]). The panel sits on
its own **Panel** layer, so adding it to `PlayerMovement` is just a third masked ray after
the guard ray, checked before movement so a tap that lands on a panel is *consumed* instead
of also issuing a walk command — the exact funnel the holdup established ([[holdup-mechanic]]).

**In code (`PlayerMovement.cs`):**

```csharp
if (TryGetPointerPress(out Vector2 pressPosition))
{
    if (TryHoldupAt(pressPosition))
    {
        return;
    }

    if (TryDisablePanelAt(pressPosition))
    {
        return;
    }
}
```

```csharp
private bool TryDisablePanelAt(Vector2 pointerPosition)
{
    Ray ray = _camera.ScreenPointToRay(pointerPosition);
    if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, panelMask.value))
    {
        return false;
    }

    Panel panel = hit.collider.GetComponent<Panel>();
    if (panel != null)
    {
        panel.TryDisable(transform.position);
    }

    return true;
}
```

- The press handler is an ordered chain of "try to consume this tap": guard first, panel
  second, fall through to movement. Each returns `true` when it *hit its layer at all*, so
  a tap on a panel never leaks into a move even if the panel refuses to fire.
- `TryDisablePanelAt` mirrors `TryHoldupAt` line-for-line but on `panelMask` — separate
  layers are what let three different tap meanings (guard / panel / floor) coexist on one
  press without ambiguity.
- The player passes **its own position** into `TryDisable`; the range check is the panel's
  job, not the input's — input only decides *which* panel was tapped.

The panel itself enforces three things before it acts: the game is **Playing**, the player
is **in range**, and it hasn't been **used** yet. On success it disables its one linked
laser and swaps its own material from armed (magenta) to used (cyan) so the board reads its
state at a glance.

**In code (`Panel.cs`):**

```csharp
public bool TryDisable(Vector3 playerWorldPos)
{
    if (!isPlaying || isUsed)
    {
        return false;
    }

    if (Vector3.Distance(playerWorldPos, transform.position) > activationRange)
    {
        return false;
    }

    if (linkedLaser != null)
    {
        linkedLaser.SetActive(false);
    }

    isUsed = true;

    if (panelRenderer != null && usedMaterial != null)
    {
        panelRenderer.sharedMaterial = usedMaterial;
    }

    return true;
}
```

- `isUsed` is a one-shot latch like the laser's `hasDetected` and the guard's `isHeldUp` —
  a panel is consumed on first use and can't re-toggle its laser.
- The range gate is a plain `Vector3.Distance` against a serialized `activationRange`; this
  is what forces the *detour* — the panel is placed off the direct line, so the player has
  to leave the route to get inside range.
- `linkedLaser` is the whole coupling: one serialized reference, set per instance in the
  scene. The panel calls `SetActive(false)` and is done — it never reads or writes the
  laser's internals, which is why duplicating a laser+panel pair and re-pointing the link is
  all it takes to add another gate.

---
### Building a level out of pairs

Because the switch and the hazard only meet through that one reference, a level is just a
bag of independent pairs: drop a laser across a corridor, drop a panel to its side, link
them. Level 2 (*LaserInjection*) is built this way — two laser gates, each disabled by its
own panel placed across the room, threaded through the wall weave so the panel detours and
the guard's cone ([[vision-cone]]) all compete for the same time budget ([[level-flow]]).
Nothing in `Laser` or `Panel` knows it's the first pair or the tenth.
