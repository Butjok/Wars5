using Butjok.CommandLine;
using UnityEngine;

public class TargetFpsSetter : MonoBehaviour {
	public int vSynCount = 0;
	public int targetFrameRate = 120;
	public void Awake() {
		DontDestroyOnLoad(gameObject);
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = targetFrameRate;
	}
	[Command]
	public int TargetFrameRate {
		get => Application.targetFrameRate;
		set => Application.targetFrameRate = value;
	}
}