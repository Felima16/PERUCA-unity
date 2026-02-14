using System;

public enum MainMenu
{
    Teleport,
    AvatarHelp
}

public class Menu
{
    public String title;
    public MainMenu mainMenu;
    public MenuOption[] subMenus;

    public Menu(String title, MainMenu mainMenu, MenuOption[] subMenus)
    {
        this.title = title;
        this.mainMenu = mainMenu;
        this.subMenus = subMenus;
    }
}