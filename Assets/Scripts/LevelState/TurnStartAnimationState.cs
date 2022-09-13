using System.Collections;
using UnityEngine;

public class TurnStartAnimationState : State2<Game2> {
	public TurnStartAnimationState(Game2 parent) : base(parent) { }
	public IEnumerator Animation {
		get {
			Debug.Log($"Start of turn #{parent.Turn}");
			yield return new WaitForSeconds(2);
			Debug.Log("GO!");
			UnpauseLastState();
		}
	}
	public override void Start() {
		parent.StartCoroutine(Animation);
	}
}