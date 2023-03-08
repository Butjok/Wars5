using System;
using Butjok.CommandLine;
using static Gettext;
using static Dialogue;

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
    };
    
    public static string GetName(MissionName missionName) {
        return missionName switch {
            MissionName.Tutorial => _p("Campaign.Mission", "Tutorial"),
            MissionName.FirstMission => _p("Campaign.Mission", "First mission"),
            MissionName.SecondMission => _p("Campaign.Mission", "Second mission"),
            _ => throw new ArgumentOutOfRangeException(nameof(missionName), missionName, null)
        };
    }
    
    public static string GetDescription(MissionName missionName) {
        return missionName switch {
            MissionName.Tutorial => _p("Campaign.Mission", "A introduction into the game."),
            MissionName.FirstMission => _p("Campaign.Mission", "A challenger appears!"),
            MissionName.SecondMission => _p("Campaign.Mission", "Final push!"),
            _ => throw new ArgumentOutOfRangeException(nameof(missionName), missionName, null)
        };
    }
    
    [Command]
    public static string TutorialWelcome =>
        nata + happy + _("Hello there!") + next +
        normal + _("Welcome to the Wars3d! An amazing strategy game!") + next +
        vlad + mad + _("What are you saying?") + next +
        nata + shocked + _("I dont know what to say...") + next +
        crying + _("Probably...") + Pause(1) + _("we should have done something different...") + next +
        vlad + laughing + _("You have no clue who you are messing with!") + next +
        nata + normal + _("Enough!") + next +
        nata + _("Let's do it!") + next;

    [Command]
    public static string Victory =>
        nata + happy + _("We did it! They are falling back!") + next +
        vlad + mad + _("This is just a beginning...") + next + _("Troops fall back!") + next +
        nata + happy + _("I am so proud of you!") + next +
        intimate + _("You did a great job.") + next;

    [Command]
    public static string Defeat =>
        nata + worried + _("Oh no...") + next +
        vlad + happy + _("This land is ours!") + Pause(1) + _("Forever!") + next +
        nata + _("Next time we should try different tactic.") + next;
}