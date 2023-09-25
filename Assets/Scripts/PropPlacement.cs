using System;
using System.Collections.Generic;
using Drawing;
using UnityEngine;
using Random = UnityEngine.Random;

public class PropPlacement : MonoBehaviour {

    public const string savePath = "Assets/Props.save";

    public Camera camera;
    public Color color = Color.yellow;

    public List<Transform> props = new();
    public List<Transform> prefabs = new();
    public Transform prefab;
    public Dictionary<Transform, Transform> previews = new();

    private void Awake() {
        if (prefabs.Count > 0)
            prefab = prefabs[0];
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
                else if (prefab) {
                    var prop = Instantiate(prefab);
                    prop.position = hit.point;
                    prop.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up) * Quaternion.Euler(0, Random.value * 360, 0);
                    props.Add(prop);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab) && prefabs.Count > 0) {
            var index = prefabs.IndexOf(prefab);
            var nextIndex = index == -1 ? 0 : (index + 1).PositiveModulo(prefabs.Count);
            prefab = prefabs[nextIndex];
        }
    }

    public void Save(string path = savePath) { }

    public void TryLoad(string path = savePath) { }
}