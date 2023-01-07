using UnityEngine;

public class TargetFpsSetter : MonoBehaviour {
	public int vSynCount = 0;
	public int targetFrameRate = 120;
	public void Awake() {
		DontDestroyOnLoad(gameObject);
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = targetFrameRate;
	}
}