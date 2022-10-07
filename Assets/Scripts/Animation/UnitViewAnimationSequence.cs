using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[RequireComponent(typeof(UnitView))]
public class UnitViewAnimationSequence : MonoBehaviour {

    [TextArea(20, 20)]
    public string sequence = "";

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

    public void Play(int spawnPointIndex, int shuffledIndex, List<ImpactPoint> impactPoints = null) {
        EnsureInitialized();
        StartCoroutine(Sequence(spawnPointIndex, shuffledIndex, impactPoints));
    }

    private IEnumerator Sequence(int spawnPointIndex, int shuffledIndex, List<ImpactPoint> impactPoints) {

        var parts = sequence.Trim().Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var stack = new Stack<float>();

        foreach (var part in parts) {

            if (float.TryParse(part, out var value))
                stack.Push(value);

            else
                switch (part) {

                    case "spawnPointIndex":
                        stack.Push(spawnPointIndex);
                        break;

                    case "shuffledIndex":
                        stack.Push(shuffledIndex);
                        break;

                    case "+":
                    case "-":
                    case "*":
                    case "/":
                    case "random":

                        Assert.IsTrue(stack.Count >= 2);
                        var b = stack.Pop();
                        var a = stack.Pop();

                        stack.Push(part switch {
                            "+" => a + b,
                            "-" => a - b,
                            "*" => a * b,
                            "/" => a / b,
                            "random" => Random.Range(a, b)
                        });
                        break;
                    
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
                        throw new ArgumentOutOfRangeException(part);
                }
        }
    }

    private void Update() {
        speed += acceleration * Time.deltaTime;
        transform.position += transform.forward * speed * Time.deltaTime;
        var angles = transform.rotation.eulerAngles;
        angles.y += steeringSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(angles);
    }
}