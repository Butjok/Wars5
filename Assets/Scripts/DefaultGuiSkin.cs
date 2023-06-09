using UnityEngine;

public static class DefaultGuiSkin {
    public static GUISkin TryGet => Resources.Load<GUISkin>("CommandLine");
}