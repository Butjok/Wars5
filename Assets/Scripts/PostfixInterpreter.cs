using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.Assertions;

public static class PostfixInterpreter {

    public static string[] Tokenize(this string input) {
        return input.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
    }
    
    public static void ExecuteToken(this Stack stack, string token) {

        if (int.TryParse(token, out var intValue))
            stack.Push(intValue);

        else if (float.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out var floatValue))
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

                case "pop":
                    stack.Pop();
                    break;

                case "type": {
                    var type = Type.GetType(stack.Pop<string>());
                    Assert.IsNotNull(type);
                    stack.Push(type);
                    break;
                }

                case "enum": {
                    var type = stack.Pop<Type>();
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
                    stack.Push(token);
                    if (char.IsLower(token[0]))
                        Debug.Log($"Unrecognized command: {token} - it was pushed on a stack as a string");
                    break;
            }
    }

    public static T Pop<T>(this Stack stack) {
        return (T)stack.Pop();
    }
}