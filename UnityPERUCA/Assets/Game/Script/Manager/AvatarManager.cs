using UnityEngine;

public class AvatarManager : MonoBehaviour
{
    public static AvatarManager instance;
    public static event System.Action<AvatarState> OnAvatarStateChanged;
    public AvatarState state { get; private set; }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
        
    }

    void Start() {
        UpdateAvatarState(AvatarState.Edit);
    }

    public void UpdateAvatarState(AvatarState newState)
    {
        state = newState;

        OnAvatarStateChanged?.Invoke(newState);
    }
    void OnDestroy()
    {
        instance = null;
        OnAvatarStateChanged = null;
    }
}

public enum AvatarState
{
    Edit,
    Game,
    Help
}
