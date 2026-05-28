# Unity Base Classes & Interfaces

The Unity engine types our scripts inherit from or implement, and the members
they hand us. New base classes and interfaces get added here as the project
starts using them.

---
## MonoBehaviour

`MonoBehaviour` is the base class a script inherits from to become a *component*
you can attach to a GameObject. A plain C# class can't sit on a GameObject or
receive engine callbacks; inheriting `MonoBehaviour` is what lets Unity find the
script, show it in the Inspector, and call lifecycle methods on it. You never
call those methods yourself and you don't `override` them — you declare a method
with the right name and Unity invokes it.

**Lifecycle methods, in call order:**

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

`OnEnable`/`OnDisable` come as a pair: subscribe in one, unsubscribe in the
other, so every subscription has exactly one matching removal per enable/disable
cycle. `GameManager` and `EventLogger` both use them this way (this is the
observer pattern from [[design-patterns]]):

```csharp
private void OnEnable()
{
    GameEvents.OnGoalReached    += HandleGoalReached;     // wire up listeners when active
    GameEvents.OnPlayerDetected += HandlePlayerDetected;
}

private void OnDisable()
{
    GameEvents.OnGoalReached    -= HandleGoalReached;     // tear them down again when inactive
    GameEvents.OnPlayerDetected -= HandlePlayerDetected;
}
```

**Commonly used members** (inherited; available on every component):

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

`[SerializeField]` exposes a private field in the Inspector without making it
`public`, and `[ContextMenu]` adds a method to the component's gear-icon menu for
manual testing. `GameManager` uses both:

```csharp
[SerializeField] private LevelConfig _levelConfig;   // editable in Inspector, still private in code

[ContextMenu("TEST: Start Level")]                   // right-click the component to invoke
private void TestStartLevel() => StartLevel();
```

---
## ScriptableObject

`ScriptableObject` is the other base class we inherit (`ScoringConfig`,
`LevelConfig`). Like `MonoBehaviour` it derives from `UnityEngine.Object`, but an
instance is **not** attached to a GameObject — it lives as a standalone `.asset`
file in the project. That makes it the natural home for shared, designer-editable
data. It receives a few of the same messages: `Awake` (on creation),
`OnEnable`/`OnDisable` (on load/unload), and `OnValidate` (in the editor when a
value changes).

**ScriptableObject asset vs hardcoded value.** Hardcoding bakes values into
source (`const int MaxScore = 2000;`). Moving them onto an asset changes who can
edit them and when:

| | Hardcoded value | ScriptableObject asset |
| --- | --- | --- |
| Change a value | edit code, recompile | edit in Inspector, no recompile (even at runtime) |
| Who can edit | programmers | anyone, including designers |
| Variants | new code / branches | duplicate the asset (e.g. one per level) |
| Sharing | value copied per use site | many objects reference one asset; change once |
| Data vs logic | mixed together | data in the asset, logic in the code that reads it |

`ScoringConfig` holds tuning values as an asset *and* carries the logic that
reads them, so the data and the formula that consumes it travel together:

```csharp
[CreateAssetMenu(fileName = "SO_Scoring_Config", menuName = "Stealth Particles/Scoring Config")]
public class ScoringConfig : ScriptableObject
{
    public int maxScore = 2000;            // designer-tunable in the Inspector
    public float sRankThreshold = 0.20f;

    public int CalculateScore(float elapsed, float budget) { /* ... */ }   // logic ships with the data
}
```

A field typed as a ScriptableObject becomes a *reference* to an asset, so configs
can nest. `LevelConfig` holds level data and points at an optional
`ScoringConfig`, letting one level override the global scoring rules:

```csharp
public float timeBudget;                // plain data
public ScoringConfig scoringOverride;   // reference to another SO asset, assigned in the Inspector
```

**`[CreateAssetMenu]`** is an attribute on a `ScriptableObject` subclass that adds
an entry to Unity's **Assets ▸ Create** menu (and the Project window's right-click
**Create**), so you can spawn `.asset` instances without writing an editor script.

| Property | What it sets |
| --- | --- |
| `menuName` | the menu path; `/` creates submenus |
| `fileName` | default name for a newly created asset |
| `order` | optional int to position the entry in the menu |

---
## Interfaces

None implemented yet. Add each one here with its members as it appears, following
the `I`-prefixed naming convention from [[project-setup]] (e.g. `IDamageable`).
