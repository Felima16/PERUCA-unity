# AI Coding Agent Instructions for UnityPERUCA

## Project Overview
**UnityPERUCA** is a Meta Quest VR application for avatar customization and interactive games. It uses Unity 6000.3.3 targeting Android, with XR hand tracking (Meta XR Interaction Toolkit) and PICO platform integration.

## Architecture & Component Boundaries

### Core Game Flow
1. **AvatarManager** (`Assets/Game/Script/Manager/AvatarManager.cs`) - Singleton that manages global game states:
   - `EditAvatar` → `Game` → `Victory`
   - Broadcasts state changes via `OnAvatarStateChanged` event
   - All major systems listen to this event for transitions

2. **DialogueManager** (`Assets/Game/Script/DialogueSystem/`) - Manages narrative:
   - Loads dialogue cases from JSON in `Assets/Resources/DialogueSystem/Cases/`
   - Maps `DialogueScene` (OnBoarding, etc.) to dialogue options via `SceneManager`
   - Uses `DialogueCase` objects to control UI panels and button interactions

3. **Avatar System** - Split by gender:
   - `AvatarCustom` class manages customization UI and character assembly
   - Elements: Head, Eyebrow, FacialHair, Hair (stored as `CharacterObjectGroups`)
   - Enum-based selection: `Gender`, `Race`, `SkinColor`, `HeadCovering`, `FacialHair`
   - Material swapping for colors via shared `mat` reference

### Feature Modules
- **AvatarCustomise**: UI-driven character builder with color/part selection
- **Wander**: NPC AI using state machine (`IdleState`, `MovementState`) with NavMesh navigation
- **SlideGame**: Puzzle game with pieces and zone validation
- **Gesture**: Hand gesture recognition for interactions (e.g., `TeleportStaticHandGesture`)
- **Menu**: Main menu navigation between Teleport and Help sections
- **Teleport**: VR teleportation mechanics

## Code Patterns & Conventions

### Singleton Pattern (Strict)
```csharp
public class AvatarManager : MonoBehaviour {
    public static AvatarManager instance;
    private void Awake() {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
}
```
Used for: `AvatarManager`, `DialogueManager`, `StringManager`. Clean up in `OnDestroy()`.

### Event-Driven Communication
```csharp
public static event System.Action<AvatarState> OnAvatarStateChanged;
OnAvatarStateChanged?.Invoke(newState);
```
Prefer events over direct coupling. Subscribe in `OnEnable()`, unsubscribe in `OnDestroy()`.

### Namespace Organization
- Root namespace: `AvatarLab` (e.g., `AvatarLab.Wander`)
- Separate namespaces only when needed to avoid conflicts
- Keep game scripts largely un-namespaced (Assembly-CSharp default)

### Enum-Driven Configuration
Heavy use of enums for customization states:
```csharp
public enum Gender { Male, Female }
public enum AvatarElement { Head, Eyebrow, FacialHair, Hair }
public enum HeadCovering { HeadCoverings_Base_Hair, HeadCoverings_No_FacialHair }
```
Use enums to avoid magic strings; map to lists at runtime.

### Resource Loading
All user-facing data loads from `Assets/Resources/`:
- JSON dialogue cases: `Resources.Load<TextAsset>("DialogueSystem/Cases/OnBoardingCase")`
- Textures, audio, prefabs follow this pattern
- Never use hard paths; always use Resources folder

### NavMesh + CharacterController Pattern
`AvatarWander` combines both systems:
```csharp
RequireComponent(typeof(Animator))
RequireComponent(typeof(CharacterController))
private NavMeshAgent navMeshAgent;
```
For NPC movement: use NavMesh for pathfinding, CharacterController for physics.

## XR & VR-Specific Notes

### Meta XR Interaction Toolkit Integration
- Hand gesture detection: `TeleportStaticHandGesture` class
- Tracked in `Assets/XR/Loaders/` and `Assets/XRI/Settings/`
- Uses `UnityEngine.XR.Interaction.Toolkit` namespace
- Always check for VR rig presence in Start()

### PICO Platform
- Settings in `Assets/Resources/`: `PXR_ProjectSetting.asset`, `PXR_SDKSettingAsset.asset`
- Build target: Android (Quest/PICO devices)
- Check compiler symbols for conditional XR code:
  ```csharp
  #if UNITY_ANDROID
  #endif
  ```

## Project Build & Dependencies

### Key Dependencies (from .csproj)
- **Unity.XR.PICO** - Meta Quest SDK
- **Unity.XR.Interaction.Toolkit** - XR controller/hand interaction framework
- **Unity.XR.Hands** - Hand tracking samples
- **Polyperfect.People** - Avatar/character system (3rd party)
- **TextMeshPro** - UI text rendering
- **Pico.Spatializer** - Audio spatialization

### Build Target
- **Platform**: Android
- **API Level**: Configured in ProjectSettings
- **Language**: C# with .NET Standard 2.1 (netstandard2.1)

### Multiple Solution Files
- `UnityPERUCA.sln` (primary)
- `ProjectVR.sln` (legacy; prefer UnityPERUCA)

## Debugging & Testing

### Editor Configuration
- Platform override to Android in PlayerSettings
- Scenes: `HomeScene.unity` (main), `HandTest.unity` (hand tracking tests)
- Debug panel: `PXR_DebuggerPanel.prefab` in Resources

### Conditional Debugging
```csharp
#if UNITY_EDITOR
    SceneView.RepaintAll();
#endif
```
Editor gizmos/visualization only compile in editor context.

### Logging Convention
Use `Debug.LogError()`, `Debug.Log()` with contextual class names. Console output captured in `Logs/relay.txt`.

## File Organization Reference

| Path | Purpose |
|------|---------|
| `Assets/Game/Script/Manager/` | Global systems (AvatarManager, DialogueManager) |
| `Assets/Game/Script/DialogueSystem/` | Dialogue engine + JSON data loading |
| `Assets/Game/Script/AvatarCustomise/` | Avatar assembly & UI controllers |
| `Assets/Game/Script/Wander/` | NPC AI state machine |
| `Assets/UI/` | UI materials, shaders, and assets |
| `Assets/Resources/DialogueSystem/Cases/` | JSON dialogue data |
| `Assets/XR/` & `Assets/XRI/` | VR setup and hand tracking config |
| `ProjectSettings/` | Unity project config (read-only in editor) |

## When Adding New Features

1. **State-driven features** → Hook into `AvatarManager.OnAvatarStateChanged` or create sister singleton
2. **UI features** → Create under `Assets/Game/Script/Menu/` or `Assets/Game/Script/[FeatureName]/`
3. **Data-heavy features** → Add JSON to `Assets/Resources/` and load via `SceneManager` pattern
4. **VR interactions** → Inherit from XR Toolkit base classes; test in `HandTest.unity`
5. **NPC/Avatar behavior** → Extend `AvatarWander` state machine (`AIState` subclasses)

## Critical Gotchas

- **Do NOT modify `ProjectSettings.asset` manually** – changes revert on next Unity open
- **Dialogue system expects JSON wrapper format** – see `DialogueCaseWrapper` class
- **CharacterObjectGroups must be assigned per-gender** – male/female lists are separate
- **NavMesh must be baked** before AvatarWander can pathfind; check Scene tab in editor
- **XR hand gestures require calibration** – test on device, not always accurate in editor
