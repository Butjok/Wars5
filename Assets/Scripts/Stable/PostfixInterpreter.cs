using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;

public static class PostfixInterpreter {

    public static Dictionary<string, Type> typeCache = new();

    public static void ExecuteToken(this DebugStack stack, string token) {
        
        if (int.TryParse(token, out var intValue))
            stack.Push(intValue);

        else if (float.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out var floatValue))
            stack.Push(floatValue);
        
        else if (token[0]=='#')
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
                    if (!typeCache.TryGetValue(typeName, out var type) || type == null)
                    {
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

                case "load": // obsolete
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

                case "float3": {
                    var z = stack.Pop<dynamic>();
                    var y = stack.Pop<dynamic>();
                    var x = stack.Pop<dynamic>();
                    stack.Push(new Vector3(x, y, z));
                    break;
                }

                default:
                    stack.Push(token.ToString());
                    if (char.IsLower(token[0]))
                        Debug.LogError($"Unrecognized command: {token} - it was pushed on a stack as a string");
                    break;
            }
    }

    public static T Pop<T>(this DebugStack stack) {
        return (T)stack.Pop();
    }
}

public class DebugStack {
		
    public Stack stack = new();
    public Stack<string> stackTrace = new();
		
    public void Push(object value) {
        stack.Push(value);
        //stackTrace.Push(Environment.StackTrace);
    }
    public object Pop() {
        if (stack.Count==0) {
            throw new InvalidOperationException("stack is empty");
        }
        return stack.Pop();
    }
    public object Peek() {
        if (stack.Count==0) {
            throw new InvalidOperationException("stack is empty");
        }
        return stack.Peek();
    }
    public int Count => stack.Count;
}