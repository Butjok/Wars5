using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public static class ExtendedPostfix {

    public static string ToPostfix(this string input) {

        input = Regex.Replace(input, @"//.*", " ", RegexOptions.Multiline);
        input = Regex.Replace(input, @"\s+\{", "{", RegexOptions.Multiline);
        input = input.Replace("(", " ( ").Replace(")", " ) ");

        var lists = new Stack<List<string>>();
        lists.Push(new List<string>());

        var prefixes = new Stack<string>();

        var tokens = Regex.Matches(input, @"\s+|\S+", RegexOptions.Singleline);
        foreach (Match token in tokens) {
            var text = token.ToString();

            if (text.EndsWith("{"))
                prefixes.Push(text.Substring(0, text.Length - 1));

            else if (text == "}") {
                Debug.Assert(prefixes.Count > 0);
                prefixes.Pop();
            }

            else if (text == "(")
                lists.Push(new List<string>());

            else if (text == ")") {
                Debug.Assert(lists.Count >= 2);
                var content = lists.Pop();
                var parent = lists.Peek();
                var index = parent.FindLastIndex(t => !string.IsNullOrWhiteSpace(t));
                Debug.Assert(index != -1);
                var command = parent[index];
                parent.RemoveAt(index);
                parent.InsertRange(index, content);
                parent.Add(command);
            }

            else {
                var prefix = string.Join("", prefixes.Reverse());
                if (text.StartsWith("."))
                    text = prefix + text;
                else if (text.StartsWith(":"))
                    text = prefix + ".set-" + text.Substring(1);
                lists.Peek().Add(text);
            }
        }

        var processed = string.Join("", lists.Peek());
        var lines = processed.Split('\n');

        for (var i = 0; i < lines.Length; i++) {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                lines[i] = "";
            else {
                var parts = line.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (char.IsLower(parts[^1][0])) {
                    var left = string.Join(" ", parts.Take(parts.Length - 1));
                    var right = parts[^1];
                    lines[i] = $"{left,-64} {right}";
                }
                else
                    lines[i] = string.Join(" ", parts);
            }
        }
        return string.Join("\n", lines);
    }
}