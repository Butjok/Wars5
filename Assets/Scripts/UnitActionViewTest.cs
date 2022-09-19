using Cinemachine;
using UnityEngine;
public class UnitActionViewTest : MonoBehaviour {
	public CinemachineVirtualCamera virtualCamera;
	public UnitAction action;
	public void Focus() {
		if (virtualCamera)
			virtualCamera.enabled = true;
	}
	public void Unfocus() {
		if (virtualCamera)
			virtualCamera.enabled = false;
	}
}