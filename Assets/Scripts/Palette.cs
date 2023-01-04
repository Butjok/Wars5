using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

public static class KnownColor {

    public static readonly Color32 red = new Color32(255, 87, 17, 255);
    public static readonly Color32 green = new Color32(134, 255, 0, 255);
    public static readonly Color32 blue = new Color32(2, 81, 255, 255);
    public static readonly Color32 white = Color.white;
    
    public static Color32 ToColor32(this ColorName name) {
        return name switch {
            ColorName.Red=>red,
            ColorName.Green=>green,
            ColorName.Blue=>blue,
            ColorName.White=>white
        };
    }
    
    public static ColorName GetName(this Color32 color) {
        bool AreEqual(Color32 a, Color32 b)
            => a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
        
        if (AreEqual(color,red))
            return ColorName.Red;
        if (AreEqual(color,green))
            return ColorName.Green;
        if (AreEqual(color,blue))
            return ColorName.Blue;
        if (AreEqual(color,white))
            return ColorName.White;

        throw new ArgumentOutOfRangeException(color.ToString());
    }
}
public enum ColorName { Red, Green,Blue,White }