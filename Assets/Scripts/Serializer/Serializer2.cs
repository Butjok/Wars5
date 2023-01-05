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
/*
 
Game set-state

0 set-turn

Players set-state

Alpha Team enum 	    set-team
250 			        set-credits
VladanCo 		        set-co
true 			        set-local
Red ColorName enum 	    add-player 

Bravo Team enum 	    set-team
10000 			        set-credits
Easy AiDifficulty enum 	set-difficulty
NatalieCo 		        set-co
Green ColorName enum 	add-player

Tiles set-state

-5 5 int2 set-start-position

. . . . . R H . . . . . . . .  nl
. . . . . . . . . . . . . . nl
. . . . . . . . . . . . . . nl
. . . . . . . . . . . . . . nl
. . . . . . . . . . . . . . nl
. . . . . . . . . . . . . . nl
. . . . . . . . . G H G F

Units set-state

Infantry UnitType enum 	set-type 
-2 4 int2 		        set-position 
0 -1 int2 		        set-look-direction
R 			            add-unit

Infantry UnitType enum 	set-type 
6 3 int2 			    set-position 
0 -1 int2 		        set-look-direction
R 			            add-unit

Infantry UnitType enum 	set-type 
1 2 int2 			    set-position 
G 			            add-unit

Infantry UnitType enum 	set-type 
2 3 int2 			    set-position 
G 			            add-unit
 
 */