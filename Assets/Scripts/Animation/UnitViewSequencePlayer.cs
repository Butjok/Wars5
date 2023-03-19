using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

[RequireComponent(typeof(UnitView))]
public class UnitViewSequencePlayer : MonoBehaviour {

    public const string runtimeData = "Runtime Data";

    [TextArea(10, 15)]
    public string input = "";
    public bool playOnAwake = false;

    public UnitViewSequencePlayer[] siblings = Array.Empty<UnitViewSequencePlayer>();
    public int index = -1;
    public int shuffledIndex = -1;
    public float speed;
    public float acceleration;
    public float steeringSpeed;
    public UnitView unitView;
    public UnitViewSequenceSubroutines subroutines;
    public Action<UnitViewSequencePlayer> onComplete;

    private bool initialized;
    private void EnsureInitialized() {
        if (initialized)
            return;
        initialized = true;

        unitView = GetComponent<UnitView>();
        //Assert.IsTrue(unitView);

        index = Array.IndexOf(siblings, this);
        Assert.AreNotEqual(-1, index);
    }

    public void Play(BattleView.TargetingSetup targetingSetup, bool shuffle = true) {

        EnsureInitialized();

        if (index == 0) {
            var shuffledSiblings = siblings.Union(new[] { this }).Distinct().OrderBy(item => shuffle ? Random.value : item.index).ToList();
            foreach (var sibling in shuffledSiblings)
                sibling.shuffledIndex = shuffledSiblings.IndexOf(sibling);
        }

        StartCoroutine(Sequence(input, targetingSetup));
    }

    private IEnumerator Sequence(string input, BattleView.TargetingSetup targetingSetup, int level = 0, Stack<object> stack = null) {

        // wait for al the siblings to get indices and shuffled indices
        if (level == 0)
            yield return null;

        if (string.IsNullOrWhiteSpace(input))
            yield break;

        var tokens = input.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (level == 0)
            stack = new Stack<object>();

        var ignore = false;

        foreach (var token in tokens) {

            if (token == "#") {
                ignore = !ignore;
                continue;
            }
            if (ignore)
                continue;

            if (float.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var floatValue))
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
                        var b = (dynamic)stack.Pop();
                        var a = (dynamic)stack.Pop();
                        stack.Push(token switch {
                            "+" => a + b,
                            "-" => a - b,
                            "*" => a * b,
                            "/" => a / b
                        });
                        break;
                    }

                    case "call":
                        if (!subroutines) {
                            subroutines = GetComponent<UnitViewSequenceSubroutines>();
                            Assert.IsTrue(subroutines);
                        }
                        var name = (dynamic)stack.Pop();
                        var subroutine = subroutines.list.Single(item => item.name == name);
                        yield return Sequence(subroutine.text, targetingSetup, level + 1, stack);
                        break;

                    case "spawnPointIndex":
                        Assert.AreNotEqual(-1, index);
                        stack.Push(index);
                        break;

                    case "shuffledIndex":
                        Assert.AreNotEqual(-1, shuffledIndex);
                        stack.Push(shuffledIndex);
                        break;

                    case "random": {
                        var b = (dynamic)stack.Pop();
                        var a = (dynamic)stack.Pop();
                        stack.Push(Random.Range(a, b));
                        break;
                    }

                    case "setSpeed":
                        speed = (float)stack.Pop();
                        break;

                    case "accelerate":
                        acceleration = (dynamic)stack.Pop();
                        yield return new WaitForSeconds((dynamic)stack.Pop());
                        acceleration = 0;
                        break;

                    case "break":
                        acceleration = -Mathf.Sign(speed) * (dynamic)stack.Pop();
                        yield return new WaitForSeconds(Mathf.Abs(speed / acceleration));
                        acceleration = 0;
                        speed = 0;
                        break;

                    case "aim":
                        //unitView.turret.aim = (dynamic)stack.Pop();
                        break;

                    case "shoot":
                        //unitView.turret.Shoot(targetingSetup, (dynamic)stack.Pop());
                        break;

                    case "steer":
                        steeringSpeed = (dynamic)stack.Pop();
                        yield return new WaitForSeconds((dynamic)stack.Pop());
                        steeringSpeed = 0;
                        break;

                    case "wait":
                        yield return new WaitForSeconds((dynamic)stack.Pop());
                        break;

                    case "translate":
                        transform.position += transform.forward * (dynamic)stack.Pop();
                        break;

                    default:
                        stack.Push(token);
                        break;
                }
        }

        if (level == 0) {
            Assert.AreEqual(0, stack.Count);
            onComplete?.Invoke(this);
        }
    }

    private void Awake() {
        EnsureInitialized();
        if (playOnAwake)
            Play(new BattleView.TargetingSetup());
    }

    private void Update() {
        speed += acceleration * Time.deltaTime;
        transform.position += transform.forward * speed * Time.deltaTime;
        var angles = transform.rotation.eulerAngles;
        angles.y += steeringSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(angles);
    }

    [ContextMenu(nameof(DebugSetSiblings))]
    private void DebugSetSiblings() {
        var firstLine = input.Split('\n')[0].Trim();
        var items = FindObjectsOfType<UnitViewSequencePlayer>();
        siblings = items.Where(sibling => sibling.input.StartsWith(firstLine)).ToArray();
    }
}