using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[RequireComponent(typeof(UnitView))]
public class UnitViewSequencePlayer : MonoBehaviour {

    [FormerlySerializedAs("sequence")] [TextArea(20, 20)]
    public string input = "";

    [Space]
    public float speed;
    public float acceleration;
    public float steeringSpeed;
    public UnitView unitView;

    private bool initialized;
    private void EnsureInitialized() {

        if (initialized)
            return;
        initialized = true;

        unitView = GetComponent<UnitView>();
        Assert.IsTrue(unitView);
    }

    public void Play(int spawnPointIndex = -1, int shuffledIndex = -1, List<ImpactPoint> impactPoints = null) {
        EnsureInitialized();
        StartCoroutine(Execute(spawnPointIndex, shuffledIndex, impactPoints));
    }

    private IEnumerator Execute(int index = -1, int shuffledIndex = -1, List<ImpactPoint> impactPoints = null) {

        if (string.IsNullOrWhiteSpace(input))
            yield break;

        var tokens = input.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var stack = new Stack<float>();

        foreach (var token in tokens) {

            if (float.TryParse(token, out var floatValue))
                stack.Push(floatValue);

            else
                switch (token) {

                    case "+":
                    case "-":
                    case "*":
                    case "/": {
                        var b = stack.Pop();
                        var a = stack.Pop();
                        stack.Push(token switch {
                            "+" => a + b,
                            "-" => a - b,
                            "*" => a * b,
                            "/" => a / b
                        });
                        break;
                    }

                    case "spawnPointIndex":
                        Assert.AreNotEqual(-1, index);
                        stack.Push(index);
                        break;

                    case "shuffledIndex":
                        Assert.AreNotEqual(-1, shuffledIndex);
                        stack.Push(shuffledIndex);
                        break;

                    case "random": {
                        var b = stack.Pop();
                        var a = stack.Pop();
                        stack.Push(Random.Range(a, b));
                        break;
                    }

                    case "setSpeed":
                        speed = stack.Pop();
                        break;

                    case "accelerate":
                        acceleration = stack.Pop();
                        yield return new WaitForSeconds(stack.Pop());
                        acceleration = 0;
                        break;

                    case "break":
                        acceleration = -Mathf.Sign(speed) * stack.Pop();
                        yield return new WaitForSeconds(Mathf.Abs(speed / acceleration));
                        acceleration = 0;
                        speed = 0;
                        break;

                    case "aim":
                        unitView.turret.aim = Random.value <= stack.Pop();
                        break;

                    case "shoot":
                        if (Random.value <= stack.Pop())
                            unitView.turret.Shoot(impactPoints);
                        break;

                    case "steer":
                        steeringSpeed = stack.Pop();
                        yield return new WaitForSeconds(stack.Pop());
                        steeringSpeed = 0;
                        break;

                    case "wait":
                        yield return new WaitForSeconds(stack.Pop());
                        break;

                    case "translate":
                        transform.position += transform.forward * stack.Pop();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(token);
                }
        }

        Assert.AreEqual(0, stack.Count);
    }

    private void Update() {
        speed += acceleration * Time.deltaTime;
        transform.position += transform.forward * speed * Time.deltaTime;
        var angles = transform.rotation.eulerAngles;
        angles.y += steeringSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(angles);
    }
}