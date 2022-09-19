using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class UnitActionUiContainer : MonoBehaviour {
	
	private UnitActionUiContainer instance;
	public UnitActionUiContainer Instance {
		get {
			if (!instance) {
				instance = FindObjectOfType<UnitActionUiContainer>();
				Assert.IsTrue(instance);
			}
			return instance;
		}
	}

	public List<UnitActionViewTest> views = new();
	
	
}