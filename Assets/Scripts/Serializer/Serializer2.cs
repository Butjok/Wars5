using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class Serializer2 {

    public static readonly object skip = null;

    public static TextWriter WriteWord(this TextWriter tw, object value) {
        tw.Write(value);
        tw.Write(' ');
        return tw;
    }
    public static TextWriter WriteWord(this TextWriter tw, object value, string suffix) {
        tw.Write(value);
        tw.Write(suffix);
        tw.Write(' ');
        return tw;
    }

    public static TextWriter WritePlayer(this TextWriter tw, Player p) {
        tw
            .WriteWord(p.color.GetName())
            .WriteWord(p.credits, "cr");
        if (p.IsAi)
            tw.WriteWord(p.difficulty);
        tw.WriteWord(p.type);
        if (p.co)
            tw.WriteWord(p.co.name);
        return tw;
    }
    
    public static TextWriter WriteUnit(this TextWriter tw, Unit u) {
        tw
            .WriteWord(u.type)
            .WriteWord(u.hp.v, "hp")
            .WriteWord("at").WriteWord(u.position.v)
            .WriteWord(u.fuel.v, "fuel");
        if (u.view)
            tw.WriteWord(u.view.transform.rotation.eulerAngles.y, "deg");
        if (u.moved.v)
            tw.WriteWord("moved");
        foreach (var ammo in u.ammo)
            tw.WriteWord(ammo, "ammo");
        if (u.cargo.Count > 0) {
            tw.WriteWord("[");
            foreach (var su in u.cargo)
                tw.WriteUnit(su);
            tw.WriteWord("]");
        }
        return tw;
    }

    public static TextWriter WriteTile(this TextWriter tw, TileType type, IEnumerable<Vector2Int> positions) {
        tw
            .WriteWord(type)
            .WriteWord("at");
        foreach (var position in positions)
            tw.WriteWord(positions);
        return tw;
    }

    public static TextWriter WriteBuilding(this TextWriter tw, Building building) {
        return tw
            .WriteWord(building.type)
            .WriteWord("at").WriteWord(building.position)
            .WriteWord(building.cp, "cp");
    }

    public static TextWriter WriteGameSave(this Main main) {
        //foreach (var position in main.tiles.Keys)
        return null;
    }
}