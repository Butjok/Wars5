using static Gettext;

public static class Strings {

    public static string[] All => new[] {
        _p("Campaign.Mission", "Previous"),
        _p("Campaign.Mission", "Next"),
        _p("Campaign.Mission", "Start"),
        _p("Campaign", "Back"),
        _p("MainMenu", "PRESS ANY KEY"),
        _p("MainMenu", "NEW CAMPAIGN"),
        _p("MainMenu", "START MISSION"),
        _p("MainMenu", "CONTINUE"),
        _p("MainMenu", "SETTINGS"),
        _p("MainMenu", "QUIT"),
        _p("MainMenu", "LOAD GAME"),
        _p("MainMenu", "ABOUT"),
        _p("Loading", "START"),
        _p("LoadGame", "Load"),
        _("DAY {0}"),
    };
}