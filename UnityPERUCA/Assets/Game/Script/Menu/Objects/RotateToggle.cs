using UnityEngine;
using UnityEngine.UI;
using System;

public class RotateToggle : MonoBehaviour
{
    [SerializeField]
    private Image imageUI; 
    private Toggle toggle;
    private bool isInitialized = false;
    private Texture2D playTexture;
    private Texture2D pauseTexture;

    private RotateAroundAxisOnActivate rotateScript;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        if (toggle == null)
        {
            toggle = gameObject.AddComponent<Toggle>();
        }
        Initialize();
        UpdateImage(toggle.isOn);
    }

    /// <summary>
    /// Initialize the toggle with a label and callback
    /// </summary>
    public void Initialize()
    {
        if (isInitialized) return;

        // Subscribe to toggle changes
        toggle.onValueChanged.AddListener(onValueChanged);
        playTexture = Resources.Load<Texture2D>("UI/play");
        pauseTexture = Resources.Load<Texture2D>("UI/pause");
        rotateScript = GameObject.Find("MainCharacters").GetComponent<RotateAroundAxisOnActivate>();;
        isInitialized = true;
    }

    private void UpdateImage(bool isOn)
    {
        if (imageUI == null) return;
        imageUI.sprite = isOn ? GetSprite(pauseTexture) : GetSprite(playTexture);
    }

    private Sprite GetSprite(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    private void OnDestroy()
    {
        if (toggle != null)
            toggle.onValueChanged.RemoveAllListeners();
    }

    private void onValueChanged(bool isOn)
    {
        UpdateImage(isOn);
        rotateScript.ToggleContinuous(isOn);
    }
}