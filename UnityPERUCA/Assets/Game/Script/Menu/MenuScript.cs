using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.XR.Interaction.Toolkit.Samples.Hands;

public class MenuScript : MonoBehaviour
{
    [Header("Content Panel")]
    [SerializeField] private GameObject subMenuPanel;
    [SerializeField] private Text subMenuTitleText;

    [Header("Toggle Groups")]
    [SerializeField] private ToggleGroup mainToggleGroup;

    [Header("Prefabs")]
    [SerializeField] private MenuToggle togglePrefab;
    [SerializeField] private SubMenuButton subMenuButtonPrefab;

    [Header("Content ScrollRects")]
    [SerializeField] private ScrollRect mainMenu;
    
    [SerializeField] private ScrollRect subMenu;

    // Create internal variables
    private Menu[] menus;
    private ToggleGameObject toggleGameObject;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MakeMainMenu();
    }

    // Mark: Init dependencies
    void Awake() {
        GameObject textButtonObject = GameObject.Find("Text Button");
        if (textButtonObject == null)
        {
            Debug.LogError("MenuScript: Could not find GameObject named 'Text Button'. Disabling MenuScript.");
            enabled = false;
            return;
        }
        toggleGameObject = textButtonObject.GetComponent<ToggleGameObject>();
        if (toggleGameObject == null)
        {
            Debug.LogError("MenuScript: 'Text Button' GameObject is missing a ToggleGameObject component. Disabling MenuScript.");
            enabled = false;
            return;
        }
    }

    // MARK: Building Menus
    private Menu[] CreateMenus()
    {
        return new Menu[]
        {
            new Menu("Ação", MainMenu.Teleport, new MenuOption[]
            {
                new MenuOption("Editar Avatar", SubMenu.GoToAvatarEdit, toggleGameObject.ToggleActiveState),
                new MenuOption("Ir ao jogo Organizar", SubMenu.GoToOrganiseGame, toggleGameObject.ToggleActiveState),
                new MenuOption("Ver a janela", SubMenu.GoToWindow, toggleGameObject.ToggleActiveState)
            }),
            new Menu("Ajuda do Avatar", MainMenu.AvatarHelp, new MenuOption[]
            {
                new MenuOption("Como Editar Avatar", SubMenu.HowToEditAvatar, toggleGameObject.ToggleActiveState),
                new MenuOption("Como Mover", SubMenu.HowToMove, toggleGameObject.ToggleActiveState),
                new MenuOption("Como Jogar Organizar Jogo", SubMenu.HowToPlayOrganiseGame, toggleGameObject.ToggleActiveState)
            })
        };
    }

    private void MakeMainMenu()
    {
        menus = CreateMenus();
        foreach (Menu menu in menus)
        {
            // Create button for each menu
            CreateToggleInScroll(menu.title, (isOn) => OnMenuToggleChanged(menu, isOn));
        }
    }

    // MARK: Building UI Elements
    private void CreateToggleInScroll(string label, System.Action<bool> callback)
    {
        // Instantiate from prefab
        MenuToggle menuToggle = Instantiate(togglePrefab, mainMenu.content, false);
        menuToggle.gameObject.name = label;
        
        // Initialize the toggle
        menuToggle.Initialize(label, mainToggleGroup, new UnityEngine.Events.UnityAction<bool>(callback));
    }

    private void CreateSubMenuButton(string label, System.Action callback)
    {
        // Instantiate from prefab
        SubMenuButton subButton = Instantiate(subMenuButtonPrefab, subMenu.content, false);
        subButton.gameObject.name = label;
        
        // Initialize the button
        subButton.Initialize(label, callback);
    }

    // MARK: Menu Callbacks
    private void OnMenuToggleChanged(Menu menu, bool isOn)
    {
        // Clear previous submenu buttons
        foreach (Transform child in subMenu.content)
        {
            Destroy(child.gameObject);
        }

        subMenuPanel.SetActive(isOn);
        if (isOn)
        {
            // Set submenu title
            subMenuTitleText.text = menu.title;
            // Create buttons for each submenu option
            foreach (MenuOption option in menu.subMenus)
            {
                CreateSubMenuButton(option.title, option.HandleSubMenuClick);
            }
        }
    }
}