using UnityEngine;

public class BattleViewTest : MonoBehaviour {

	public BattleView battleViewLeft;
	public BattleView battleViewRight;

	public UnitView unitViewPrefab;

	[Range(1, 5)] public int countLeft = 5;
	[Range(1, 5)] public int countRight = 5;

	public void Awake() {

		battleViewLeft.Setup(unitViewPrefab, countLeft);
		battleViewRight.Setup(unitViewPrefab, countRight);

		battleViewLeft.AssignTargets(battleViewRight.unitViews);
		//battleViewRight.AssignTargets(battleViewRight.unitViews);

		if (battleViewLeft.unitViews.Count > 0) {
			var unitView = battleViewLeft.unitViews[0];
			var manualControl = unitView.GetComponent<ManualControl>();
			if (manualControl)
				manualControl.enabled = true;
		}
	}
}