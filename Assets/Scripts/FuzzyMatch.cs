using Butjok.CommandLine;
using UnityEngine;

public static class FuzzyMatch {
    [Command]
    public static bool MatchesFuzzy(this string pattern, string input, bool ignoreCase = true) {
        var offset = 0;
        for (var i = 0; i < pattern.Length; i++) {
            var c = pattern[i];

            if (c is '$' or '^')
                continue;

            var index = -1;
            if (ignoreCase) {
                var alternateCase = char.IsLower(c) ? char.ToUpper(c) : char.ToLower(c);
                var index0 = input.IndexOf(c, offset);
                var index1 = input.IndexOf(alternateCase, offset);
                if (index0 != -1 && index1 != -1)
                    index = Mathf.Min(index0, index1);
                else if (index0 != -1)
                    index = index0;
                else if (index1 != -1)
                    index = index1;
            }
            else
                index = input.IndexOf(c, offset);
            if (index == -1)
                return false;
            offset = index + 1;

            if (i - 1 >= 0 && pattern[i - 1] == '^' && index != 0 ||
                i + 1 < pattern.Length && pattern[i + 1] == '$' && index != input.Length - 1)
                return false;
        }
        return true;
    }
}