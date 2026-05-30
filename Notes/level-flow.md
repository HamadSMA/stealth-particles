## Level Flow — Objective, Win/Lose, and Results

A level is a tiny arc: start, play toward an objective, end in success or failure.
But the *pieces* that drive those transitions are many small independent systems —
the loot, the exit, a countdown, a scorer, the results panel — and wiring them to
each other directly would tangle every system into every other. Instead they
coordinate through the `GameEvents` hub and the `GameManager` state machine (both in
[[design-patterns]]): each system only knows the *moment* it cares about, never the
other systems. The result is that "what causes a win" and "what reacts to a win" can
each change without touching the other.

---
### The win path is an objective chain

Winning is a sequence of independent steps, each handing off to the next through an
event rather than a method call. Loot pickups raise their collection; the
`LootManager` is the only thing that knows the *count*, so it watches them and raises
`OnAllLootCollected` when the last one lands. The `Goal` listens for that — until it
fires, the exit is **hidden and inert** (a floor marker shows where it will appear),
which turns "collect everything" into a visible, earned objective rather than a hidden
rule.

**In code (`Goal.cs`):**

```csharp
private void HandleAllLootCollected()
{
    allLootCollected = true;
    UpdateVisibility();
}

private void UpdateVisibility()
{
    if (goalRenderer != null)
    {
        goalRenderer.enabled = !requiresAllLoot || allLootCollected;
    }
}

private void OnTriggerEnter(Collider other)
{
    if (hasReached || !isPlaying)
    {
        return;
    }

    if (!other.CompareTag("Player"))
    {
        return;
    }

    if (requiresAllLoot && !allLootCollected)
    {
        return;
    }

    hasReached = true;
    GameEvents.RaiseGoalReached();
}
```

- `requiresAllLoot` is the gate and `allLootCollected` is the latch it waits on; `UpdateVisibility` ties the *look* to the same condition, so the goal becomes visible exactly when it becomes reachable.
- `OnTriggerEnter` bails on every disqualifier before committing — already-fired, not playing, not the player, loot still owed — then raises `OnGoalReached` once. `GameManager` turns that into `Success`.
- When no loot system is present the goal treats itself as already satisfied (set in `Start`), so it still works standalone — the gate is opt-in, not a hard dependency.

---
### The lose path is one funnel with several taps

Failure has multiple causes — a guard *sees* you, a guard *touches* you, or the clock
runs out — but only one outcome. Rather than three different paths into `GameManager`,
all three raise the same `OnPlayerDetected`, which the manager routes to `Fail`. The
timer is the clearest example: it owns no knowledge of guards or UI, it just publishes
the same "you lost" signal when it overflows.

**In code (`LevelTimer.cs`):**

```csharp
elapsed += Time.deltaTime;
GameEvents.RaiseTimerUpdated(elapsed);

if (levelConfig != null && elapsed >= levelConfig.timeBudget)
{
    isRunning = false;
    GameEvents.RaisePlayerDetected();
}
```

- The timer broadcasts `OnTimerUpdated` every frame for any HUD to read, and on overflow reuses `OnPlayerDetected` as a generic "run ended badly" rather than inventing a separate timeout event.
- Because vision ([[vision-cone]]), contact, and timeout all converge on one event, `GameManager` needs a single `Fail` handler and none of the three causes knows the others exist.

---
### Scoring is computed *at* the transition, from data the timer kept

Score is not tracked continuously; it is derived once, the instant the level is won,
from the elapsed time. Keeping the formula in a `ScriptableObject` (`ScoringConfig`)
means designers tune the curve and rank cutoffs as data, and a level can override the
global config with its own.

**In code (`ScoringConfig.cs`):**

```csharp
public int CalculateScore(float elapsed, float budget)
{
    if (budget <= 0f)
    {
        return 0;
    }

    float remaining = Mathf.Max(0f, 1f - (elapsed / budget));
    int score = Mathf.FloorToInt(maxScore * (remaining * remaining));
    return Mathf.Max(0, score);
}
```

- Score is the fraction of the budget left, **squared**, times `maxScore` — squaring makes the curve reward speed steeply (finishing in half the budget keeps 75% of the points, not 50%).
- Rank is a separate parallel test (`GetRank`) over the same `elapsed / budget` fraction against ordered thresholds (S/A/B/C), so the letter and the number stay consistent without recomputing one from the other.
- The budget is the single knob balancing both: it sets where the score curve bottoms out *and*, as fractions of it, every rank cutoff — so tuning one value tunes the whole reward.

---
### When one listener feeds another: ordering a shared event

Two systems both react to `OnGameStateChanged(Success)`: `ScoreCalculator` *computes*
the result, and `ResultsScreen` *displays* it by reading `ScoreCalculator.LastScore`
and friends. That only works if the calculator runs first — and the order in which
subscribers to a C# event fire is the order they subscribed, which is the order their
`OnEnable` ran, which Unity drives by **script execution order**. Left to chance the
results panel could read last frame's (cleared) values.

**In code (`ResultsScreen.cs`):**

```csharp
[DefaultExecutionOrder(100)]
public class ResultsScreen : MonoBehaviour
{
    ...
    private void ShowSuccess()
    {
        rankText.text = scoreCalculator.LastRank.ToString();
        scoreText.text = "SCORE   " + scoreCalculator.LastScore;
        timeText.text = "TIME   " + FormatTime(scoreCalculator.LastTime);
        Show();
    }
}
```

- `[DefaultExecutionOrder(100)]` pushes `ResultsScreen` *after* `ScoreCalculator` (default 0): it subscribes later, so its handler fires later, so by the time it reads `LastScore` the calculator has already written it.
- The general lesson for an event hub: decoupling removes *reference* order, not *time* order — when one subscriber produces what another consumes on the same event, make the dependency explicit with execution order (or split into a second event the producer raises when done) rather than hoping.

The whole loop is the two patterns from [[design-patterns]] working at level scale: the
state machine names *what* the level is doing, and the event hub lets a fistful of
single-purpose systems cause and react to those changes without ever holding a
reference to one another.
