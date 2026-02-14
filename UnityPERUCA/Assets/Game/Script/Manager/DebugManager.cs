using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugManager : MonoBehaviour
{
    [Header("Setup Debug")]
    [SerializeField]
    private bool enableDebugLogs = true; // Toggle for enabling/disabling debug logs
    [SerializeField]
    private GameObject debugPanel; // Reference to a GameObject that serves as the debug panel

    [Header("UI Debug")]
    [SerializeField]
    private TextMeshProUGUI textArea; // Reference to a TextMeshPro component for displaying debug messages
    [SerializeField]
    private ScrollRect scrollRect; // Reference to a ScrollRect component for scrolling the text area

    // Singleton instance
    public static DebugManager instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject); // Ensure this instance persists across scenes

        textArea.text = ""; // Clear the text area at the start
        debugPanel.SetActive(enableDebugLogs); // Set the debug panel active based on the toggle
    }

    void OnDestroy()
    {
        instance = null;

    }

    public void MyLOG(string message)
    {
        textArea.text += "-> " + message + "\n"; // Append message to the TextMeshPro text area
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
