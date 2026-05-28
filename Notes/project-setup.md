# Project Setup

The concepts introduced while standing up the project: how large binaries are
versioned, how work flows through branches, the naming and folder conventions
that keep the project navigable, and how Claude edits the Unity project through
MCP.

---
## Git LFS

Git stores a full copy of every version of every file. For text that's fine —
diffs are tiny — but a Unity project is full of binaries (textures, audio,
models, `.unity` scenes) where each "version" is the whole file again, so history
bloats fast and clones get slow.

**Git Large File Storage** replaces those binaries in the repo with small text
*pointer* files; the real bytes live in a separate LFS store and are downloaded
on checkout. Which paths are tracked is declared in `.gitattributes`:

```gitattributes
*.png filter=lfs diff=lfs merge=lfs -text   # texture bytes go to LFS, not git history
*.psd filter=lfs diff=lfs merge=lfs -text
*.wav filter=lfs diff=lfs merge=lfs -text   # audio
*.fbx filter=lfs diff=lfs merge=lfs -text   # models
```

Git then versions the lightweight pointers, keeping the repo small while the
heavy assets stay retrievable.

---
## Feature-Branch Workflow

A branch is an independent line of commits. Doing each feature on its own branch
keeps half-finished work off `main`, so `main` always builds and is always safe
to branch from. The feature merges back only once it works.

```bash
git checkout main
git pull                              # start from the latest main
git checkout -b feature/guard-patrol  # branch named feature/<thing>
# ...work, commit in small steps...
git checkout main
git merge feature/guard-patrol        # fold the finished feature back in
```

---
## Naming Conventions

Consistent names make a file's kind and role obvious from the name alone, and
keep Unity's asset references (stored by GUID, not path) readable in the
Inspector.

**Asset rules:**

- **Scripts:** `PascalCase.cs`, one class per file, class name = file name
- **Prefabs:** `PascalCase`, e.g. `Player.prefab`, `Guard_Patrol.prefab`
- **SO assets:** `SO_<Type>_<Name>`, e.g. `SO_Level_Apartment`, `SO_Patrol_Loop_A`
- **Scenes:** `Level_##_Name.unity`
- **Layers / Tags:** `PascalCase`

**C# casing** — the public API surface is PascalCase; local and private members
are camelCase, with private fields prefixed `_`:

| Element | Convention | Example |
| --- | --- | --- |
| Classes / MonoBehaviours / ScriptableObjects | PascalCase | `GuardController` |
| Interfaces | `I` + PascalCase | `IDamageable` |
| Methods, public fields, properties | PascalCase | `TakeDamage`, `Health` |
| Private fields | `_camelCase` | `_currentHealth` |
| Local variables / parameters | camelCase | `targetPosition` |
| Constants | PascalCase | `MaxHealth` |
| Enums (type and members) | PascalCase | `GuardState.Patrolling` |
| Events | PascalCase, `On`-prefixed | `OnPlayerSpotted` |

```csharp
public class GuardController : MonoBehaviour
{
    public const int MaxHealth = 100;                 // PascalCase constant
    public event System.Action OnPlayerSpotted;       // On-prefixed event
    public int Health { get; private set; }           // PascalCase property

    [SerializeField] private float _patrolSpeed;      // _camelCase private field
    private GuardState _state;

    public void TakeDamage(int amount)                // PascalCase method
    {
        int clamped = Mathf.Min(amount, Health);      // camelCase local
        Health -= clamped;
    }
}

public enum GuardState { Idle, Patrolling, Chasing }

public interface IDamageable
{
    void TakeDamage(int amount);
}
```

---
## Project Structure

Assets can be grouped two ways on disk:

| Structure | Groups by | Trait |
| --- | --- | --- |
| **Type-based** | asset kind (all scripts together, all prefabs together) | matches Unity's defaults; a single feature ends up scattered |
| **Feature-based** | gameplay concern (everything for Guards lives together) | a feature is self-contained; shared asset kinds don't map to one feature |

This project is **feature-based**. Code under `Scripts/` is split by concern, and
shared asset kinds get their own top-level folders. The `_` prefix sorts
`_Project/` above imported packages in the Project window:

```text
Assets/
└── _Project/                 # _ sorts this above imported packages
    ├── Scenes/                # Level_##_Name.unity
    ├── ScriptableObjects/     # SO_*.asset data assets
    └── Scripts/
        ├── Core/              # GameManager, GameEvents, GameState, EventLogger
        ├── Config/            # ScoringConfig, LevelConfig, Rank
        ├── Player/            # (per-concern gameplay folders)
        ├── Guards/
        ├── Hazards/
        ├── Powerups/
        └── UI/
```

Shared kinds like `Prefabs`, `Materials`, `Audio`, and `VFX` sit as their own
folders under `_Project/` alongside `Scripts/`.

---
## MCP (Model Context Protocol)

MCP is a protocol that exposes a set of tools an AI assistant can call. The
**UnityMCP** server connects Claude to the running Unity Editor, so scene and
asset operations run *inside* the Editor rather than by editing files on disk.

That boundary matters: Unity owns serialization and GUID generation.
Hand-editing a `.unity`, `.meta`, or `.asset` file — or forging a GUID — risks
corrupting the scene and breaking references. Letting the Editor make the change
keeps everything consistent.

| Do through MCP | Don't hand-edit |
| --- | --- |
| create / modify GameObjects, components, materials | `.unity` scene files |
| create folders, prefabs, ScriptableObject assets | `.meta` / `.asset` files |
| run menu items, manage scenes | forged GUIDs (`uuidgen`) |
