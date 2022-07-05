using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu(menuName = nameof(GameSettings), fileName = nameof(GameSettings))]
public class GameSettings : ScriptableObject {

	private static GameSettings instance;
	public static GameSettings Instance {
		get {
			if (!instance) {
				instance = Resources.Load<GameSettings>(nameof(GameSettings));
				Assert.IsTrue(instance);
			}
			return instance;
		}
	}

	public float unitSpeed = 3;
	public bool showBattleAnimation=true;
}