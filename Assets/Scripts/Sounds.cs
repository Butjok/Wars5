using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu(menuName = nameof(Sounds))]
public class Sounds : ScriptableObject {

	private static Sounds instance;
	private static Sounds Instance {
		get {
			if (!instance) {
				instance = Resources.Load<Sounds>(nameof(Sounds));
				Assert.IsTrue(instance);
			}
			return instance;
		}
	}

	public static AudioClip NotAllowed => Instance.notAllowed;
	public static AudioClip Places => Instance.placed;

	[SerializeField] private AudioClip notAllowed;
	[SerializeField] private AudioClip placed;
}