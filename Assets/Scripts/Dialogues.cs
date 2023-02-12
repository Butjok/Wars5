using System.Globalization;
using System.IO;
using Butjok.CommandLine;
using NGettext;
using UnityEngine;
using static Dialogue;

public static class Dialogues {

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

    [Command]
    public static string Help {
        get {
            var count = Random.Range(0, 10);
            return nata + normal + string.Format(_n("The enemy still has a unit!", "The enemy still has {0} units!", count), count) + next;
        }
    }

    /*
     * END
     */
    
    private static Catalog catalog;
    private static Catalog Catalog {
        get {
            if (catalog == null)
                catalog = new Catalog("Dialogues", Path.Combine(Application.streamingAssetsPath, "Translations"), CultureInfo.GetCultureInfo("ru-RU"));
            return catalog;
        }
    }

    public static string _(string text) => Catalog.GetString(text);
    public static string _(string text, params object[] args) => Catalog.GetString(text, args);
    public static string _n(string text, string pluralText, long n) => Catalog.GetPluralString(text, pluralText, n);
    public static string _n(string text, string pluralText, long n, params object[] args) => Catalog.GetPluralString(text, pluralText, n, args);
    public static string _p(string context, string text) => Catalog.GetParticularString(context, text);
    public static string _p(string context, string text, params object[] args) => Catalog.GetParticularString(context, text, args);
    public static string _pn(string context, string text, string pluralText, long n) => Catalog.GetParticularPluralString(context, text, pluralText, n);
    public static string _pn(string context, string text, string pluralText, long n, params object[] args) => Catalog.GetParticularPluralString(context, text, pluralText, n, args);
}