using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI Dialogue")]
    [SerializeField]
    private Text displayDialogText; // Reference to a Text component for displaying dialogue text
    [SerializeField]
    private Image displayDialogImage; // Reference to an Image component for displaying dialogue images (if any)

    [Header("Dialogue Options")]
    [SerializeField]
    private Button[] optionButtons; // Array of buttons for dialogue options
    [SerializeField]
    private Button finishButton; // Button to finish the dialogue

    [Header("UI Panels")]
    [SerializeField]
    private GameObject buttonsPanel; // Reference to a GameObject that serves as the button panel
    [SerializeField]
    private GameObject textPanel; // Reference to a GameObject that serves as the text panel
    [SerializeField]
    private GameObject imagePanel; // Reference to a GameObject that serves as the image panel

    // Singleton instance
    public static DialogueManager instance;

    // Current dialogue state
    private DialogueScene scene;
    private Dictionary<DialogueScene, List<DialogueCase>> dialogueCases = new Dictionary<DialogueScene, List<DialogueCase>>();
    private List<DialogueCase> currentDialogueScene = new List<DialogueCase>();
    private DialogueCase currentDialogueCase;

    // To track previous game state to return after dialogue
    private AvatarState previousAvatarState = AvatarState.Edit;

    private bool isFirstTimeOrganiseGame = true;
    private bool isFirstTimeAction = true;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        dialogueCases = SceneManager.Instance.GetScenes();
        finishButton.gameObject.SetActive(false);
        finishButton.onClick.AddListener(() => FinishDialogue());
        SetupUI(); // Setup UI for dialogue options
        SetDialogueScene(DialogueScene.AvatarEditor, true); // Start with AvatarEditor scene and onboarding
    }

    void OnDestroy()
    {
        instance = null;
        finishButton.onClick.RemoveListener(() => FinishDialogue());
        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i; // Capture the current index for the button click event
            optionButtons[i].onClick.RemoveListener(() => OnOptionSelected(index));
        }
    }

    // Method to handle dialogue cases
    private void SetupUI()
    {
        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i; // Capture the current index for the button click event
            optionButtons[i].onClick.AddListener(() => OnOptionSelected(index));
        }
    }

    // Method to set the dialogue sequence based on the current scene
    public void SetDialogueScene(DialogueScene newScene, bool isOnboarding = false)
    {
        bool isActionsOnboarding = newScene == DialogueScene.Actions && isOnboarding;
        previousAvatarState = isActionsOnboarding ? AvatarState.Game : AvatarManager.instance.state;
        AvatarManager.instance.UpdateAvatarState(AvatarState.Help);
        
        scene = newScene;
        currentDialogueScene = dialogueCases[scene];

        DebugManager.instance.MyLOG("**** Setting dialogue scene: " + scene.ToString() + " with " + currentDialogueScene.Count + " cases.");

        String startIndex = isOnboarding ? "onBoardingStart" : "start";

        SetNextDialogueCase(startIndex); // Start with the first dialogue case
        buttonsPanel.SetActive(true);
        textPanel.SetActive(true);
    }

    public void VerifyShouldShowOrganiseGameDialogue()
    {
        if (isFirstTimeOrganiseGame)
        {
            isFirstTimeOrganiseGame = false;
            SetDialogueScene(DialogueScene.OrganiseGame, true);
        }
    }

    public bool VerifyShouldShowActionsDialogue()
    {
        if (isFirstTimeAction)
        {
            isFirstTimeAction = false;
            SetDialogueScene(DialogueScene.Actions, true);
            return true;
        }
        return false;
    }

    private void SetNextDialogueCase(String index)
    {
        currentDialogueCase = currentDialogueScene.Find(dc => dc.id == index);
        if (currentDialogueCase == null)
        {
            FinishDialogue(); // If no case found, finish the dialogue
            return;
        }
        
        displayDialogText.text = currentDialogueCase.text;
        
        // Load image from Resources
        if (!string.IsNullOrEmpty(currentDialogueCase.image))
        {
            try
            {
                Texture2D texture = Resources.Load<Texture2D>(currentDialogueCase.image);
                if (texture != null)
                {
                    displayDialogImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    imagePanel.SetActive(true);
                }
                else
                {
                    Debug.LogWarning("Image not found at path: " + currentDialogueCase.image);
                    displayDialogImage.sprite = null;
                    imagePanel.SetActive(false);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error loading image: " + ex.Message);
                displayDialogImage.sprite = null;
                imagePanel.SetActive(false);
            }
        }
        else
        {
            displayDialogImage.sprite = null;
            imagePanel.SetActive(false);
        }

        SetButtons();
    }

    private void SetButtons()
    {       
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < currentDialogueCase.options.Length)
            {
                optionButtons[i].gameObject.SetActive(true);
                optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = currentDialogueCase.options[i].title;
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }

        if (currentDialogueCase.options == null || currentDialogueCase.options.Length == 0)
        {
            finishButton.gameObject.SetActive(true);
            finishButton.GetComponentInChildren<TextMeshProUGUI>().text = "Finalizar";
        }
    }

    private void FinishDialogue()
    {
        // Logic to finish the dialogue, e.g., hide UI, reset state, etc.
        buttonsPanel.SetActive(false);
        textPanel.SetActive(false);
        finishButton.gameObject.SetActive(false);
        displayDialogText.text = string.Empty;
        currentDialogueScene = null;
        currentDialogueCase = null;
        AvatarManager.instance.UpdateAvatarState(previousAvatarState);
    }

    private void OnOptionSelected(int optionIndex)
    {
        if (currentDialogueCase == null || optionIndex < 0 || optionIndex >= currentDialogueCase.options.Length)
            return;

        // Logic to handle the selected option
        String nextSceneIndex = currentDialogueCase.options[optionIndex].nextSceneIndex;
        SetNextDialogueCase(nextSceneIndex);
    }
}
