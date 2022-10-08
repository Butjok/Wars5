using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class BattleViews : MonoBehaviour {

    public const int left = 0;
    public const int right = 1;

    public BattleView[] battleViews = new BattleView[2];
    public CameraRectDriver[] cameraRectDrivers = new CameraRectDriver[2];

    public GameObject level;
    public Color fadeColor = Color.black;
    public float fadeDuration = .25f;
    public Ease fadeEase = default;

    private static string GetSceneName(TileType tileType, int side) {
        Assert.IsTrue(side is left or right);
        return $"{tileType}{(side == left ? "Left" : "Right")}";
    }

    private AsyncOperation LoadAsync(TileType tileType, int side) {
        Assert.IsFalse(battleViews[side]);
        var sceneName = GetSceneName(tileType, side);
        var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        operation.completed += _ => {
            var scene = SceneManager.GetSceneByName(sceneName);
            var roots = scene.GetRootGameObjects();
            Assert.AreEqual(1, roots.Length);
            var battleView = roots[0].GetComponent<BattleView>();
            Assert.IsTrue(battleView);
            battleViews[side] = battleView;
            cameraRectDrivers[side] = battleView.GetComponent<CameraRectDriver>();
        };
        return operation;
    }

    private AsyncOperation UnloadAsync(int side) {
        var battleView = battleViews[side];
        Assert.IsTrue(battleView);
        var operation = SceneManager.UnloadSceneAsync(battleView.gameObject.scene.name);
        operation.completed += _ => {
            battleViews[side] = null;
            cameraRectDrivers[side] = null;
        };
        return operation;
    }

    public bool visible;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha9)) {
            visible = !visible;
            if (visible) {
                var lightTank = "light-tank".LoadAs<UnitView>();
                Show(new[] { TileType.Plain }, new[] { lightTank }, new Vector2Int(2, 5));
            }
            else {
                Hide(1);
            }
        }
    }

    public void Show(TileType[] tileTypes, UnitView[] unitViewPrefabs, Vector2Int count) {
        StartCoroutine(ShowAnimation(tileTypes, unitViewPrefabs, count));
    }
    private IEnumerator ShowAnimation(TileType[] tileTypes, UnitView[] unitViewPrefabs, Vector2Int count) {

        yield return PostProcessing.Fade(fadeColor, fadeDuration, fadeEase).WaitForCompletion();
        if (level)
            level.SetActive(false);

        var operations = new List<AsyncOperation>();
        for (var side = left; side < tileTypes.Length; side++) {
            Assert.IsFalse(battleViews[side], side.ToString());
            var operation = LoadAsync(tileTypes[side], side);
            operation.allowSceneActivation = true;
            var side1 = side;
            operation.completed += _ => battleViews[side1].Setup(unitViewPrefabs[side1], count[side1]);
            operations.Add(operation);
        }

        yield return new WaitUntil(() => operations.All(operation => operation.isDone));
        LightProbes.Tetrahedralize();

        PostProcessing.Fade(Color.white, fadeDuration, fadeEase);

        for (var side = left; side <= right; side++)
            if (cameraRectDrivers[side])
                cameraRectDrivers[side].Show();

        battleViews[left].MoveAndShoot();
    }

    public void Hide(int count) {
        StartCoroutine(HideAnimation(count));
    }
    private IEnumerator HideAnimation(int count) {

        yield return PostProcessing.Fade(fadeColor, fadeDuration, fadeEase).WaitForCompletion();

        var operations = new List<AsyncOperation>();
        for (var side = left; side < count; side++) {
            Assert.IsTrue(battleViews[side], side.ToString());
            battleViews[side].Cleanup();
            if (cameraRectDrivers[side])
                cameraRectDrivers[side].Hide();
            operations.Add(UnloadAsync(side));
        }
        yield return new WaitUntil(() => operations.All(operation => operation.isDone));

        PostProcessing.Fade(Color.white, fadeDuration, fadeEase);
        if(level)
            level.SetActive(true);
    }
}