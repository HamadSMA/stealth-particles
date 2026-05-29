## Unity Input System (new)

> **Change (2026-05-29):** player control is now **hold-to-move** — `TryGetPointerHold` reading `isPressed`, which steers the agent toward the pointer every frame it's held and clears the path on release. The tap-to-move variant described below (`TryGetPointerPress` / `wasPressedThisFrame`) is an earlier iteration, kept for reference.

Unity ships two input stacks. The **old** Input Manager (`Input.GetMouseButtonDown`,
`Input.touchCount`) reads global static state polled each frame. The **new** Input
System package (`com.unity.inputsystem`) replaces it with device objects you query
explicitly (`Mouse.current`, `Touchscreen.current`) and an action-mapping layer on
top. This project runs the new system, so all input code goes through device
objects.

Which stack is live is a project setting, not a code choice — and code written for
the wrong one fails at runtime (the old `Input` API throws once the new system is
active). So the first thing to know before writing any input is *which is on*.

---
### Knowing which system is active

The active stack is `activeInputHandler` in `ProjectSettings/ProjectSettings.asset`
(set via **Edit ▸ Project Settings ▸ Player ▸ Active Input Handling`):

| Value | Meaning |
| --- | --- |
| `0` | Old Input Manager only |
| `1` | New Input System only |
| `2` | Both |

This project reads `activeInputHandler: 1` with `com.unity.inputsystem` in the
package manifest — new system only. Old `Input.GetMouseButtonDown(0)` calls would
throw here; everything must use the device API below.

---
### Reading the pointer from devices

In the new system you don't ask a global "was the mouse clicked?" — you ask a
specific device. `Mouse.current` and `Touchscreen.current` are the current
devices (or `null` if none is connected, which is why each needs a null check
before use). Each exposes controls you read three ways:

- **`wasPressedThisFrame`** — an edge trigger, true only on the frame a button or
  touch went down. This is the equivalent of the old `GetMouseButtonDown`, and
  what a one-shot "tap to act" control needs: it fires once per tap, sets a
  destination, and the agent walks there on its own without further input.
- **`isPressed`** — a level read, true on *every* frame the button or touch is
  held down. This is what a hold-to-steer control would use instead: it lets
  `Update` react continuously for as long as the finger is down, not just on the
  first frame.
- **`ReadValue()`** — the current analog value of a control, e.g. the pointer's
  screen position as a `Vector2`.

Handling mouse and touch together is just checking both devices: mouse for the
editor and desktop builds, touchscreen for the phone. The first one pressed this
frame wins.

**In code (`PlayerMovement.cs`):**

```csharp
private static bool TryGetPointerPress(out Vector2 position)
{
    if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
    {
        position = Mouse.current.position.ReadValue();
        return true;
    }

    if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
    {
        position = Touchscreen.current.primaryTouch.position.ReadValue();
        return true;
    }

    position = default;
    return false;
}
```

- Each device is null-checked first, so the code is safe on a phone with no mouse or a desktop with no touchscreen.
- `leftButton.wasPressedThisFrame` / `primaryTouch.press.wasPressedThisFrame` are edge triggers — true only on the frame the input goes down, so each tap sets one destination and the NavMeshAgent paths there on its own; holding the button does nothing extra.
- `position.ReadValue()` returns the pointer's screen-space `Vector2` at the moment of the tap, which the camera ray turns into a world point to path toward (see [[raycasting-and-layermasks]]).
- Returning a `bool` with the position as an `out` lets the caller bail in one line on the frames nothing was tapped.

This reads devices directly rather than going through the Input System's Action
asset layer. Actions are worth it once you need rebindable controls or multiple
control schemes; for a single tap-to-move control a direct device read is less
ceremony.
