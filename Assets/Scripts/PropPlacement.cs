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

    [Command] public Vector2 randomRotationRange = new(0, 360);
    [Command] public float rotationStep = 90;
    public float yaw;
    private bool lastRotationWasRandom;
    public float thickness = 2;
    public float arrowLength = .33f;
    public float normalLength = .25f;
    [Command] public bool alignToNormal = true;

    [Command] public float highlightDuration = 3f;
    [Command] public float highlightFrequency = 66;
    [Command] public Color highlightColor = Color.red;

    [Command]
    public void HighlightProps() {
        StartCoroutine(HighlightAnimation());
    }
    public IEnumerator HighlightAnimation() {
        var startTime = Time.time;
        while (Time.time < startTime + highlightDuration) {
            using (Draw.ingame.WithLineWidth(thickness))
                foreach (var prop in props)
                    Draw.ingame.CircleXZ(prop.position, .1f, highlightColor * (Mathf.Sin((Time.time - startTime) * highlightFrequency) + 1) / 2);
            yield return null;
        }
    }

    private void Awake() {
        if (prefabs.Count > 0)
            prefab = prefabs[0];
        TryLoad();
        foreach (var prefab in prefabs) {
            var preview = Instantiate(prefab, transform);
            preview.name = prefab.name + "Preview";
            preview.gameObject.SetActive(false);
            previews.Add(prefab, preview);
        }
    }

    private void OnDisable() {
        foreach (var preview in previews.Values)
            if (preview) // when quitting this may happen
                preview.gameObject.SetActive(false);
        HideSearch();
    }

    public void ShowSearch() {
        searchInput = "";
        justOpened = true;
    }
    public void HideSearch() {
        searchInput = null;
        matches.Clear();
    }

    public void LateUpdate() {

        if (Input.GetKeyDown(KeyCode.P)) {
            ShowSearch();
            return;
        }

        Transform closestProp = null;
        var ray = camera.FixedScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, float.MaxValue, 1 << LayerMask.NameToLayer("Terrain"))) {

            var position = hit.point;
            var rotation = (alignToNormal ? hit.normal : Vector3.up).ToRotation(yaw);
            var scale = Vector3.one;

            var preview = previews[prefab];
            foreach (var item in previews.Values)
                if (item != preview)
                    item.gameObject.SetActive(false);
                else if (!preview.gameObject.activeSelf)
                    preview.gameObject.SetActive(true);

            preview.position = position;
            preview.rotation = rotation;
            preview.localScale = scale;

            foreach (var prop in props) {
                var distance = Vector3.Distance(hit.point, prop.position);
                if (distance > .1f)
                    continue;
                if (!closestProp || distance < Vector3.Distance(hit.point, closestProp.position))
                    closestProp = prop;
            }

            using (Draw.ingame.WithLineWidth(thickness)) {

                //Draw.ingame.Line(hit.point, hit.point + hit.normal * normalLength, color);
                Draw.ingame.Arrow(hit.point, hit.point + rotation * Vector3.forward * arrowLength, color);

                if (closestProp) {
                    Draw.ingame.CircleXZ(closestProp.position, .1f, Color.red);
                    Draw.ingame.Line(hit.point, closestProp.position);
                }
            }

            if (searchInput == null)
                if (prefab && Input.GetMouseButtonDown(Mouse.left)) {
                    if (closestProp)
                        RemoveProp(closestProp);
                    AddProp(prefab, position, rotation, scale);
                    if (lastRotationWasRandom)
                        yaw = RandomYaw;
                }
                else if (closestProp && Input.GetMouseButtonDown(Mouse.right))
                    RemoveProp(closestProp);
        }

        if (Input.GetKeyDown(KeyCode.Tab) && prefabs.Count > 0) {
            var index = prefabs.IndexOf(prefab);
            var nextIndex = index == -1 ? 0 : (index + 1).PositiveModulo(prefabs.Count);
            prefab = prefabs[nextIndex];
        }

        else if (Input.GetKeyDown(KeyCode.R)) {
            yaw = RandomYaw;
            lastRotationWasRandom = true;
        }
        else if (Input.GetKeyDown(KeyCode.F)) {
            var direction = Input.GetKey(KeyCode.LeftShift) ? -1 : 1;
            yaw = (Mathf.Round(yaw / rotationStep) * rotationStep + direction * rotationStep).PositiveModulo(360);
            lastRotationWasRandom = false;
        }
        else if (Input.GetKeyDown(KeyCode.N))
            alignToNormal = !alignToNormal;
        else if (Input.GetKeyDown(KeyCode.H))
            HighlightProps();
    }

    public float RandomYaw => Random.Range(randomRotationRange.x, randomRotationRange.y);

    public void RemoveProp(Transform prop) {
        props.Remove(prop);
        Destroy(prop.gameObject);
    }
    public Transform AddProp(Transform prefab, Vector3 position, Quaternion rotation, Vector3 scale) {
        var prop = Instantiate(prefab, transform);
        prop.position = position;
        prop.rotation = rotation;
        prop.localScale = scale;
        props.Add(prop);
        getPrefab.Add(prop, prefab);
        return prop;
    }

    private string searchInput, oldSearchInput;
    public float searchInputWidth = 500;
    public List<Transform> matches = new();
    private GUIStyle searchInputStyle;
    private GUIContent guiContent = new();
    private bool justOpened = false;

    private void OnGUI() {

        GUI.skin = DefaultGuiSkin.TryGet;

        if (prefab) {
            GUILayout.Label($"Prop: {prefab.name}");
            GUILayout.Space(15);
        }

        GUILayout.Label($"[Tab] Cycle Props");
        GUILayout.Label($"  [F] Snap & Rotate By {Mathf.RoundToInt(Mathf.Abs(rotationStep))}°");
        GUILayout.Label($"  [R] Random Rotation");
        GUILayout.Label($"  [P] Search");
        GUILayout.Label($"  [N] Align To Normal");
        GUILayout.Label($"  [H] Highlight Props");

        /*if (prefab)
            GUILayout.Label($"Prop: {prefab.name}");
        GUILayout.Label($"Rotation: {Mathf.RoundToInt(yaw)} (step: {rotationStep})");*/

        if (searchInput != null) {

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
                HideSearch();

            else {
                // ugly hack because in the CommandLine GUI skin I used transparent text for the text field
                if (searchInputStyle == null) {
                    searchInputStyle = new GUIStyle(GUI.skin.textField);
                    searchInputStyle.normal.textColor = Color.white;
                }

                guiContent.text = searchInput;
                var height = searchInputStyle.CalcHeight(guiContent, searchInputWidth);
                var size = new Vector2(searchInputWidth, height);
                var position = new Vector2(Screen.width, Screen.height) / 2 - size / 2;
                GUI.SetNextControlName("SearchInput");
                oldSearchInput = searchInput;
                searchInput = GUI.TextField(new Rect(position, size), searchInput, searchInputStyle);
                GUI.FocusControl("SearchInput");

                if (justOpened || oldSearchInput != searchInput) {
                    justOpened = false;
                    matches.Clear();
                    foreach (var prefab in prefabs) {
                        if (prefab && searchInput.MatchesFuzzy(prefab.name))
                            matches.Add(prefab);
                    }
                    matches.Sort((a, b) => Levenshtein.Distance(a.name, searchInput) - Levenshtein.Distance(b.name, searchInput));
                    if (matches.Count > 0)
                        prefab = matches[0];
                    if (matches.Count == 1)
                        HideSearch();
                }

                position += new Vector2(0, height);
                foreach (var match in matches) {
                    guiContent.text = match.name;
                    height = GUI.skin.label.CalcHeight(guiContent, searchInputWidth);
                    size = new Vector2(searchInputWidth, height);
                    GUI.Label(new Rect(position, size), guiContent);
                    position += new Vector2(0, height);
                }
            }
        }
    }

    [Command]
    public void ProjectProps() {
        foreach (var prop in props) {
            var ray = new Ray(prop.position + Vector3.up * 100, Vector3.down);
            if (Physics.Raycast(ray, out var hit, float.MaxValue, 1 << LayerMask.NameToLayer("Terrain"))) {
                prop.position = hit.point;
                var forward = Vector3.ProjectOnPlane(prop.forward, hit.normal);
                prop.rotation = Quaternion.LookRotation(forward, hit.normal);
            }
        }
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