using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            if (Input.GetKeyDown(KeyCode.P)) {
                if (closestProp) {
                    props.Remove(closestProp);
                    Destroy(closestProp.gameObject);
                }
                else if (prefab)
                    AddProp(prefab, hit.point, Quaternion.LookRotation(Vector3.forward, hit.normal) * Quaternion.Euler(0, Random.value * 360, 0), Vector3.one);
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab) && prefabs.Count > 0) {
            var index = prefabs.IndexOf(prefab);
            var nextIndex = index == -1 ? 0 : (index + 1).PositiveModulo(prefabs.Count);
            prefab = prefabs[nextIndex];
        }
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