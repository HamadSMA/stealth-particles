## Project Setup

The concepts introduced while standing up the project: how large binaries are
versioned, how work flows through branches, the naming and folder conventions
that keep the project navigable, and how Claude edits the Unity project through
MCP.

---
### Git LFS

Git stores a full copy of every version of every file. For text that's fine —
diffs are tiny — but a Unity project is full of binaries (textures, audio,
models, `.unity` scenes) where each "version" is the whole file again, so history
bloats fast and clones get slow.

**Git Large File Storage** replaces those binaries in the repo with small text
*pointer* files; the real bytes live in a separate LFS store and are downloaded
on checkout. Which paths are tracked is declared in `.gitattributes`.

**In code (`.gitattributes`):**

```gitattributes
*.png filter=lfs diff=lfs merge=lfs -text
*.psd filter=lfs diff=lfs merge=lfs -text
*.wav filter=lfs diff=lfs merge=lfs -text
*.fbx filter=lfs diff=lfs merge=lfs -text
```

- Each line matches a binary file type and routes its contents to the LFS store instead of git history.
- `filter=lfs diff=lfs merge=lfs` swaps git's normal handling for the LFS driver; `-text` tells git not to treat the file as text (no line-ending conversion or line-based diffing).
- Git then versions only the lightweight pointer files, so the repo stays small while the heavy assets are still fetched on checkout.

---
### Feature-Branch Workflow

A branch is an independent line of commits. Doing each feature on its own branch
keeps half-finished work off `main`, so `main` always builds and is always safe
to branch from. The feature merges back only once it works.

**In code (terminal):**

```bash
git checkout main
git pull
git checkout -b feature/guard-patrol
git checkout main
git merge feature/guard-patrol
```

- `git pull` on `main` first, so the new branch starts from the latest shared history.
- `git checkout -b feature/guard-patrol` creates and switches to the feature branch in one step; the `feature/<thing>` prefix keeps branches self-describing.
- After committing the work in small steps, switching back to `main` and running `git merge` folds the finished feature in.

---
### Naming Conventions

Consistent names make a file's kind and role obvious from the name alone, and keep
Unity's asset references (stored by GUID, not path) readable in the Inspector.

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

**In code (`GuardController.cs`):**

```csharp
public class GuardController : MonoBehaviour
{
    public const int MaxHealth = 100;
    public event System.Action OnPlayerSpotted;
    public int Health { get; private set; }

    [SerializeField] private float _patrolSpeed;
    private GuardState _state;

    public void TakeDamage(int amount)
    {
        int clamped = Mathf.Min(amount, Health);
        Health -= clamped;
    }
}

public enum GuardState { Idle, Patrolling, Chasing }

public interface IDamageable
{
    void TakeDamage(int amount);
}
```

- The public surface — `MaxHealth`, `OnPlayerSpotted`, `Health`, `TakeDamage` — is PascalCase, with the event `On`-prefixed.
- `_patrolSpeed` and `_state` are private fields, so they take the `_camelCase` prefix; `clamped` is a local and stays plain camelCase.
- The enum (type and members) and the `I`-prefixed interface follow PascalCase.

---
### Project Structure

Assets can be grouped two ways on disk, and the choice shapes how it feels to work
on a single feature:

| Structure | Groups by | Pros | Cons |
| --- | --- | --- | --- |
| **Type-based** | asset kind (all scripts together, all prefabs together) | matches Unity's default folders; familiar | one feature scatters across many folders |
| **Feature-based** | gameplay concern (everything for Guards together) | a feature is self-contained and easy to find | shared asset kinds don't belong to any one feature |

This project is **feature-based**, because the day-to-day work is "build one
mechanic at a time" and keeping a mechanic's pieces together beats matching Unity's
defaults. Code under `Scripts/` is split by concern; the few genuinely shared asset
kinds get their own top-level folders instead.

**In code (folder layout):**

```text
Assets/
└── _Project/
    ├── Scenes/
    ├── ScriptableObjects/
    └── Scripts/
        ├── Core/
        ├── Config/
        ├── Player/
        ├── Guards/
        ├── Hazards/
        ├── Powerups/
        └── UI/
```

- `_Project/` holds everything authored for this game; the leading `_` sorts it above imported packages in the Project window.
- `Scripts/` is split by gameplay concern (`Core`, `Config`, `Player`, `Guards`, …) so one feature lives in one folder rather than spread across separate script/prefab/material folders.
- Shared asset kinds like `Prefabs`, `Materials`, `Audio`, and `VFX` sit as their own folders under `_Project/` alongside `Scripts/`.

---
### MCP (Model Context Protocol)

MCP is a protocol that exposes a set of tools an AI assistant can call. The
**UnityMCP** server connects Claude to the running Unity Editor, so scene and
asset operations run *inside* the Editor rather than by editing files on disk.

That boundary is the whole point: Unity owns serialization and GUID generation.
Hand-editing a `.unity`, `.meta`, or `.asset` file — or forging a GUID — risks
corrupting the scene and breaking the references Unity tracks by GUID. Routing the
change through the Editor keeps everything consistent.

| Do through MCP | Don't hand-edit |
| --- | --- |
| create / modify GameObjects, components, materials | `.unity` scene files |
| create folders, prefabs, ScriptableObject assets | `.meta` / `.asset` files |
| run menu items, manage scenes | forged GUIDs (`uuidgen`) |
