using UnityEngine;
using UnityEngine.UI;

public class SelectedPanelColorToggle : MonoBehaviour
{
    [SerializeField]
    private Toggle toggle;
    [SerializeField]
    public TypeColor typeColor;

    ColorPanel colorPanelScript;

    void Start()
    {
        colorPanelScript = GameObject.Find("ColorButtons").GetComponent<ColorPanel>();
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
        colorPanelScript.SetColorPanel(typeColor);
    }
}
