using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class PropPlacement : MonoBehaviour {

    public const string savePath = "Assets/Props.save";

    public Camera camera;
    public Color color = Color.yellow;

    public List<Transform> props = new();
    public List<Transform> prefabs = new();
    public Transform prefab;
    public Dictionary<Transform, Transform> previews = new();
    public Dictionary<Transform, Transform> getPrefab = new();

    public bool useRandomRotation = true;
    [Command] public Vector2 randomRotationRange = new(0, 360);
    [Command] public float rotationStep = 90;
    public float rotation;

    private void Awake() {
        if (prefabs.Count > 0)
            prefab = prefabs[0];
        TryLoad();
    }

    public void Update() {

        Transform closestProp = null;
        var ray = camera.FixedScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, float.MaxValue, 1 << LayerMask.NameToLayer("Terrain"))) {

            Draw.ingame.Ray(hit.point, hit.normal, color);

            foreach (var prop in props) {
                var distance = Vector3.Distance(hit.point, prop.position);
                if (distance > .1f)
                    continue;
                if (!closestProp || distance < Vector3.Distance(hit.point, closestProp.position))
                    closestProp = prop;
            }

            if (closestProp) {
                Draw.ingame.CircleXZ(closestProp.position, .1f, Color.red);
                Draw.ingame.Line(hit.point, closestProp.position);
            }

            /*if (prefab) {
                if (!previews.TryGetValue(prefab, out var preview))
                    preview = previews[prefab] = Instantiate(prefab);
            }*/

            if (!closestProp && prefab && Input.GetMouseButtonDown(Mouse.left)) {
                var yaw = useRandomRotation ? Random.Range(randomRotationRange[0], randomRotationRange[1]) : rotation;
                AddProp(prefab, hit.point, Quaternion.LookRotation(Vector3.forward, hit.normal) * Quaternion.Euler(0, yaw, 0), Vector3.one);
            }
            else if (closestProp && Input.GetMouseButtonDown(Mouse.right)) {
                props.Remove(closestProp);
                Destroy(closestProp.gameObject);
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab) && prefabs.Count > 0) {
            var index = prefabs.IndexOf(prefab);
            var nextIndex = index == -1 ? 0 : (index + 1).PositiveModulo(prefabs.Count);
            prefab = prefabs[nextIndex];
        }

        else if (Input.GetKeyDown(KeyCode.Minus))
            rotation = (rotation - rotationStep).PositiveModulo(360);
        else if (Input.GetKeyDown(KeyCode.Equals))
            rotation = (rotation + rotationStep).PositiveModulo(360);

        else if (Input.GetKeyDown(KeyCode.Alpha0))
            useRandomRotation = !useRandomRotation;
        else if (Input.GetKeyDown(KeyCode.R))
            useRandomRotation = true;
        else if (Input.GetKeyDown(KeyCode.F))
            useRandomRotation = false;
    }

    public Transform AddProp(Transform prefab, Vector3 position, Quaternion rotation, Vector3 scale) {
        var prop = Instantiate(prefab);
        prop.position = position;
        prop.rotation = rotation;
        prop.localScale = scale;
        props.Add(prop);
        getPrefab.Add(prop, prefab);
        return prop;
    }

    private void OnGUI() {
        GUI.skin = DefaultGuiSkin.TryGet;
        if (prefab)
            GUILayout.Label($"Prop: {prefab.name}");
        if (useRandomRotation)
            GUILayout.Label("Random Rotation");
        else
            GUILayout.Label($"Rotation: {rotation} (step: {rotationStep})");
    }

    private void OnApplicationQuit() {
        Save();
    }

    public void Save(string path = savePath) {
        var writer = new StringWriter();
        foreach (var prop in props)
            writer.PostfixWriteLine("add ( {0} {1} {2} {3} )", getPrefab[prop].name, prop.position, prop.rotation, prop.localScale);
        File.WriteAllText(path, writer.ToString());
    }

    public bool TryLoad(string path = savePath) {
        if (!File.Exists(path))
            return false;
        var stack = new Stack();
        foreach (var token in Tokenizer.Tokenize(File.ReadAllText(path).ToPostfix()))
            switch (token) {
                case "add": {
                    var scale = (Vector3)stack.Pop();
                    var rotation = (Quaternion)stack.Pop();
                    var position = (Vector3)stack.Pop();
                    var prefabName = (string)stack.Pop();
                    var prefabs = this.prefabs.Where(p => p.name == prefabName).ToList();
                    Assert.IsTrue(prefabs.Count == 1);
                    AddProp(prefabs[0], position, rotation, scale);
                    break;
                }
                default:
                    stack.ExecuteToken(token);
                    break;
            }
        return true;
    }
}