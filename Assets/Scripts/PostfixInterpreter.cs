using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public static class PostfixInterpreter {

    public static Stack Execute(string input, Func<string, Stack, bool> execute = null) {

        var stack = new Stack();

        if (string.IsNullOrWhiteSpace(input))
            return stack;

        var tokens = input.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var ignore = false;

        foreach (var token in tokens) {

            if (token == "#") {
                ignore = !ignore;
                continue;
            }

            if (ignore)
                continue;

            if (int.TryParse(token, out var intValue))
                stack.Push(intValue);

            else if (float.TryParse(token, out var floatValue))
                stack.Push(floatValue);

            else
                switch (token) {

                    case "+":
                    case "-":
                    case "*":
                    case "/": {
                        var b = stack.Pop<dynamic>();
                        var a = stack.Pop<dynamic>();
                        stack.Push(token switch {
                            "+" => a + b,
                            "-" => a - b,
                            "*" => a * b,
                            "/" => a / b
                        });
                        break;
                    }

                    case "true":
                    case "false":
                        stack.Push(token == "true");
                        break;

                    case "and":
                    case "or": {
                        var b = stack.Pop<bool>();
                        var a = stack.Pop<bool>();
                        stack.Push(token == "and" ? a && b : a || b);
                        break;
                    }

                    case "not":
                        stack.Push(!stack.Pop<bool>());
                        break;

                    case "dup":
                        stack.Push(stack.Peek());
                        break;

                    case "enum": {

                        var type = Type.GetType(stack.Pop<string>());
                        Assert.IsTrue(type != null);
                        var success = Enum.TryParse(type, stack.Pop<string>(), out var result);
                        Assert.IsTrue(success);

                        stack.Push(result);
                        break;
                    }

                    case "int2": {
                        var y = stack.Pop<int>();
                        var x = stack.Pop<int>();
                        stack.Push(new Vector2Int(x, y));
                        break;
                    }

                    default:
                        var recognized = execute?.Invoke(token, stack);
                        if (recognized != true)
                            stack.Push(token);
                        break;
                }
        }

        return stack;
    }

    public static T Pop<T>(this Stack stack) {
        return (T)stack.Pop();
    }
}