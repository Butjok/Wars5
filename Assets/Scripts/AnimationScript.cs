using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class AnimationScript : MonoBehaviour {

    [SerializeField] [TextArea(10, 50)] private string text = "";
    [SerializeField] private float linearAcceleration = 1;
    [SerializeField] private float steerAcceleration = 90;

    private readonly Stack stack = new();

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space))
            Animate();
    }

    [ContextMenu(nameof(Animate))]
    public void Animate() {
        StartCoroutine(Execute());
    }

    private IEnumerator Execute() {

        var tokens = text.Trim().Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var token in tokens) {
            switch (token) {

                case "+":
                case "-":
                case "*":
                case "/":
                case "<":
                case "<=":
                case ">":
                case ">=": {
                    dynamic b = stack.Pop();
                    dynamic a = stack.Pop();
                    stack.Push(token switch {
                        "+" => a + b,
                        "-" => a - b,
                        "*" => a * b,
                        "/" => a / b,
                        "<" => a < b,
                        "<=" => a <= b,
                        ">" => a > b,
                        ">=" => a >= b
                    });
                    break;
                }

                case "true":
                case "false": {
                    stack.Push(token == "true");
                    break;
                }
                
                case "RandomValue": {
                    stack.Push(Random.value);
                    break;
                }

                case "RandomRange": {
                    dynamic high = stack.Pop();
                    dynamic low = stack.Pop();
                    stack.Push(Random.Range(low, high));
                    break;
                }
                
                case "Restart": {
                    yield return Execute();
                    yield break;
                }
                
                case "Move": {
                    float offset = (dynamic)stack.Pop();
                    var duration = Mathf.Sqrt( Mathf.Abs(offset) / linearAcceleration);
                    var targetPosition = transform.position + transform.forward * offset;
                    yield return transform
                        .DOMove(targetPosition, duration)
                        .SetEase(Ease.InOutQuad)
                        .WaitForCompletion();
                    break;
                }

                case "Rotate": {
                    float angleDelta = (dynamic)stack.Pop();
                    var startAngle = transform.rotation.eulerAngles.y;
                    var targetAngle = startAngle + angleDelta;
                    var duration = Mathf.Sqrt(Mathf.Abs(angleDelta) / steerAcceleration);
                    yield return transform
                        .DORotate(new Vector3(0, targetAngle, 0), duration)
                        .SetEase(Ease.InOutQuad)
                        .WaitForCompletion();
                    break;
                }

                case "Wait": {
                    float duration = (dynamic)stack.Pop();
                    yield return new WaitForSeconds(duration);
                    break;
                }

                case "SetAim": {
                        GetComponent<UnitView>().turret.aim = (bool)stack.Pop();
                    break;
                }

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
    }
}