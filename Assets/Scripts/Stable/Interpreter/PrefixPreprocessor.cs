using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class PrefixPreprocessor {

    public static string Processed(this string input) {

        input = Regex.Replace(input, @"//.*", " ");
        input = input.Replace("{", " { ").Replace("}", " } ").Replace(";", " ; ");
        if (string.IsNullOrWhiteSpace(input))
            return input;

        var stack = new Stack<(string identifier, List<string> body)>();
        stack.Push((null, new List<string>()));

        var prefixStack = new Stack<string>();
        prefixStack.Push("");

        var lines = input.Split('\n');
        for (var i = 0; i < lines.Length; i++) {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue;
            var tokens = Regex.Split(line.Trim(), @"\s+");

            stack.Peek().body.Add($"// {i + 1}: " + line.TrimEnd());
            foreach (var token in tokens) {
                switch (token) {

                    case "{": {
                        var peeked = stack.TryPeek(out var top);
                        Debug.Assert(peeked);
                        Debug.Assert(top.body.Count > 0);
                        var pivot = top.body[^1];
                        top.body.RemoveAt(top.body.Count - 1);
                        stack.Push((pivot, new List<string>()));
                        break;
                    }

                    case "}": {
                        var popped = stack.TryPop(out var top);
                        Debug.Assert(popped, "Too many closing braces.");
                        var (pivot, body) = top;
                        top = stack.Peek();
                        top.body.AddRange(body);
                        top.body.Add(pivot);
                        break;
                    }

                    case "...":
                    case ".": 
                    case ";":
                        prefixStack.Pop();
                        break;

                    case "..!":
                    case "!":
                        stack.Peek().body.Add(prefixStack.Pop());
                        break;

                    default:
                        if (token.EndsWith("..."))
                            prefixStack.Push(token[..^3]);
                        else if (token.EndsWith(":"))
                            prefixStack.Push(token[..^1]);
                        else if (int.TryParse(token, out _) || float.TryParse(token, out _) || char.IsUpper(token[0]))
                            stack.Peek().body.Add(token);
                        else {
                            var shouldAddPrefix = token[0] is '.' or ':';
                            var command = token.Replace(":", ".set-");
                            stack.Peek().body.Add((shouldAddPrefix ? prefixStack.Peek() : "") + command);
                        }
                        break;
                }
            }
        }

        Debug.Assert(stack.Count == 1, "Unclosed braces.");
        Debug.Assert(stack.Peek().identifier == null);
        Debug.Assert(prefixStack.Count == 1, "Unclosed prefixes.");
        
        return string.Join("\n", stack.Pop().body);
    }
}