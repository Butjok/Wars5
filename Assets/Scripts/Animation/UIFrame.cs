using UnityEngine;
using UnityEngine.Assertions;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class UIFrame : MonoBehaviour {

	public BoxCollider boxCollider;
	public RectTransform rectTransform;

	public void Reset() {
		rectTransform = GetComponent<RectTransform>();
		Assert.IsTrue(rectTransform);
	}

	public void LateUpdate() {

		if (!boxCollider)
			return;
		var camera = Camera.main;
		if (!camera)
			return;

		var min = new Vector2(float.MaxValue, float.MaxValue);
		var max = new Vector2(float.MinValue, float.MinValue);

		var size = boxCollider.size;
		for (var x = -1; x <= 1; x += 2)
		for (var y = -1; y <= 1; y += 2)
		for (var z = -1; z <= 1; z += 2) {

			var localPosition = boxCollider.center + new Vector3(x * size.x / 2, y * size.y / 2, z * size.z / 2);
			var worldPosition = boxCollider.transform.TransformPoint(localPosition);

			var screenPosition = camera.WorldToScreenPoint(worldPosition);

			if (screenPosition.z > 0) {

				min.x = Mathf.Min(min.x, screenPosition.x);
				min.y = Mathf.Min(min.y, screenPosition.y);

				max.x = Mathf.Max(max.x, screenPosition.x);
				max.y = Mathf.Max(max.y, screenPosition.y);
			}
			else
				min = max = new Vector2(float.MinValue, float.MinValue);
		}

		rectTransform.anchoredPosition = min;
		rectTransform.sizeDelta = max - min;
	}
}