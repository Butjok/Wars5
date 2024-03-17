using UnityEngine;

public static class DefaultGuiSkin {
    public static GUISkin TryGet => Resources.Load<GUISkin>("CommandLine");
    public const int defaultSpacingSize = 10;
    public static readonly Vector2 padding = new(10, 10);
    public const string leftClick = "Left click";
    public const string rightClick = "Right click";
}