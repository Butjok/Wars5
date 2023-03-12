using System;
using System.Collections.Generic;
using Butjok.CommandLine;
using UnityEngine;
using static Gettext;

public enum PersonName { Natalie, Vladan, JamesWillis, LjubisaDragovic }
public enum Mood { Normal, Happy, Sad, Mad, Worried, Shocked, Crying, Laughing, Intimate, Nervous }

public static class People {

    public static string GetName(PersonName name) => name switch {
        PersonName.Natalie => _("Natalie Moore"),
        PersonName.Vladan => _("Vladan Raznatovic"),
        PersonName.JamesWillis => _("James G. Willis"),
        PersonName.LjubisaDragovic => _("Ljubisa Dragovic"),
        _ => throw new Exception()
    };

    public static string GetShortName(PersonName name) => name switch {
        PersonName.Natalie => _("Natalie"),
        PersonName.Vladan => _("Vladan"),
        _ => GetName(name)
    };

    public static string GetDescription(PersonName name) => name switch {
        PersonName.Natalie => _("A fresh out of the military academia commanding officer from United Treaty. She was the best student in class."),
        PersonName.Vladan => _("A cold-blooded veteran commanding officer from the Novoslavia. He is well known for the effectiveness and cruelty of his command."),
        PersonName.JamesWillis => _("President of the United Treaty Organization."),
        PersonName.LjubisaDragovic => _("Secretary General of the People's Republic of Novoslavia."),
        _ => throw new Exception()
    };

    public static bool TryGetFaction(PersonName personName, out FactionName factionName) {
        switch (personName) {
            case PersonName.Natalie or PersonName.JamesWillis:
                factionName = FactionName.UnitedTreaty;
                return true;
            case PersonName.Vladan or PersonName.LjubisaDragovic:
                factionName = FactionName.Novoslavia;
                return true;
        }
        factionName = default;
        return false;
    }

    public static Sprite[] GetPhotos(PersonName name) => Resources.LoadAll<Sprite>($"PhotosOf{name}");

    public static Sprite TryGetPortrait(PersonName personName, Mood mood) {
        var portrait = Resources.Load<Sprite>($"{personName}{mood}");
        return portrait ? portrait : TryGetFallbackMood(mood, out var fallbackMood) ? TryGetPortrait(personName, fallbackMood) : null;
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

    public static int GetDialogueSide(PersonName personName) {
        if (!TryGetFaction(personName, out var factionId))
            return 1;
        return factionId switch {
            FactionName.Novoslavia => 1,
            FactionName.UnitedTreaty => -1,
            _ => throw new Exception()
        };
    }

    [Command(true)]
    public static void Test() {
        foreach (var id in new[] { PersonName.Natalie, PersonName.Vladan, PersonName.JamesWillis, PersonName.LjubisaDragovic }) {
            if (!TryGetFaction(id, out var _))
                Debug.LogWarning($"person {id} does not have faction");
            if( !TryGetPortrait(id, Mood.Normal))
                Debug.LogWarning($"person {id} does not have normal portrait");
            if (GetPhotos(id).Length == 0)
                Debug.LogWarning($"person {id} does not have photos");
        }
    }
    
    public static bool IsCo(PersonName personName) {
        return personName is PersonName.Natalie or PersonName.Vladan;
    }
    public static List<AudioClip> GetMusicThemes(PersonName coName) {
        return new List<AudioClip>();
    }
}