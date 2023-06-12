using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public static class PostfixInterpreter {

    public static Dictionary<string, Type> typeCache = new();

    [Command]
    public static string Format(object value) {
        switch (value) {
            case int v:
                return v.ToString(CultureInfo.InvariantCulture);
            case float v:
                return v.ToString(CultureInfo.InvariantCulture);
            case bool v:
                return v ? "true" : "false";
            case null:
                return "null";
            case Type v:
                return $"{v} type";
            case Enum v:
                return $"{v} {Format(v.GetType())} enum";
            case Vector2 v:
                return $"{Format(v.x)} {Format(v.y)} float2";
            case Vector3 v:
                return $"{Format(v.x)} {Format(v.y)} {Format(v.z)} float3";
            case Vector2Int v:
                return $"{Format(v.x)} {Format(v.y)} int2";
            case Vector3Int v:
                return $"{Format(v.x)} {Format(v.y)} {Format(v.z)} int3";
            case Color v:
                return $"{Format(v.r)} {Format(v.g)} {Format(v.b)} {Format(v.a)} color";
            case string v:
                Assert.AreNotEqual(0,v.Length);
                Assert.IsTrue(char.IsUpper(v[0]));
                Assert.IsFalse(v.Any(char.IsWhiteSpace));
                return v;
            default:
                throw new ArgumentOutOfRangeException(value.ToString());
        }
    }

    public static T Pop<T>(this Stack stack) {
        return (T)stack.Pop();
    }
    public static void ExecuteToken(this Stack stack, string token) {

        if (int.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out var intValue))
            stack.Push(intValue);

        else if (float.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out var floatValue))
            stack.Push(floatValue);

        else if (token[0] == '#')
            return;

        else
            switch (token) {

                case "+":
                case "-":
                case "*":
                case "/": {
                    var b = stack.Pop<dynamic>();
                    var a = stack.Pop<dynamic>();
                    stack.Push(token[0] switch {
                        '+' => a + b,
                        '-' => a - b,
                        '*' => a * b,
                        '/' => a / b
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
                    var b = stack.Pop<dynamic>();
                    var a = stack.Pop<dynamic>();
                    stack.Push(token == "and" ? a && b : a || b);
                    break;
                }

                case "not":
                    stack.Push(!stack.Pop<dynamic>());
                    break;

                case "dup":
                    stack.Push(stack.Peek());
                    break;

                case "pop":
                    stack.Pop();
                    break;

                case "type": {
                    var typeName = stack.Pop<string>();
                    if (!typeCache.TryGetValue(typeName, out var type) || type == null) {
                        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                            type = assembly.GetType(typeName);
                            if (type != null)
                                break;
                        }
                        Assert.IsNotNull(type, typeName);
                        typeCache[typeName] = type;
                    }
                    stack.Push(type);
                    break;
                }
                
                case "random": {
                    var b = stack.Pop<dynamic>();
                    var a = stack.Pop<dynamic>();
                    stack.Push(Random.Range(a, b));
                    break;
                }

                case "find-with-tag": {
                    var tag = stack.Pop<string>();
                    var gameObjects = GameObject.FindGameObjectsWithTag(tag);
                    Assert.AreEqual(1, gameObjects.Length);
                    stack.Push(gameObjects[0]);
                    break;
                }

                case "find-single-object-of-type": {
                    var includeInactive = stack.Pop<bool>();
                    var type = stack.Pop<Type>();
                    var objects = Object.FindObjectsOfType(type, includeInactive);
                    Assert.AreEqual(1, objects.Length, $"not a single object of type {type} is found, found: {objects.Length}");
                    stack.Push(objects[0]);
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

                case "load": // obsolete
                case "load-resource": {
                    var type = stack.Pop<Type>();
                    var name = stack.Pop<string>();
                    var resource = Resources.Load(name, type);
                    if (!resource)
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

                case "float3": {
                    var z = stack.Pop<dynamic>();
                    var y = stack.Pop<dynamic>();
                    var x = stack.Pop<dynamic>();
                    stack.Push(new Vector3(x, y, z));
                    break;
                }

                case "rgb":
                case "rgba": {
                    var a = token == "rgba" ? stack.Pop<dynamic>() : 1; 
                    var b = stack.Pop<dynamic>();
                    var g = stack.Pop<dynamic>();
                    var r = stack.Pop<dynamic>();
                    stack.Push(new Color(r,g,b,a));
                    break;
                }

                case "throw-exception": {
                    throw new Exception(stack.Pop<string>());
                }

                case "log": {
                    Debug.Log(stack.Pop());
                    break;
                }

                default:
                    stack.Push(token);
                    if (char.IsLower(token[0]))
                        Debug.LogWarning($"Unrecognized command: {token} - it was pushed on a stack as a string");
                    break;
            }
    }
}
