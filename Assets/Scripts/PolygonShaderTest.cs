using System.Collections.Generic;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;

public class PolygonShaderTest : MonoBehaviour {

	public const int pointsCapacity = 128;

	public Material material;
	public Vector2[] from = { };
	public Vector2[] to = { };
	private readonly List<Vector4> list = new(pointsCapacity);

	[Command]
	public void UpdateMaterial() {

		void FillList(IEnumerable<Vector2> points) {
			list.Clear();
			foreach (var point in points)
				list.Add(point);
			var padLength = list.Capacity - list.Count;
			for (var i = 0; i < padLength; i++)
				list.Add(Vector2.zero);
			Assert.AreEqual(pointsCapacity, list.Count);
		}
		
		Assert.IsTrue(material);

		Assert.AreEqual(from.Length, to.Length);
		material.SetInt("_Count", from.Length);
		FillList(from);
		material.SetVectorArray("_From", list);
		FillList(to);
		material.SetVectorArray("_To", list);

		material.SetFloat("_StartTime", Time.timeSinceLevelLoad);
	}
}