using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.Assertions;

public static class StringExtensions {
    public static string ToWords(this string str) {
        return Regex.Replace(str, "[a-z][A-Z]", m => m.Value[0] + " " + char.ToLower(m.Value[1]));
    }
    
    public static T ParseEnum<T>(this string text) where T : struct {
        text = text.Replace(" ", "");
        var parsed = Enum.TryParse(text, false, out T value);
        Assert.IsTrue(parsed, text);
        return value;
    }
    public static int ParseInt(this string text) {
        text = text.Replace(" ", "");
        if (text == "inf")
            return int.MaxValue;
        var parsed = int.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var value);
        Assert.IsTrue(parsed, text);
        return value;
    }
    public static bool ParseBool(this string text) {
        text = text.Replace(" ", "");
        Assert.IsTrue(text is "true" or "false");
        return text == "true";
    }
    
    /// <summary>
    /// separate string with separators: ' ' ',' ':',
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static IEnumerable<string> Separate(this string text) {
        if (string.IsNullOrWhiteSpace(text))
            return Enumerable.Empty<string>();
        return text.Split(new[] { ' ', ',', ':' , ';'}, StringSplitOptions.RemoveEmptyEntries);
    }
}