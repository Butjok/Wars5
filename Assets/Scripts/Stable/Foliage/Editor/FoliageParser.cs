using System.Linq;
using UnityEditor;
using UnityEngine;

public class FoliageParser : EditorWindow {

	public Transform source;
	public Foliage foliageSource;
	public TransformList target;
	public string prefix = "";
	public float margin = 1;

	[MenuItem("Window/FoliageParser")]
	public static void ShowWindow() {
		GetWindow(typeof(FoliageParser));
	}

	public void OnGUI() {

		source = (Transform)EditorGUILayout.ObjectField("Source", source, typeof(Transform), true);
		foliageSource = (Foliage)EditorGUILayout.ObjectField("FoliageSource", foliageSource, typeof(Foliage), false);
		target = (TransformList)EditorGUILayout.ObjectField("Target", target, typeof(TransformList), false);
		prefix = EditorGUILayout.TextField("Prefix", prefix);
		margin = EditorGUILayout.FloatField("Margin", margin);

		if (GUILayout.Button("Process")) {
			if (target) {

				if (source) {
					var transforms = source.GetComponentsInChildren<Transform>()
						.Where(transform => transform.name.StartsWith(prefix) && transform.GetComponent<Renderer>())
						.ToArray();

					target.matrices = transforms
						.Select(transform => {
							if (transform.parent &&
							    (transform.parent.position != Vector3.zero ||
							     transform.parent.rotation != Quaternion.identity ||
							     transform.parent.localScale != Vector3.one))
								Debug.LogWarning($"Nested transforms are not supported yet.", transform);
							return transform.GetComponent<Renderer>().localToWorldMatrix;
						})
						.ToArray();

					if (transforms.Length > 0) {
						target.bounds = new Bounds();
						foreach (var transform in transforms)
							target.bounds.Encapsulate(transform.position);
						target.bounds.Encapsulate(target.bounds.min - Vector3.one * margin);
						target.bounds.Encapsulate(target.bounds.max + Vector3.one * margin);
					}
				}

				else if (foliageSource) {
					target.matrices = foliageSource.types[0].transforms;
					// TODO: fix this
					target.bounds = new Bounds {
						min = new Vector3(-100, -100, -100),
						max = new Vector3(100, 100, 100),
					};
				}

				else
					Debug.Log("Please specify the source.");

				EditorUtility.SetDirty(target);
			}

			else
				Debug.Log("Please specify the output target.");
		}
	}
}