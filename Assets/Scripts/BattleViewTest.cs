using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class BattleViewTest : MonoBehaviour {

	public BattleView battleViewLeft;
	public BattleView battleViewRight;

	public UnitView unitViewPrefab;

	[Range(1, 5)] public int countLeft = 5;
	[Range(1, 5)] public int countRight = 5;

	public Camera mainCamera;
	public float shadowDistance = 10;

	public void Awake() {
		mainCamera = Camera.main;
		Assert.IsTrue(mainCamera);
	}

	public bool visible;
	
	public void Update() {
		
		if (Input.GetKeyDown(KeyCode.Alpha9)) {
			StopAllCoroutines();
			visible = !visible;
			StartCoroutine(visible ? ShowAnimation : HideAnimation);
		}
	}

	public IEnumerator ShowAnimation {
		
		get {

			mainCamera.enabled = false;
			QualitySettings.shadowDistance = shadowDistance;
			
			battleViewLeft.Setup(unitViewPrefab,countLeft);
			battleViewRight.Setup(unitViewPrefab,countRight);
			
			battleViewLeft.AssignTargets(battleViewRight.unitViews);

			// wait for 2 frames to avoid stutter
			yield return null;
			yield return null;

			battleViewLeft.cameraRectDriver.Show();
			battleViewRight.cameraRectDriver.Show();
			
			battleViewLeft.MoveAndShoot();
		}
	}

	public IEnumerator HideAnimation {
		
		get {

			battleViewLeft.cameraRectDriver.Hide();
			battleViewRight.cameraRectDriver.Hide();
			
			battleViewLeft.Cleanup();
			battleViewRight.Cleanup();
			
			mainCamera.enabled = true;
			
			yield break;
		}
	}
}