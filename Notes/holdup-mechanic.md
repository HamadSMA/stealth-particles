## Guard Holdup & Death

The player's one offensive move is the *holdup*: sneak up on a guard and take it
out of the level. It only makes sense from behind and up close — walking up to a
guard's face and tapping it should do nothing (that's how you get *seen*, not how
you win). So the holdup is two cooperating pieces: an **input path** that recognises
"the player deliberately tapped *this guard*", and a **rule** on the guard that
decides whether the tap is a legal takedown. The payoff — particle burst and the
guard vanishing — rides on the existing state machine.

---
### Tap to hold up, hold to move — two input paths that must not blur

Movement is a *hold*: while the pointer is down, the player streams a destination at
the ground every frame ([[input-system]] covers `isPressed` vs `wasPressedThisFrame`).
A holdup is a *discrete tap*: one press, on a guard. If both read the same continuous
hold, dragging the finger across a guard mid-move would trigger a takedown — and worse,
it would re-fire every frame the finger stayed on the guard. So the holdup is gated on
`wasPressedThisFrame` (a fresh press), and it's checked *before* movement so a guard-tap
is consumed instead of also issuing a move command.

The two raycasts are also separated by `LayerMask`: the holdup ray only looks at
`guardMask`, the movement ray only at `walkableMask` ([[raycasting-and-layermasks]]).
A guard sits on its own **Guard** layer, so the holdup ray can pick guards out cleanly
and the movement ray sees straight past them to the floor.

**In code (`PlayerMovement.cs`):**

```csharp
private void Update()
{
    if (TryGetPointerPress(out Vector2 pressPosition) && TryHoldupAt(pressPosition))
    {
        return;
    }

    if (TryGetPointerHold(out Vector2 pointerPosition))
    {
        SteerToward(pointerPosition);
    }
}

private bool TryHoldupAt(Vector2 pointerPosition)
{
    Ray ray = _camera.ScreenPointToRay(pointerPosition);
    if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, guardMask.value))
    {
        return false;
    }

    GuardController guard = hit.collider.GetComponent<GuardController>();
    if (guard != null)
    {
        guard.TryHoldup(transform.position);
    }

    return true;
}
```

- `TryGetPointerPress` reports a *fresh* press only; a held finger returns false after the first frame, so the holdup can't auto-repeat.
- The early `return` when `TryHoldupAt` succeeds is what keeps the paths from colliding — a tap that landed on a guard never falls through to `SteerToward`, so the player doesn't also walk to that spot.
- `TryHoldupAt` returns `true` whenever the ray *hit a guard at all*, success or not, so even an out-of-position tap consumes the press instead of leaking into movement. Whether it's a legal takedown is the guard's call, not the input's.

---
### The rule: range and a rear arc

`GuardController.TryHoldup` is the gatekeeper. A takedown is legal only when the
attacker is **close** and **behind** — the same two numbers a guard already carries in
its `GuardConfig` (`holdupRange`, `holdupAngle`). The geometry mirrors the vision-cone
test ([[vision-cone]]) but inverted: vision asks "is the player in front of me?", the
holdup asks "is the attacker in my blind spot behind me?".

**In code (`GuardController.cs`):**

```csharp
public bool TryHoldup(Vector3 fromPosition)
{
    if (isHeldUp)
    {
        return false;
    }

    Vector3 toSource = fromPosition - transform.position;
    toSource.y = 0f;

    if (toSource.sqrMagnitude > config.holdupRange * config.holdupRange)
    {
        return false;
    }

    Vector3 forward = transform.forward;
    forward.y = 0f;

    if (Vector3.Angle(forward, toSource) < 180f - config.holdupAngle * 0.5f)
    {
        return false;
    }

    isHeldUp = true;
    TransitionTo(new DeadState(this));
    return true;
}
```

- `isHeldUp` latches on the first success so a takedown can't be triggered twice during the death fade — the same guard-against-re-firing idea as `alreadyDetected` on the vision path.
- The range gate compares **squared** magnitudes (`sqrMagnitude` vs `holdupRange²`) to skip a square root; everything is flattened onto XZ first so height doesn't count.
- The angle gate is the rear-arc test. `Vector3.Angle(forward, toSource)` is 0° when the attacker is dead ahead and 180° when directly behind. Requiring it to be *at least* `180 − holdupAngle/2` carves out a wedge of width `holdupAngle` centred on the guard's back — with the default `holdupAngle = 120`, the rear 120°.
- On success the guard latches and hands itself to `DeadState`; the `bool` return tells the caller it worked, though the input path ignores it (success or failure is read from what happens on screen, not a flag).

---
### Death as a terminal state, plus a one-shot particle burst

The takedown's outcome reuses the guard FSM ([[design-patterns]]): `DeadState` is a
**terminal** state whose `Enter` does everything and whose `Tick`/`Exit` are empty —
there is no state after death. It stops the agent, switches off the vision sensor so a
dying guard can't still catch you, fires the burst, then schedules its own destruction
after a fade so the particles have a moment to play.

**In code (`DeadState.cs`):**

```csharp
public void Enter()
{
    NavMeshAgent agent = guard.Agent;
    agent.isStopped = true;
    agent.velocity = Vector3.zero;

    if (guard.Vision != null)
    {
        guard.Vision.enabled = false;
    }

    guard.PlayHoldupVFX();

    Object.Destroy(guard.gameObject, guard.Config.fadeDuration);
}
```

- Disabling `guard.Vision` (a `VisionCone` exposed by `GuardController`) kills both the detection test and the drawn cone in one line.
- `Object.Destroy(go, fadeDuration)` is the delayed-destroy overload: the GameObject lives `fadeDuration` seconds more, long enough for the burst to finish, then disappears — no coroutine or timer needed.

The burst itself is a prefab the guard spawns and forgets:

**In code (`GuardController.cs`):**

```csharp
public void PlayHoldupVFX()
{
    if (holdupBurstPrefab != null)
    {
        Instantiate(holdupBurstPrefab, transform.position, Quaternion.identity);
    }
}
```

- The `HoldupBurstFX` prefab is a one-shot Particle System: Looping off, a single 40-particle Burst at time 0, **Play On Awake** on, and **Stop Action = Destroy**. So it plays the instant it's instantiated and deletes its own GameObject when the burst finishes — `PlayHoldupVFX` never has to track or clean up what it spawned.
- The guard destroys itself on `fadeDuration`; the particle instance is independent and outlives the guard by however long its own lifetime runs.
