using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(BuildingView))]
public class BuildingMaterialSwitcher : MonoBehaviour {

	public Material opaqueMaterial;
	public Material seeThroughMaterial;
	public BuildingView view;

	public void Reset() {
		view = GetComponent<BuildingView>();
	}
	public void Start() {
		view.building = new Building(NewBehaviourScript.level2, Vector2Int.zero);
	}
	public void Update() {
		if (!view || view.renderers.Length == 0 || view.building?.level?.units == null)
			return;
		var shouldBeOpaque = true;
		var units = view.building.level.units;
		foreach (var position in units.positions)
			if (units.TryGetValue(position, out var unit))
				if (unit.view.transform.position.ToVector2().RoundToInt() == view.transform.position.ToVector2().RoundToInt()) {
					shouldBeOpaque = false;
					break;
				}
		var targetMaterial = shouldBeOpaque ? opaqueMaterial : seeThroughMaterial;
		if (view.renderers[0].sharedMaterial != targetMaterial)
			foreach (var renderer in view.renderers)
				renderer.sharedMaterial = targetMaterial;
	}
}