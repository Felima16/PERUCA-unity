using UnityEngine;
using UnityEngine.UI;
using System;

public class MenuToggle : MonoBehaviour
{
    private Toggle toggle;
    private bool isInitialized = false;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        if (toggle == null)
        {
            toggle = gameObject.AddComponent<Toggle>();
        }
    }

    /// <summary>
    /// Initialize the toggle with a label and callback
    /// </summary>
    public void Initialize(string label, ToggleGroup toggleGroup, UnityEngine.Events.UnityAction<bool> onValueChanged)
    {
        if (isInitialized) return;

        // Set label if there's a Text component
        Text labelText = GetComponentInChildren<Text>();
        if (labelText != null)
            labelText.text = label;

        // Assign to ToggleGroup
        toggle.group = toggleGroup;

        // Subscribe to toggle changes
        toggle.onValueChanged.AddListener(onValueChanged);
        isInitialized = true;
    }

    /// <summary>
    /// Set toggle state programmatically
    /// </summary>
    public void SetToggleState(bool isOn)
    {
        toggle.isOn = isOn;
    }

    private void OnDestroy()
    {
        if (toggle != null)
            toggle.onValueChanged.RemoveAllListeners();
    }
}