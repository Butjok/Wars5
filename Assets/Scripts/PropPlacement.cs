using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using Stable;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class PropPlacement : MonoBehaviour {

    public const string defaultLoadOnAwakeFileName = "Autosave";

    public Camera camera;
    public Color color = Color.yellow;

    public List<Transform> props = new();
    public List<PrefabInfo> prefabInfos = new();
    public Transform prefab;

    [Command] public Vector2 randomRotationRange = new(0, 360);
    [Command] public float rotationStep = 90;
    public float yaw;
    public float thickness = 2;
    public float arrowLength = .33f;
    public float normalLength = .25f;
    [Command] public bool alignToNormal = true;

    [Command] public float highlightDuration = 3f;
    [Command] public float highlightFrequency = 66;
    [Command] public Color highlightColor = Color.red;

    [Header("Startup")]
    public bool loadOnAwake = true;
    [FormerlySerializedAs("loadOnAwake")] public string loadOnAwakeFileName = defaultLoadOnAwakeFileName;
    public float searchInputWidth = 500;
    public List<Transform> matches = new();
    public Dictionary<Transform, Transform> getPrefab = new();
    private readonly GUIContent guiContent = new();
    private bool justOpened;
    private bool lastRotationWasRandom;
    public Dictionary<Transform, Transform> previews = new();

    private string searchInput, oldSearchInput;
    private GUIStyle searchInputStyle;

    public float RandomYaw => Random.Range(randomRotationRange.x, randomRotationRange.y);

    private void Awake() {
        if (prefabInfos.Count > 0)
            SelectPrefab(prefabInfos[0]);

        if (loadOnAwake)
            TryLoad(loadOnAwakeFileName);

        foreach (var prefabInfo in prefabInfos) {
            var prefab = prefabInfo.prefab;
            var preview = Instantiate(prefab, transform);
            preview.name = prefab.name + "Preview";
            preview.gameObject.SetActive(false);
            previews.Add(prefab, preview);
        }
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

            var preview = previews[prefab];
            foreach (var item in previews.Values)
                if (item != preview)
                    item.gameObject.SetActive(false);
                else if (!preview.gameObject.activeSelf)
                    preview.gameObject.SetActive(true);

            preview.position = position;
            preview.rotation = rotation;

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
                    AddProp(prefab, position, rotation, prefab.localScale);
                    if (lastRotationWasRandom)
                        yaw = RandomYaw;
                }
                else if (closestProp && Input.GetMouseButtonDown(Mouse.right)) {
                    RemoveProp(closestProp);
                }
        }

        if (Input.GetKeyDown(KeyCode.Tab) && prefabInfos.Count > 0) {
            var index = prefabInfos.FindIndex(info => info.prefab == prefab);
            var nextIndex = index == -1 ? 0 : (index + 1).PositiveModulo(prefabInfos.Count);
            SelectPrefab(prefabInfos[nextIndex]);
        }

        else if (Input.GetKeyDown(KeyCode.R)) {
            RotateRandomly();
        }
        else if (Input.GetKeyDown(KeyCode.F)) {
            CycleFixedRotation();
        }
        else if (Input.GetKeyDown(KeyCode.N)) {
            alignToNormal = !alignToNormal;
        }
        else if (Input.GetKeyDown(KeyCode.H)) {
            HighlightProps();
        }
    }

    private void OnDisable() {
        foreach (var preview in previews.Values)
            if (preview) // when quitting this may happen
                preview.gameObject.SetActive(false);
        HideSearch();
    }

    private void OnGUI() {

        GUI.skin = DefaultGuiSkin.TryGet;
        GUILayout.Label("Prop placement");
        GUILayout.Space(DefaultGuiSkin.defaultSpacingSize);

        if (prefab) {
            GUILayout.Label($"Prop: {prefab.name}");
            GUILayout.Space(DefaultGuiSkin.defaultSpacingSize);
        }

        GUILayout.Label($"[Tab] Cycle Props");
        GUILayout.Label($"  [F] Snap & Rotate By {Mathf.RoundToInt(Mathf.Abs(rotationStep))}°");
        GUILayout.Label($"  [R] Random Rotation");
        GUILayout.Label($"  [P] Search");
        GUILayout.Label($"  [N] {(alignToNormal ? "Unalign" : "Align")} To Terrain");
        GUILayout.Label($"  [H] Highlight Props");

        /*if (prefab)
            GUILayout.Label($"Prop: {prefab.name}");
        GUILayout.Label($"Rotation: {Mathf.RoundToInt(yaw)} (step: {rotationStep})");*/

        if (searchInput != null) {

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape) {
                HideSearch();
            }

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
                    foreach (var prefab in prefabInfos.Select(i => i.prefab))
                        if (prefab && searchInput.MatchesFuzzy(prefab.name))
                            matches.Add(prefab);
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
    public void HighlightProps() {
        StartCoroutine(HighlightAnimation());
    }
    public IEnumerator HighlightAnimation() {
        var startTime = Time.time;
        while (Time.time < startTime + highlightDuration) {
            using (Draw.ingame.WithLineWidth(thickness)) {
                foreach (var prop in props)
                    Draw.ingame.CircleXZ(prop.position, .1f, highlightColor * (Mathf.Sin((Time.time - startTime) * highlightFrequency) + 1) / 2);
            }
            yield return null;
        }
    }

    [Command]
    public void Clear() {
        foreach (var prop in props.ToList())
            RemoveProp(prop);
    }

    public void ShowSearch() {
        searchInput = "";
        justOpened = true;
    }
    public void HideSearch() {
        searchInput = null;
        matches.Clear();
    }

    public void CycleFixedRotation() {
        var direction = Input.GetKey(KeyCode.LeftShift) ? -1 : 1;
        yaw = (Mathf.Round(yaw / rotationStep) * rotationStep + direction * rotationStep).PositiveModulo(360);
        lastRotationWasRandom = false;
    }
    public void RotateRandomly() {
        yaw = RandomYaw;
        lastRotationWasRandom = true;
    }
    public void SelectPrefab(PrefabInfo prefabInfo) {
        prefab = prefabInfo.prefab;
        alignToNormal = prefabInfo.defaultAlignToNormal;
        if (prefabInfo.defaultRotation == PrefabInfo.RotationType.Random)
            RotateRandomly();
        else
            CycleFixedRotation();
    }

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

    [Command]
    public void ProjectProps() {
        foreach (var prop in props) {
            var ray = new Ray(prop.position + Vector3.up * 100, Vector3.down);
            if (Physics.Raycast(ray, out var hit, float.MaxValue, 1 << LayerMask.NameToLayer("Terrain"))) {
                if (prop.name.Contains("Flag"))
                    hit.normal = Vector3.up;
                prop.position = hit.point;
                var forward = Vector3.ProjectOnPlane(prop.forward, hit.normal);
                prop.rotation = Quaternion.LookRotation(forward, hit.normal);
            }
        }
    }

    [Command]
    public void Save() {
        Save(loadOnAwakeFileName);
    }
    
    public void Save(string name) {
        var writer = new StringWriter();
        foreach (var prop in props)
            writer.PostfixWriteLine("add ( {0} {1} {2} {3} )", getPrefab[prop].name, prop.position, prop.rotation, prop.localScale);
        LevelEditorFileSystem.Save(name, writer.ToString());
    }

    public bool TryLoad(string name) {
        var input = LevelEditorFileSystem.TryReadLatest(name);
        if (input == null)
            return false;
        var stack = new Stack();
        Clear();
        foreach (var token in Tokenizer.Tokenize(input.ToPostfix()))
            switch (token) {
                case "add": {
                    var scale = (Vector3)stack.Pop();
                    var rotation = (Quaternion)stack.Pop();
                    var position = (Vector3)stack.Pop();
                    var prefabName = (string)stack.Pop();
                    var prefabInfos = this.prefabInfos.Where(i => i.prefab.name == prefabName).ToList();
                    Assert.IsTrue(prefabInfos.Count == 1);
                    AddProp(prefabInfos[0].prefab, position, rotation, scale);
                    break;
                }
                default:
                    stack.ExecuteToken(token);
                    break;
            }
        return true;
    }

    [Serializable]
    public class PrefabInfo {
        public enum RotationType { Random, Fixed }
        public Transform prefab;
        public RotationType defaultRotation = RotationType.Random;
        public bool defaultAlignToNormal = true;
    }
}