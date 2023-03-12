using System;
using Butjok.CommandLine;
using UnityEngine;
using static Gettext;

public enum FactionName { Novoslavia, UnitedTreaty }

public static class Factions {

    public static string GetName(FactionName name) => name switch {
        FactionName.Novoslavia => _("People's Republic of Novoslavia"),
        FactionName.UnitedTreaty => _("United Treaty Organization"),
        _ => throw new Exception()
    };
    public static PersonName HeadOfState(FactionName name) => name switch {
        FactionName.Novoslavia => PersonName.LjubisaDragovic,
        FactionName.UnitedTreaty => PersonName.JamesWillis,
        _ => throw new Exception()
    };
    public static string GetDescription(FactionName name) => name switch {
        FactionName.Novoslavia => _("A socialist republic, one of the most military advanced powers in the region."),
        FactionName.UnitedTreaty => _("A large international political, economical and military alliance. The dominant power in the region."),
        _ => throw new Exception()
    };
    public static Sprite TryGetFlag(FactionName name) => name switch {
        FactionName.Novoslavia or FactionName.UnitedTreaty => Resources.Load<Sprite>($"FlagOf{name}"),
        _ => throw new Exception()
    };
    public static Sprite TryGetFlagThumbnail(FactionName name) => name switch {
        FactionName.Novoslavia or FactionName.UnitedTreaty => Resources.Load<Sprite>($"ThumbnailFlagOf{name}"),
        _ => throw new Exception()
    };
    public static Sprite TryGetCoatOfArmsOfGroundForces(FactionName name) => name switch {
        FactionName.Novoslavia or FactionName.UnitedTreaty => Resources.Load<Sprite>($"GroundForcesCoatOfArmsOf{name}"),
        _ => throw new Exception()
    };
    public static Sprite TryGetCoatOfArmsOfAirForces(FactionName name) => null;
    public static Sprite TryGetCoatOfArmsONavalForces(FactionName name) => null;
    public static string GetMotto(FactionName name) => name switch {
        FactionName.Novoslavia => _("For the People and Fatherland"),
        FactionName.UnitedTreaty => _("Solidarity and Unity"),
        _ => throw new Exception()
    };

    [Command(true)]
    public static void Test() {
        foreach (var id in new []{FactionName.Novoslavia,FactionName.UnitedTreaty}) {
            if (!TryGetFlag(id))
                Debug.LogWarning($"faction {id} does not have flag");
            if (!TryGetFlagThumbnail(id))
                Debug.LogWarning($"faction {id} does not have flag thumbnail");
            if (!TryGetCoatOfArmsOfGroundForces(id))
                Debug.LogWarning($"faction {id} does not have a coat of arms of ground forces");
        }
    }
}