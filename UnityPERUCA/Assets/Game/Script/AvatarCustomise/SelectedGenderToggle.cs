using AvatarLab;
using UnityEngine;
using UnityEngine.UI;

public class SelectedGenderToggle : MonoBehaviour
{
    [SerializeField]
    private Toggle toggle;
    [SerializeField]
    public Gender gender;

    AvatarCustom avatarCustomScript;

    void Start()
    {
        avatarCustomScript = GameObject.Find("MainCharacters").GetComponent<AvatarCustom>();
    }

    void OnEnable()
    {
        toggle.onValueChanged.AddListener(SelectedTypeColor);
    }

    void OnDisable()
    {
        toggle.onValueChanged.RemoveListener(SelectedTypeColor);
    }

    private void SelectedTypeColor(bool isOn)
    {
        if (!isOn) return;
        avatarCustomScript.SelectGender(gender);
    }
}
