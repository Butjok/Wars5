using System.Globalization;
using System.IO;
using Butjok.CommandLine;
using NGettext;
using UnityEngine;

public static class Dialogues {

    public const string next = " @next ";
    public const string nata = " @nata ";
    public const string pause = " @pause ";
    public const string vlad = " @vlad ";
    public const string happy = " @happy ";
    public const string normal = " @normal ";
    public const string mad = " @mad ";
    public const string shocked = " @shocked ";
    public const string sad = " @sad ";
    public const string laughing = " @laughing ";
    public const string intimate = " @intimate ";
    public const string worried = " @worried ";
    public const string crying = " @crying ";
    public static string Pause(float delay) => $" @{delay:0.00} {pause} ";

    [Command]
    public static string TutorialWelcome =>
        nata + happy + _("Hello there!") + next +
        normal + _("Welcome to the Wars3d! An amazing strategy game!") + next +
        vlad + mad + _("What are you saying?") + next +
        nata + shocked + _("I dont know what to say...") + next +
        crying + _("Probably...") + Pause(1) + _("we should have done something different...") + next +
        vlad + laughing + _("You have no clue who you are messing with!") + next +
        nata + normal + _("Enough!") + next;

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

    private static Catalog catalog;
    private static Catalog Catalog {
        get {
            if (catalog == null)
                catalog = new Catalog("Dialogues", Path.Combine(Application.streamingAssetsPath, "Translations"), CultureInfo.GetCultureInfo("ru-RU"));
            return catalog;
        }
    }

    public static string _(string text) {
        return Catalog.GetString(text);
    }
    public static string _(string text, params object[] args) {
        return Catalog.GetString(text, args);
    }
    public static string _n(string text, string pluralText, long n) {
        return Catalog.GetPluralString(text, pluralText, n);
    }
    public static string _n(string text, string pluralText, long n, params object[] args) {
        return Catalog.GetPluralString(text, pluralText, n, args);
    }
    public static string _p(string context, string text) {
        return Catalog.GetParticularString(context, text);
    }
    public static string _p(string context, string text, params object[] args) {
        return Catalog.GetParticularString(context, text, args);
    }
    public static string _pn(string context, string text, string pluralText, long n) {
        return Catalog.GetParticularPluralString(context, text, pluralText, n);
    }
    public static string _pn(string context, string text, string pluralText, long n, params object[] args) {
        return Catalog.GetParticularPluralString(context, text, pluralText, n, args);
    }
}