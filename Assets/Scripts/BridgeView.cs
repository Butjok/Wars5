using System;
using System.Collections;
using Shapes;
using UnityEngine;

public class BridgeView : ImmediateModeShapeDrawer {

	public Bridge bridge;

	public GameObject intact;
	public GameObject destroyed;

	public int hp;
	public int Hp {
		set {
			hp = value;
			if (intact) 
				intact.SetActive(value > 0);
			if (destroyed) 
				destroyed.SetActive(value <= 0);
		}
	}

	public TextElement textElement;
	private void Awake() {
		textElement = new TextElement();
	}
	private void OnDestroy() {
		textElement.Dispose();
	}

	public override void DrawShapes(Camera cam) {
		using (Draw.Command(cam)) {
			//Draw.Text(textElement, transform.position, hp.ToString());
		}
	}

	public IEnumerator DestructionAnimation() {

		if (CameraRig.TryFind(out var cameraRig)) {
			var (min, max) = bridge.tiles.Keys.GetMinMax();
			var center = Vector2.Lerp(min, max, .5f);
			yield return cameraRig.Jump(center.Raycast());
		}
			
		Debug.Log("BRIDGE GETS DESTROYED!!!");

		Hp = 0;
	}
}