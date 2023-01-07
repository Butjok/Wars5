using System.Collections.Generic;
using UnityEngine;

public class MovePathTest : MonoBehaviour {
	public MovePathWalker walker;

	[ContextMenu(nameof(Setup))]
	public void Setup() {
		var points = new List<Vector2> { transform.position.ToVector2().RoundToInt() };
		for (var i = 0; i < 2; i++)
			points.Add(new Vector2Int(Random.Range(-3, 3), Random.Range(-3, 3)));
		
		/*points.Clear();
		points.Add(new Vector2(0,0));
		points.Add(new Vector2(5,0));*/
		
		//walker.moves = MovePath.From(points, transform.position.ToVector2().RoundToInt(), transform.forward.ToVector2().RoundToInt());
	}

	public void Update() {
		if (Input.GetKeyDown(KeyCode.Space)) {
			walker.enabled = false;
			Setup();
			walker.enabled = true;
		}
	}
}