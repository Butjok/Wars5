using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

public static class KnownColor {

    public enum Name { Red, Green,Blue,White }

    public static readonly Color32 red = new Color32(255, 87, 17, 255);
    public static readonly Color32 green = new Color32(134, 255, 0, 255);
    public static readonly Color32 blue = new Color32(2, 81, 255, 255);
    public static readonly Color32 white = Color.white;
    
    public static Color32 ToColor32(this Name name) {
        return name switch {
            Name.Red=>red,
            Name.Green=>green,
            Name.Blue=>blue,
            Name.White=>white
        };
    }
    
    public static Name GetName(this Color32 color) {
        bool AreEqual(Color32 a, Color32 b)
            => a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
        
        if (AreEqual(color,red))
            return Name.Red;
        if (AreEqual(color,green))
            return Name.Green;
        if (AreEqual(color,blue))
            return Name.Blue;
        if (AreEqual(color,white))
            return Name.White;

        throw new ArgumentOutOfRangeException(color.ToString());
    }
}