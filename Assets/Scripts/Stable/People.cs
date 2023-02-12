using System;
using UnityEngine;
using static Gettext;

public enum PersonId { Natalie, Vladan, JamesWillis, LjubisaDragovic }
public enum Mood { Normal, Happy, Sad, Mad, Worried, Shocked, Crying, Laughing, Intimate }

public static class People {

    public static string GetName(PersonId id) => id switch {
        PersonId.Natalie => _("Natalie Moore"),
        PersonId.Vladan => _("Vladan Raznatovic"),
        PersonId.JamesWillis => _("James G. Willis"),
        PersonId.LjubisaDragovic => _("Ljubisa Dragovic"),
        _ => throw new Exception()
    };

    public static string GetShortName(PersonId id) => id switch {
        PersonId.Natalie => _("Natalie"),
        PersonId.Vladan => _("Vladan"),
        _ => GetName(id)
    };

    public static string GetDescription(PersonId id) => id switch {
        PersonId.Natalie => _("A fresh out of the military academia commanding officer from United Treaty. She was the best student in class."),
        PersonId.Vladan => _("A cold-blooded veteran commanding officer from the Novoslavia. He is well known for the effectiveness and cruelty of his command."),
        PersonId.JamesWillis => _("President of the United Treaty Organization."),
        PersonId.LjubisaDragovic => _("Secretary General of the People's Republic of Novoslavia."),
        _ => throw new Exception()
    };

    public static bool TryGetFaction(PersonId personId, out FactionId factionId) {
        switch (personId) {
            case PersonId.Natalie or PersonId.JamesWillis:
                factionId = FactionId.UnitedTreaty;
                return true;
            case PersonId.Vladan or PersonId.LjubisaDragovic:
                factionId = FactionId.Novoslavia;
                return true;
        }
        factionId = default;
        return false;
    }

    public static Sprite[] GetPhotos(PersonId id) => Resources.LoadAll<Sprite>($"PhotosOf{id}");
    
    public static Sprite TryGetPortrait(PersonId personId, Mood mood) {
        var portrait = Resources.Load<Sprite>($"{personId}{mood}");
        return portrait ? portrait : TryGetFallbackMood(mood, out var fallbackMood) ? TryGetPortrait(personId, fallbackMood) : null;
    }

    public static bool TryGetFallbackMood(Mood mood, out Mood fallback) {
        Mood? result = mood switch {
            Mood.Normal => null,
            Mood.Laughing or Mood.Intimate => Mood.Happy,
            Mood.Crying => Mood.Sad,
            Mood.Shocked => Mood.Worried,
            _ => Mood.Normal
        };
        fallback = result is { } value ? value : default;
        return result != null;
    }

    public static int GetDialogueSide(PersonId personId) {
        if (!TryGetFaction(personId, out var factionId))
            return 1;
        return factionId switch {
            FactionId.Novoslavia => 1,
            FactionId.UnitedTreaty => -1,
            _ => throw new Exception()
        };
    }
}