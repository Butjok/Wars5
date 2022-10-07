using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public static class PostfixInterpreter {

    public static void Execute(string input, Func<string, Stack<object>, bool> execute = null) {

        if (string.IsNullOrWhiteSpace(input))
            return;

        var tokens = input.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var stack = new Stack<object>();

        foreach (var token in tokens) {

            if (int.TryParse(token, out var intValue))
                stack.Push(intValue);

            else if (float.TryParse(token, out var floatValue))
                stack.Push(floatValue);

            else
                switch (token) {

                    case "true":
                    case "false":
                        stack.Push(token == "true");
                        break;

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
        
        Assert.AreEqual(0, stack.Count);
    }

    public static T Pop<T>(this Stack<object> stack) {
        return (T)stack.Pop();
    }
}