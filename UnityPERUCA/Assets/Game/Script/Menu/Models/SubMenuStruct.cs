using System;

public enum SubMenu
{
    GoToAvatarEdit,
    GoToOrganiseGame,
    GoToWindow,
    HowToMove,
    HowToEditAvatar,
    HowToPlayOrganiseGame
}

public class MenuOption
{
    public String title;
    public SubMenu subMenu;

    // Callback that fires when action completes
    private Action onActionInvoked;

    public MenuOption(String title, SubMenu subMenu, Action onActionInvoked = null)
    {
        this.title = title;
        this.subMenu = subMenu;
        this.onActionInvoked = onActionInvoked;
    }

    public void HandleSubMenuClick()
    {
        switch (subMenu)
        {
            case SubMenu.GoToAvatarEdit:
                GoToAvatarEdit();
                break;
            case SubMenu.GoToOrganiseGame:
                GoToOrganiseGame();
                break;
            case SubMenu.HowToMove:
                HowToMove();
                break;
            case SubMenu.HowToPlayOrganiseGame:
                HowToPlayOrganiseGame();
                break;
            case SubMenu.HowToEditAvatar:
                HowToEditAvatar();
                break;
            case SubMenu.GoToWindow:
                GoToWindow();
                break;
        }
    }

    private void GoToAvatarEdit()
    {
       // Implementation for going to avatar edit
        DebugManager.instance.MyLOG("->>>> Navigating to Avatar Edit");
        TeleportManager.Instance.TeleportToAnchor(TeleportPlaces.AvatarEdit, TeleportDirection.Left);
        AvatarManager.instance.UpdateAvatarState(AvatarState.Edit);
        onActionInvoked?.Invoke();
    }

    private void GoToWindow()
    {
       // Implementation for going to window
        DebugManager.instance.MyLOG("->>>> Navigating to Window");
        TeleportManager.Instance.TeleportToAnchor(TeleportPlaces.Window, TeleportDirection.Backward);
        onActionInvoked?.Invoke();
    }

    private void GoToOrganiseGame()
    {
       // Implementation for going to organise game
        DebugManager.instance.MyLOG("->>>> Navigating to Organise Game");
        TeleportManager.Instance.TeleportToAnchor(TeleportPlaces.OrganiseGame, TeleportDirection.Right);
        onActionInvoked?.Invoke();
    }

    private void HowToEditAvatar()
    {
       // Implementation for how to edit avatar
        DebugManager.instance.MyLOG("->>>> Showing How to Edit Avatar");
        DialogueManager.instance.SetDialogueScene(DialogueScene.AvatarEditor, false);
        onActionInvoked?.Invoke();
    }

    private void HowToMove()
    {
       // Implementation for how to move
        DebugManager.instance.MyLOG("->>>> Showing How to Move");
        DialogueManager.instance.SetDialogueScene(DialogueScene.Actions, false);
        onActionInvoked?.Invoke();
    }

    private void HowToPlayOrganiseGame()
    {
       // Implementation for how to play organise game
        DebugManager.instance.MyLOG("->>>> Showing How to Play Organise Game");
        DialogueManager.instance.SetDialogueScene(DialogueScene.OrganiseGame , false);
        onActionInvoked?.Invoke();
    }
}