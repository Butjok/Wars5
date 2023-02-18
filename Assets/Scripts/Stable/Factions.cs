using System;
using Butjok.CommandLine;
using UnityEngine;
using static Gettext;

public enum FactionId { Novoslavia, UnitedTreaty }

public static class Factions {

    public static string GetName(FactionId id) => id switch {
        FactionId.Novoslavia => _("People's Republic of Novoslavia"),
        FactionId.UnitedTreaty => _("United Treaty Organization"),
        _ => throw new Exception()
    };
    public static PersonId HeadOfState(FactionId id) => id switch {
        FactionId.Novoslavia => PersonId.LjubisaDragovic,
        FactionId.UnitedTreaty => PersonId.JamesWillis,
        _ => throw new Exception()
    };
    public static string GetDescription(FactionId id) => id switch {
        FactionId.Novoslavia => _("A socialist republic, one of the most military advanced powers in the region."),
        FactionId.UnitedTreaty => _("A large international political, economical and military alliance. The dominant power in the region."),
        _ => throw new Exception()
    };
    public static Sprite TryGetFlag(FactionId id) => id switch {
        FactionId.Novoslavia or FactionId.UnitedTreaty => Resources.Load<Sprite>($"FlagOf{id}"),
        _ => throw new Exception()
    };
    public static Sprite TryGetFlagThumbnail(FactionId id) => id switch {
        FactionId.Novoslavia or FactionId.UnitedTreaty => Resources.Load<Sprite>($"ThumbnailFlagOf{id}"),
        _ => throw new Exception()
    };
    public static Sprite TryGetCoatOfArmsOfGroundForces(FactionId id) => id switch {
        FactionId.Novoslavia or FactionId.UnitedTreaty => Resources.Load<Sprite>($"GroundForcesCoatOfArmsOf{id}"),
        _ => throw new Exception()
    };
    public static Sprite TryGetCoatOfArmsOfAirForces(FactionId id) => null;
    public static Sprite TryGetCoatOfArmsONavalForces(FactionId id) => null;
    public static string GetMotto(FactionId id) => id switch {
        FactionId.Novoslavia => _("For the People and Fatherland"),
        FactionId.UnitedTreaty => _("Solidarity and Unity"),
        _ => throw new Exception()
    };

    [Command(true)]
    public static void Test() {
        foreach (var id in new []{FactionId.Novoslavia,FactionId.UnitedTreaty}) {
            if (!TryGetFlag(id))
                Debug.LogWarning($"faction {id} does not have flag");
            if (!TryGetFlagThumbnail(id))
                Debug.LogWarning($"faction {id} does not have flag thumbnail");
            if (!TryGetCoatOfArmsOfGroundForces(id))
                Debug.LogWarning($"faction {id} does not have a coat of arms of ground forces");
        }
    }
}