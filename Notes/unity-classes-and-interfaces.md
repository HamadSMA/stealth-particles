## Unity Base Classes & Interfaces

The Unity engine types our scripts inherit from or implement, and the members
they hand us. New base classes and interfaces get added here as the project
starts using them.

---
### MonoBehaviour

`MonoBehaviour` is the base class a script inherits from to become a *component*
you can attach to a GameObject. A plain C# class can't sit on a GameObject or
receive engine callbacks; inheriting `MonoBehaviour` is what lets Unity find the
script, show it in the Inspector, and call lifecycle methods on it. You never
call those methods yourself and you don't `override` them — you declare a method
with the right name and Unity invokes it by reflection.

The lifecycle methods are the reason the class exists: they let you hook setup,
per-frame work, and teardown to the exact point in an object's life where each
belongs, instead of polling for state changes yourself.

| Method | When Unity calls it | Typical use |
| --- | --- | --- |
| `Awake` | once, as the object loads, before any `Start` | cache references on self |
| `OnEnable` | every time the component becomes active | subscribe to events |
| `Start` | once, before the first frame, after all `Awake`s | setup needing other objects ready |
| `Update` | every frame | per-frame input and game logic |
| `FixedUpdate` | on the fixed physics tick (0..n times per frame) | physics / `Rigidbody` work |
| `LateUpdate` | every frame, after all `Update`s | camera follow, reacting to moved objects |
| `OnDisable` | every time the component becomes inactive (and before destroy) | unsubscribe from events |
| `OnDestroy` | once, when the object is destroyed | final cleanup |

`OnEnable`/`OnDisable` are the pair you reach for most: subscribe in one,
unsubscribe in the other, so a subscription has exactly one matching removal per
enable/disable cycle and a disabled object stops reacting. `GameManager` and
`EventLogger` both use them this way (this is the observer pattern from
[[design-patterns]]).

**In code (`GameManager.cs`):**

```csharp
private void OnEnable()
{
    GameEvents.OnGoalReached += HandleGoalReached;
    GameEvents.OnPlayerDetected += HandlePlayerDetected;
}

private void OnDisable()
{
    GameEvents.OnGoalReached -= HandleGoalReached;
    GameEvents.OnPlayerDetected -= HandlePlayerDetected;
}
```

- `OnEnable` wires the manager's handlers to the event hub the moment the component activates.
- `OnDisable` detaches the *same* handlers, so the manager stops reacting once it's disabled or destroyed — the symmetry is what prevents leaked or doubled subscriptions.

Beyond the lifecycle, every component inherits a handful of members you use
constantly:

| Member | What it does |
| --- | --- |
| `transform` | this object's `Transform` — position, rotation, scale, hierarchy |
| `gameObject` | the GameObject the component is attached to |
| `enabled` | turn this component's per-frame callbacks on or off |
| `GetComponent<T>()` | fetch another component on the same GameObject |
| `GetComponentInChildren<T>()` / `InParent<T>()` | search down / up the hierarchy |
| `Instantiate(original)` | clone a GameObject or Object at runtime |
| `Destroy(obj)` | destroy a GameObject, component, or asset |
| `StartCoroutine(routine)` | run an `IEnumerator` across multiple frames |
| `CompareTag("Player")` | allocation-free tag check |

Two attributes show up on almost every MonoBehaviour: `[SerializeField]` exposes
a private field in the Inspector without making it `public`, and `[ContextMenu]`
adds a method to the component's gear-icon menu for manual testing.

**In code (`GameManager.cs`):**

```csharp
[SerializeField]
private LevelConfig _levelConfig;

[ContextMenu("TEST: Start Level")]
private void TestStartLevel()
{
    StartLevel();
}
```

- `[SerializeField]` keeps `_levelConfig` `private` in code but assignable in the Inspector, so a designer drops the level asset in without the field becoming publicly writable from other scripts.
- `[ContextMenu("TEST: Start Level")]` adds a right-click entry on the component, letting you fire `StartLevel()` in the editor without building any UI to trigger it.

