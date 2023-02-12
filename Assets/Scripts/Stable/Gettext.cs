using System.Globalization;
using System.IO;
using NGettext;
using UnityEngine;

public static class Gettext {
    
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