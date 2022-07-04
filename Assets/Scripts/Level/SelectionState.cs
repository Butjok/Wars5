using UnityEngine;
using UnityEngine.Assertions;

public static class Masks {
	public static int selectable = 1 << LayerMask.NameToLayer("Selectable");
	public static int terrain = 1 << LayerMask.NameToLayer("Terrain");
}

public static class Mouse {

	public const int left = 0;
	public const int right = 1;
	public const int middle = 2;

	public static Unit Unit {
		get {
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if(!Physics.Raycast(ray, out var hit, float.MaxValue, Masks.selectable))
				return null;
			var unitView = hit.collider.GetComponentInParent<UnitView>();
			if(!unitView)
				return null;
			Assert.IsNotNull(unitView.unit);
			return unitView.unit;
		}
	}
	public static Tile Tile(Level level) {
		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if(!Physics.Raycast(ray, out var hit, float.MaxValue, Masks.terrain))
			return null;
		var position = hit.point.ToVector2().RoundToInt();
		return level.tileAt.TryGetValue(position, out var tile) ? tile : null;
	}
}