A third attribute, `[RequireComponent]`, is class-level rather than field-level:
it declares that this component depends on another. Unity auto-adds the
dependency when the script is attached and blocks its removal while the script is
present, so a component that assumes a sibling exists can never be put on a
GameObject that lacks it.

**In code (`PlayerMovement.cs`):**

```csharp
[RequireComponent(typeof(NavMeshAgent))]
public class PlayerMovement : MonoBehaviour
```

- `[RequireComponent(typeof(NavMeshAgent))]` guarantees a `NavMeshAgent` sits on the same GameObject, so the `GetComponent<NavMeshAgent>()` cached in `Awake` can never come back `null` — the dependency is enforced at attach time instead of discovered as a runtime bug. The agent itself is covered in [[navmesh-navigation]].

---
### ScriptableObject

`ScriptableObject` is the other base class we inherit (`ScoringConfig`,
`LevelConfig`). Like `MonoBehaviour` it derives from `UnityEngine.Object`, but an
instance is **not** attached to a GameObject — it lives as a standalone `.asset`
file in the project. That makes it the natural home for shared, designer-editable
data. It receives a few of the same messages: `Awake` (on creation),
`OnEnable`/`OnDisable` (on load/unload), and `OnValidate` (in the editor when a
value changes).

The decision it really encodes is *where tuning values live and who owns them*.
Hardcoding bakes a value into source (`const int MaxScore = 2000;`), which means
only a programmer can change it and only with a recompile. Moving it onto an asset
flips that:

| | Hardcoded value | ScriptableObject asset |
| --- | --- | --- |
| Change a value | edit code, recompile | edit in Inspector, no recompile (even at runtime) |
| Who can edit | programmers | anyone, including designers |
| Variants | new code / branches | duplicate the asset (e.g. one per level) |
| Sharing | value copied per use site | many objects reference one asset; change once |
| Data vs logic | mixed together | data in the asset, logic in the code that reads it |

The win is iteration speed and designer ownership; the cost is asset sprawl if you
reach for it on data that was never going to change. `ScoringConfig` is a good fit
because its values are exactly the kind a designer wants to tune repeatedly — and
it keeps the scoring *logic* on the same type, so the formula and the numbers it
reads can't drift apart.

**In code (`ScoringConfig.cs`):**

```csharp
[CreateAssetMenu(fileName = "SO_Scoring_Config", menuName = "Stealth Particles/Scoring Config")]
public class ScoringConfig : ScriptableObject
{
    public int maxScore = 2000;
    public float sRankThreshold = 0.20f;

    public int CalculateScore(float elapsed, float budget)
    {
        float remaining = Mathf.Max(0f, 1f - (elapsed / budget));
        return Mathf.FloorToInt(maxScore * (remaining * remaining));
    }
}
```

- `maxScore` and `sRankThreshold` are public fields, so they appear as editable values on the `.asset` in the Inspector.
- `CalculateScore` lives on the same type, so the scoring formula reads the tuning values directly — there's no separate logic class to keep in sync with the data.

A field typed as a ScriptableObject becomes a *reference* to an asset, which lets
configs nest. `LevelConfig` holds per-level data and points at an optional
`ScoringConfig`, so one level can swap in its own scoring rules.

**In code (`LevelConfig.cs`):**

```csharp
public float timeBudget;
public ScoringConfig scoringOverride;
```

- `timeBudget` is plain per-level data stored on the asset.
- `scoringOverride` references another ScriptableObject asset; assigning one in the Inspector overrides the global scoring rules for that level, and leaving it `null` falls back to the default.

`[CreateAssetMenu]` is the attribute that makes an instance creatable: it adds an
entry to Unity's **Assets ▸ Create** menu (and the Project window's right-click
**Create**), so you spawn `.asset` files without writing an editor script.

| Property | What it sets |
| --- | --- |
| `menuName` | the menu path; `/` creates submenus |
| `fileName` | default name for a newly created asset |
| `order` | optional int to position the entry in the menu |

---
### Interfaces

None implemented yet. Add each one here with its members as it appears, following
the `I`-prefixed naming convention from [[project-setup]] (e.g. `IDamageable`).
