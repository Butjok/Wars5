using UnityEngine;

public static class LayerMasks {
	public static int Selectable => 1 << Layers.Selectable;
	public static int Terrain => 1 << Layers.Terrain;
	public static int Roads => 1 << Layers.Roads;
	public static int Water => 1 << Layers.Water;
}

public static class Layers {
	
	public static int Selectable => LayerMask.NameToLayer("Selectable");
	public static int Terrain => LayerMask.NameToLayer("Terrain");
	public static int Roads => LayerMask.NameToLayer("Roads");
	public static int Water => LayerMask.NameToLayer("Water");

	public static int Player0 => LayerMask.NameToLayer("Player0");
	public static int Player1 => LayerMask.NameToLayer("Player1");
	public static int Player2 => LayerMask.NameToLayer("Player2");
	public static int Player3 => LayerMask.NameToLayer("Player3");
}