# Unity Design Patterns

Patterns Unity developers reach for often, what each one is good for, and what it
costs. The table is the map; the sections after it drill into the patterns this
project actually uses, grounded in the code that implements them.

| Pattern | Idea | Use it for | Tradeoff |
| --- | --- | --- | --- |
| Singleton | one globally accessible instance (`Instance`) | a single always-present manager everything needs — audio, save system, the game loop | easy global access, but hidden dependencies, hard to test, lifetime/order pitfalls |
| Observer / Event hub | publishers raise events, subscribers react, neither knows the other | systems that react to game moments without coupling — UI on score change, achievements, logging | decouples systems; flow is harder to trace and you must manage subscribe/unsubscribe |
| State machine | explicit states plus the transitions allowed between them | anything with distinct modes — game flow, enemy AI (patrol/chase/search), UI screens | clear and guards illegal transitions; grows verbose as states multiply |
| ScriptableObject architecture | hold data (and sometimes events) as assets | designer-tunable config, shared data, per-level definitions, event channels | designer-editable, decoupled, no recompiles; can be overkill and lead to asset sprawl |
| Component composition | build behavior from small MonoBehaviours rather than deep inheritance | entities assembled from reusable parts — Health + Movement + Damage | flexible and reusable; many tiny components can be hard to follow |
| Object pooling | reuse a pool of objects instead of Instantiate/Destroy | high-churn spawns — bullets, particles, VFX, waves of enemies | avoids GC spikes for spawn-heavy objects; adds complexity, must reset state |

---
## Observer / Event hub (used here)

A direct method call couples the caller to the callee: the caller has to hold a
reference to whatever it notifies, and adding a second listener means editing the
caller. The **observer** pattern inverts that — a publisher raises an event and
doesn't know who listens, subscribers register themselves, and the two sides
compile independently. Here it keeps scoring, UI, audio, and logging from
reaching into each other.

C# `event`s can only be invoked from inside the type that declares them, so a
static hub exposes `Raise...()` methods as its public API. That keeps invocation
encapsulated and puts the null-check (an event with no subscribers is `null`) in
one place.

`GameEvents` is this pattern — a plain static class, deliberately not a singleton
or MonoBehaviour, so any system can publish or subscribe without holding a
reference to a manager:

```csharp
public static event Action<GameState> OnGameStateChanged;   // listeners attach with += here
public static event Action OnPlayerDetected;                // parameterless events use Action
public static event Action<float> OnTimerUpdated;           // payload type goes in Action<T>

public static void RaiseGameStateChanged(GameState state)   // the publish API callers use
{
    OnGameStateChanged?.Invoke(state);                      // ?. no-ops when nobody is subscribed
}
```

A subscriber pairs registration with `MonoBehaviour`'s enable/disable messages so
every `+=` has exactly one matching `-=` (see [[unity-classes-and-interfaces]]).
`EventLogger` listens to events purely to print them:

```csharp
private void OnEnable()
{
    GameEvents.OnGameStateChanged += HandleGameStateChanged;   // subscribe when active
    GameEvents.OnPlayerDetected   += HandlePlayerDetected;
}

private void OnDisable()
{
    GameEvents.OnGameStateChanged -= HandleGameStateChanged;   // unsubscribe when inactive
    GameEvents.OnPlayerDetected   -= HandlePlayerDetected;
}
```

---
## State machine (used here)

Some objects are only ever in one of a fixed set of modes, and only certain moves
between modes are legal — the game can go Briefing → Playing, but never
Fail → Playing. A **state machine** makes those modes an explicit `enum` and
funnels every change through one guarded method, so an illegal move is rejected
loudly instead of silently leaving the object in a nonsense state.

`GameState` is the set of modes:

```csharp
public enum GameState { Briefing, Playing, Success, Fail }
```

`GameManager` owns the current state and routes every change through
`TransitionTo`, which rejects no-op and illegal moves before committing the
change and broadcasting it:

```csharp
private void TransitionTo(GameState newState)
{
    if (newState == CurrentState) return;                       // ignore redundant transitions

    if (!IsValidTransition(CurrentState, newState))             // guard illegal moves
    {
        Debug.LogWarning($"[GameManager] Ignored invalid transition {CurrentState} -> {newState}.");
        return;
    }

    CurrentState = newState;                                    // commit the new state
    GameEvents.RaiseGameStateChanged(newState);                 // tell the rest of the game
}

private static bool IsValidTransition(GameState from, GameState to)
{
    switch (from)
    {
        case GameState.Briefing: return to == GameState.Playing;                         // briefing only starts play
        case GameState.Playing:  return to == GameState.Success || to == GameState.Fail; // play ends one of two ways
        default:                 return false;                                           // Success/Fail are terminal
    }
}
```

The two patterns compose: the state machine decides *what* changed, then the
event hub broadcasts it. `GameManager` even drives its own transitions off events
— it subscribes to `OnGoalReached` (→ `Success`) and `OnPlayerDetected`
(→ `Fail`), so gameplay code never calls the manager directly.
