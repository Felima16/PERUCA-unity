using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SubMenuButton : MonoBehaviour
{
    private Button button;
    private bool isInitialized = false;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
        }
    }

    /// <summary>
    /// Initialize the submenu button with label and callback
    /// </summary>
    public void Initialize(string label, System.Action onClicked)
    {
        if (isInitialized) return;

        // Set label text
        TextMeshProUGUI buttonText = GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
            buttonText.text = label;

        // Subscribe to button click
        button.onClick.AddListener(() => onClicked?.Invoke());
        isInitialized = true;
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveAllListeners();
    }
}