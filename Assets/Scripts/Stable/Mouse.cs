using UnityEngine;
using UnityEngine.Assertions;

public static class Mouse
{
    public const int left = 0;
    public const int right = 1;
    public const int middle = 2;
    public const int extra0 = 3;
    public const int extra1 = 4;
    public const int extra2 = 5;

    public static Vector2 viewportPosition = Input.mousePosition / new Vector2(Screen.width,Screen.height);
}