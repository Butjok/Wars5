using UnityEngine;
using UnityEngine.Assertions;

public class GameUiView : MonoBehaviour {

	private static GameUiView instance;
	public static GameUiView Instance {
		get {
			if (!instance) {
				instance = FindObjectOfType<GameUiView>();
				Assert.IsTrue(instance);
			}
			return instance;
		}
	}

	public GameObject victory, defeat;
	public GameObject menu;

	public bool Victory {
		set => victory.SetActive(value);
	}
	public bool Defeat {
		set => defeat.SetActive(value);
	}
}