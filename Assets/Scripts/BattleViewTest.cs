using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class BattleViewTest : MonoBehaviour {

	[FormerlySerializedAs("battleViewLeft")] public BattleView left;
	[FormerlySerializedAs("battleViewRight")] public BattleView right;

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

	public void Show() {
		StartCoroutine(ShowAnimation);
	}
	public void Hide() {
		StartCoroutine(HideAnimation);
	}

	private IEnumerator ShowAnimation {
		
		get {

			mainCamera.enabled = false;
			QualitySettings.shadowDistance = shadowDistance;
			
			left.Setup(unitViewPrefab,countLeft);
			right.Setup(unitViewPrefab,countRight);
			
			left.AssignTargets(right.unitViews);

			// wait for 2 frames to avoid stutter
			yield return null;
			yield return null;

			var leftCameraRectDriver = left.GetComponent<CameraRectDriver>();
			if (leftCameraRectDriver)
				leftCameraRectDriver.Show();
			
			var rightCameraRectDriver = right.GetComponent<CameraRectDriver>();
			if(rightCameraRectDriver)
				rightCameraRectDriver.Show();
			
			left.MoveAndShoot();
		}
	}

	private IEnumerator HideAnimation {
		
		get {

			var leftCameraRectDriver = left.GetComponent<CameraRectDriver>();
			if (leftCameraRectDriver)
				leftCameraRectDriver.Hide();
			
			var rightCameraRectDriver = right.GetComponent<CameraRectDriver>();
			if(rightCameraRectDriver)
				rightCameraRectDriver.Hide();
			
			left.Cleanup();
			right.Cleanup();
			
			mainCamera.enabled = true;
			
			yield break;
		}
	}
}