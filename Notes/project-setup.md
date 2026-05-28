# Project Setup

Concepts introduced while setting up the project: Git LFS, the feature-branch
workflow, the naming conventions, project-structure organization, and MCP.

## Git LFS

Git Large File Storage replaces large binary files in the repo with small text
*pointer* files; the actual bytes live in a separate LFS store and are
downloaded on checkout. Which paths are tracked is declared in `.gitattributes`.
This matters in Unity because Git otherwise keeps a full copy of every version
of a binary, so textures, audio, and models bloat history fast — LFS keeps the
repo small by versioning the pointers instead.

## Feature-Branch Workflow

A branch is an independent line of commits. Work for one feature happens on its
own branch, isolated from `main`, and is merged back only once the feature
works. That keeps `main` in a buildable state, so it's always safe to branch
from.

```bash
git checkout main
git pull
git checkout -b feature/guard-patrol
# ...work, commit in small steps...
git checkout main
git merge feature/guard-patrol
```

## Naming Conventions

Locked asset rules:

- **Scripts:** `PascalCase.cs`, one class per file, class name = file name
- **Prefabs:** `PascalCase`, e.g. `Player.prefab`, `Guard_Patrol.prefab`
- **SO assets:** `SO_<Type>_<Name>`, e.g. `SO_Level_Apartment`, `SO_Patrol_Loop_A`
- **Scenes:** `Level_##_Name.unity`
- **Layers:** `PascalCase`
- **Tags:** `PascalCase`

C# casing — public API surface is PascalCase; local/private is camelCase, with
private fields prefixed `_`:

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

## Project Structure

Two ways to organize assets on disk:

| Structure | Groups by | Trait |
| --- | --- | --- |
| **Type-based** | asset kind (all scripts together, all prefabs together) | matches Unity's default folders; a single feature ends up scattered |
| **Feature-based** | gameplay concern (everything for Guards lives together) | a feature is self-contained; shared assets don't map cleanly to one feature |

This project is **feature-based**: code under `_Project/Scripts/` is split by
concern (`Player`, `Guards`, `Hazards`, `Powerups`, `Core`, `UI`, `Config`),
with shared asset kinds (`Prefabs`, `Materials`, `Audio`, `VFX`,
`ScriptableObjects`, `Scenes`) in their own top-level folders. The `_` prefix
sorts `_Project/` above imported packages in the Asset Browser.

## MCP (Model Context Protocol)

MCP is a protocol that exposes tools an AI assistant can call. The UnityMCP
server connects Claude to the running Unity Editor, so scene and asset
operations (creating GameObjects, materials, folders, scripts) execute directly
in the Editor — keeping Unity in charge of serialization and GUIDs rather than
hand-editing `.unity`/`.meta` files.
