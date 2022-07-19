using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.PostProcessing;

public class GlobalPostProcessVolume {
	private static PostProcessVolume instance;
	public static PostProcessVolume Instance {
		get {
			if (!instance) {
				var instances = Object.FindObjectsOfType<PostProcessVolume>().Where(ppv => ppv.CompareTag(nameof(GlobalPostProcessVolume))).ToArray();
				Assert.IsTrue(instances.Length <= 1);
				if (instances.Length == 1)
					instance = instances[0];
				else {
					var go = new GameObject(nameof(GlobalPostProcessVolume)) {
						tag = nameof(GlobalPostProcessVolume)
					};
					instance = go.AddComponent<PostProcessVolume>();
					instance.isGlobal = true;
					instance.weight = 1;
					instance.profile = Resources.Load<PostProcessProfile>(nameof(GlobalPostProcessVolume));
				}
				Object.DontDestroyOnLoad(instance.gameObject);
			}
			return instance;
		}
	}
}