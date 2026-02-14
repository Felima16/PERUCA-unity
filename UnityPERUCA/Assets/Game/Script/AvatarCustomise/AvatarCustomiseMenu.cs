using UnityEngine;

public class AvatarCustomiseMenu : MonoBehaviour
{
    void Awake()
    {
        AvatarManager.OnAvatarStateChanged += AvatarManangerOnAvatarStateChanged;
    }

    private void AvatarManangerOnAvatarStateChanged(AvatarState newState)
    {
        gameObject.SetActive(newState == AvatarState.Edit);
    }
}

