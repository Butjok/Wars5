using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

public static class PostfixInterpreter {

    public static string[] Tokenize(this string input) {
        return input.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
    }

    public static DateTime launchTime;
    public static readonly Dictionary<string, Type> typeCache ;
    public static readonly List<Type> allTypes ;

    public static void ExecuteToken(this Stack stack, string token) {

        if (int.TryParse(token, out var intValue))
            stack.Push(intValue);

        else if (float.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out var floatValue))
            stack.Push(floatValue);
        
        else if (token.StartsWith("#"))
            return;

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
                
                case "null":
                    stack.Push(null);
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
                    var typeName = stack.Pop<string>();
                    Type type=null;
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                        type = assembly.GetType(typeName);
                        if (type != null)
                            break;
                    }
                    Assert.IsNotNull(type, typeName);
                    stack.Push(type);
                    break;
                }

                case "find-with-tag": {
                    var tag = stack.Pop<string>();
                    var gameObjects = GameObject.FindGameObjectsWithTag(tag);
                    Assert.AreEqual(1, gameObjects.Length);
                    stack.Push(gameObjects[0]);
                    break;
                }

                case "get-component": {
                    var type = stack.Pop<Type>();
                    var gameObject = stack.Pop<GameObject>();
                    var component = gameObject.GetComponent(type);
                    Assert.IsTrue(component);
                    stack.Push(component);
                    break;
                }

                case "load":
                case "load-resource": {
                    var type = stack.Pop<Type>();
                    var name = stack.Pop<string>();
                    var resource = Resources.Load(name, type);
                    if(!resource)
                        Debug.Log($"resource '{name}' ({type}) was not loaded");
                    stack.Push(resource);
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

    public static T Pop<T>(this IStack stack) {
        return (T)stack.Pop();
    }
}

public interface IStack {
    void Push(object value);
    object Pop();
    int Count { get; }
}