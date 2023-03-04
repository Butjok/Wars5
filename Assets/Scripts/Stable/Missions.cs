using System;
using static Gettext;

public enum MissionName { Tutorial,FirstMission,SecondMission }

public static class Missions {

    public static string GetName(MissionName missionName) {
        return missionName switch {
            MissionName.Tutorial => _("Tutorial"),
            MissionName.FirstMission => _("First mission"),
            MissionName.SecondMission => _("Second mission"),
            _ => throw new ArgumentOutOfRangeException(nameof(missionName), missionName, null)
        };
    }
    
    public static string GetDescription(MissionName missionName) {
        return missionName switch {
            MissionName.Tutorial => _("A introduction into the game."),
            MissionName.FirstMission => _("A challenger appears!"),
            MissionName.SecondMission => _("Final push!"),
            _ => throw new ArgumentOutOfRangeException(nameof(missionName), missionName, null)
        };
    }
}