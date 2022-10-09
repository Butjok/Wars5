using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.VisualScripting;
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
                Play(new[] { TileType.Plain, TileType.Plain }, new[] { lightTank, lightTank }, (new Vector2Int(5, 5), new Vector2Int(4, 2)), true, true);
            }
            else
                Hide(1);
        }
    }

    public void Play(TileType[] tileTypes, UnitView[] unitViewPrefabs, (Vector2Int before, Vector2Int after) count,
        bool move, bool respond) {
        
        Assert.AreEqual(2, tileTypes.Length);
        Assert.AreEqual(2, unitViewPrefabs.Length);
        
        Assert.IsTrue(count.before[left] >= count.after[left]);
        Assert.IsTrue(count.before[right] >= count.after[right]);
        
        StartCoroutine(Animation(tileTypes, unitViewPrefabs, count, move, respond));
    }
    
    private IEnumerator Animation(TileType[] tileTypes, UnitView[] unitViewPrefabs, (Vector2Int before, Vector2Int after) count,
        bool move, bool respond) {
        
        yield return PostProcessing.Fade(fadeColor, fadeDuration, fadeEase).WaitForCompletion();
        if (level)
            level.SetActive(false);

        var operations = new List<AsyncOperation>();
        for (var side = left; side <= right; side++) {
            Assert.IsFalse(battleViews[side], side.ToString());
            var operation = LoadAsync(tileTypes[side], side);
            operations.Add(operation);
        }

        yield return new WaitUntil(() => operations.All(operation => operation.isDone));
        foreach (var operation in operations)
            operation.allowSceneActivation = true;
        LightProbes.Tetrahedralize();

        for (var side = left; side <=right; side++)
            battleViews[side].Setup(unitViewPrefabs[side], count.before[side]);

        var targets = new Dictionary<UnitView, List<UnitView>>[] { new(), new() };
        var survivors = new List<UnitView>[] { new(), new() };

        targets[left] = BattleView.AssignTargets(battleViews[left].unitViews, battleViews[right].unitViews);
        survivors[right] = new List<UnitView>(battleViews[right].unitViews.Take(count.after[right]));
        if (respond) {
            targets[right] = BattleView.AssignTargets(survivors[right], battleViews[left].unitViews);
            survivors[left] = new List<UnitView>(battleViews[left].unitViews.Take(count.after[left]));
        }

        PostProcessing.Fade(Color.white, fadeDuration, fadeEase);

        for (var side = left; side <= right; side++)
            if (cameraRectDrivers[side])
                cameraRectDrivers[side].Show();

        var remaining = 0;
        foreach (var unitView in battleViews[left].unitViews) {
            var sequencePlayer = move ? unitView.moveAndAttack : unitView.attack;
            Assert.IsTrue(sequencePlayer);
            remaining++;
            sequencePlayer.onComplete = _ => remaining--;
            sequencePlayer.Play(targets[left][unitView], survivors[right], true);
        }
        yield return new WaitUntil(() => remaining == 0);

        if (respond) {
            remaining = 0;
            foreach (var unitView in survivors[right]) {
                Assert.IsTrue(unitView.respond);
                remaining++;
                unitView.respond.onComplete = _ => remaining--;
                unitView.respond.Play(targets[right][unitView], survivors[left], true);
            }
            yield return new WaitUntil(() => remaining == 0);
        }
        
    }

    public void Hide(int count) {
        StartCoroutine(HideAnimation());
    }
    private IEnumerator HideAnimation() {

        yield return PostProcessing.Fade(fadeColor, fadeDuration, fadeEase).WaitForCompletion();

        var operations = new List<AsyncOperation>();
        for (var side = left; side <= right; side++) {
            Assert.IsTrue(battleViews[side], side.ToString());
            battleViews[side].Cleanup();
            if (cameraRectDrivers[side])
                cameraRectDrivers[side].Hide();
            operations.Add(UnloadAsync(side));
        }
        yield return new WaitUntil(() => operations.All(operation => operation.isDone));

        PostProcessing.Fade(Color.white, fadeDuration, fadeEase);
        if (level)
            level.SetActive(true);
    }
}