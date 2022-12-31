using System;
using System.Collections;
using System.Globalization;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public class AnimationScript : MonoBehaviour {

    [SerializeField] [TextArea(10, 50)] private string text = "";

    private readonly Stack stack = new();

    [Button()]
    [ContextMenu(nameof(Execute))]
    public void Execute() {
        StartCoroutine(Execute(text));
    }

    private IEnumerator Execute(string text) {

        var tokens = text.Trim().Split(new []{' ', '\r', '\n', '\t'}, StringSplitOptions.RemoveEmptyEntries);

        foreach (var token in tokens) {
            switch (token) {

                case "+":
                case "-":
                case "*":
                case "/":
                    dynamic b = stack.Pop();
                    dynamic a = stack.Pop();

                    stack.Push(token switch {
                        "+" => a + b,
                        "-" => a - b,
                        "*" => a * b,
                        "/" => a / b
                    });
                    break;

                case "Move":

                    Debug.Log("Moving out!");

                    var duration = (dynamic)stack.Pop();
                    var steeringSpeed = (dynamic)stack.Pop();
                    var linearSpeed = (dynamic)stack.Pop();

                    var startTime = Time.time;
                    while (Time.time < startTime + duration) {
                        yield return null;
                        transform.position += transform.forward * linearSpeed * Time.deltaTime;
                        transform.rotation *= Quaternion.Euler(0, steeringSpeed * Time.deltaTime, 0);
                    }

                    Debug.Log("Done!");
                    break;

                case "RandomRange":
                    dynamic high = stack.Pop();
                    dynamic low = stack.Pop();
                    stack.Push(Random.Range(low, high));
                    break;

                default:
                    if (int.TryParse(token, out var intValue))
                        stack.Push(intValue);

                    else if (float.TryParse(token, out var floatValue))
                        stack.Push(floatValue);

                    else
                        Debug.LogError($"Unrecognized token: {token}");

                    break;
            }
        }

        yield return Execute(text);
    }
}