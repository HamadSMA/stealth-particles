## Unity Design Patterns

Patterns Unity developers reach for often, what each one is for, and what it
costs. The table is a quick map; the sections after it dig into the two patterns
this project actually uses, grounded in the code that implements them.

| Pattern | Idea | Pros | Cons |
| --- | --- | --- | --- |
| Singleton | one globally accessible instance (`Instance`) | trivial global access from anywhere | hidden dependencies, hard to test, lifetime/order pitfalls |
| Observer / Event hub | publishers raise events, subscribers react, neither knows the other | decouples systems; add listeners without touching publishers | flow is harder to trace; every subscribe needs a matching unsubscribe |
| State machine | explicit states plus the transitions allowed between them | illegal moves rejected in one place; flow is obvious | grows verbose as states multiply |
| ScriptableObject architecture | hold data (and sometimes events) as assets | designer-editable, decoupled, no recompiles | overkill for trivial data; leads to asset sprawl if overused |
| Component composition | build behavior from small MonoBehaviours rather than deep inheritance | flexible, reusable, mix-and-match | many tiny components can be hard to follow |
| Object pooling | reuse a pool of objects instead of Instantiate/Destroy | avoids GC spikes for spawn-heavy objects | adds complexity; you must reset object state on reuse |

---
### Observer / Event hub (used here)

A direct method call couples the caller to the callee: the caller has to hold a
reference to whatever it notifies, and adding a second listener means editing the
caller. That coupling is what makes gameplay code calcify — the scoring system
ends up knowing about the UI, the audio, and the logger, and every new reaction to
"the player was detected" is another edit in the same place.

The **observer** pattern inverts the dependency. A publisher raises an event and
doesn't know or care who listens; subscribers register themselves; the two sides
compile independently. You reach for it whenever several unrelated systems need to
react to the same moment and you don't want the source of that moment to grow a
dependency on each reactor. The price you pay in return is traceability — you
can't "find all references" to see what happens next, and a subscriber that
forgets to unsubscribe leaks or keeps firing after it should be dead.

C# `event`s can only be invoked from inside the type that declares them, so a
static hub exposes `Raise...()` methods as its public API. That keeps invocation
encapsulated and puts the null-check (an event with no subscribers is `null`) in
one place.

**In code (`GameEvents.cs`):**

```csharp
public static event Action<GameState> OnGameStateChanged;
public static event Action OnPlayerDetected;
public static event Action<float> OnTimerUpdated;

public static void RaiseGameStateChanged(GameState state)
{
    OnGameStateChanged?.Invoke(state);
}
```

- `event Action<GameState>` declares an event other systems attach to with `+=`; the generic argument is the payload each subscriber receives.
- `OnPlayerDetected` uses a plain `Action` with no payload — it just signals that the moment happened.
- `RaiseGameStateChanged` is the public publish API; `?.Invoke` no-ops when nobody is subscribed, so raising an event with zero listeners is safe.

`GameEvents` is deliberately a plain `static` class — not a singleton, not a
MonoBehaviour — so any system can publish or subscribe without first locating a
manager instance.

Subscribers attach in `OnEnable` and detach in `OnDisable` so every `+=` has
exactly one matching `-=` (more on those messages in
[[unity-classes-and-interfaces]]). `EventLogger` does nothing but listen and
print, which is the payoff — it observes the whole game without any other system
knowing it exists.

**In code (`EventLogger.cs`):**

```csharp
private void OnEnable()
{
    GameEvents.OnGameStateChanged += HandleGameStateChanged;
    GameEvents.OnPlayerDetected += HandlePlayerDetected;
}

private void OnDisable()
{
    GameEvents.OnGameStateChanged -= HandleGameStateChanged;
    GameEvents.OnPlayerDetected -= HandlePlayerDetected;
}
```

- `OnEnable` subscribes the handlers when the component becomes active.
- `OnDisable` removes the *same* handlers, so a disabled or destroyed logger stops receiving events — skipping this is the classic source of duplicate handlers and null-reference bugs.

---
### State machine (used here)

Some objects are only ever in one of a fixed set of modes, and only certain moves
between those modes make sense — the game can go Briefing → Playing, but
Fail → Playing is nonsense. Without structure that logic scatters into booleans
(`isPlaying`, `hasFailed`) that can contradict each other, and nothing stops a
stray call from leaving the object in an impossible combination.

A **state machine** makes the modes an explicit `enum` and funnels every change
through one guarded method. You reach for it any time an object has distinct modes
with rules about moving between them — game flow here, but equally enemy AI
(patrol → chase → search) or UI screens. The design decision it encodes is
"make illegal states unrepresentable": the transition guard is the single place
that defines what's allowed, so an invalid move fails loudly instead of silently
corrupting flow. The cost is verbosity — every new state multiplies the transition
rules you have to spell out.

**In code (`GameState.cs`):**

```csharp
public enum GameState
{
    Briefing,
    Playing,
    Success,
    Fail
}
```

- These four members are the only states the game can occupy; the rest of the system reasons in terms of them rather than ad-hoc boolean flags.

`GameManager` owns the current state and routes every change through one method
that rejects no-op and illegal moves before committing and broadcasting.

**In code (`GameManager.cs`):**

```csharp
private void TransitionTo(GameState newState)
{
    if (newState == CurrentState)
    {
        return;
    }

    if (!IsValidTransition(CurrentState, newState))
    {
        Debug.LogWarning(
            $"[GameManager] Ignored invalid state transition {CurrentState} -> {newState}."
        );
        return;
    }

    CurrentState = newState;
    GameEvents.RaiseGameStateChanged(newState);
}

private static bool IsValidTransition(GameState from, GameState to)
{
    switch (from)
    {
        case GameState.Briefing:
            return to == GameState.Playing;
        case GameState.Playing:
            return to == GameState.Success || to == GameState.Fail;
        default:
            return false;
    }
}
```

- The first guard drops a transition into the state you're already in, so redundant calls do nothing.
- `IsValidTransition` is the whole rulebook: `Briefing` can only start `Playing`, `Playing` can only end in `Success` or `Fail`, and the terminal states (`default`) allow no moves out.
- An illegal move logs a warning and returns *without* changing state, so the bug surfaces in the console instead of as corrupted game flow.
- Only an accepted move reaches the last two lines, where `CurrentState` updates and `RaiseGameStateChanged` fires — so listeners are notified exactly once per real transition.

The two patterns compose: the state machine decides *what* changed, then the event
hub broadcasts it. `GameManager` also drives its own transitions off events — it
subscribes to `OnGoalReached` (→ `Success`) and `OnPlayerDetected` (→ `Fail`), so
gameplay code signals what happened and never calls the manager directly.
