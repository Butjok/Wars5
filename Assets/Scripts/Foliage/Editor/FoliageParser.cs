using System.Linq;
using UnityEditor;
using UnityEngine;

public class FoliageParser : EditorWindow {

	public Transform source;
	public TransformList target;
	public string prefix = "";
	public float margin = 1;

	[MenuItem("Window/FoliageParser")]
	public static void ShowWindow() {
		GetWindow(typeof(FoliageParser));
	}

	public void OnGUI() {

		source = (Transform)EditorGUILayout.ObjectField("Source", source, typeof(Transform), false);
		target = (TransformList)EditorGUILayout.ObjectField("Target", target, typeof(TransformList), false);
		prefix = EditorGUILayout.TextField("Prefix", prefix);
		margin = EditorGUILayout.FloatField("Margin", margin);

		if (GUILayout.Button("Process")) {
			if (source && target) {
				
				var transforms = source.GetComponentsInChildren<Transform>()
					.Where(transform => transform.name.StartsWith(prefix))
					.ToArray();
				target.matrices = transforms
					.Select(transform => transform.GetComponent<Renderer>().localToWorldMatrix)
					.ToArray();

				if (transforms.Length > 0) {
					var bounds = new Bounds();
					foreach (var transform in transforms)
						bounds.Encapsulate(transform.position);
					bounds.Encapsulate(bounds.min - Vector3.one * margin);
					bounds.Encapsulate(bounds.max + Vector3.one * margin);

					target.min = bounds.min;
					target.max = bounds.max;
				}
			}
			else {
				if (!source)
					Debug.Log("Please specify the source transform.");
				if (!target)
					Debug.Log("Please specify the output target.");
			}
		}
	}
}