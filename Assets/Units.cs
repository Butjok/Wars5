using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class Units : IDisposable {

	public Dictionary<Vector2Int, Unit> at = new();
	public HashSet<Unit> all = new();
	public GameObject go;

	public Units() {
		go = new GameObject(nameof(Units));
		Object.DontDestroyOnLoad(go);
	}

	public void Dispose() {
		Object.Destroy(go);
	}
}