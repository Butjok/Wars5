using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class UnitAttackActionViewTest : MonoBehaviour {

    public UnitAction action;

    private void Awake() {

        var game = Testing.CreateGame(new Testing.Options {
            units = new [] {
                (Color.red, UnitType.Infantry, Vector2Int.zero),
                (Color.blue, UnitType.Infantry, new Vector2Int(2,2)),
            }
        });

        var unit0 = game.FindUnitsOf(game.players[Color.red]).First();
        var unit1 = game.FindUnitsOf(game.players[Color.blue]).First();

        if (unit0.position.v is not { } position)
            throw new Exception();
        
        var path = new MovePath(new[]{ position }, Vector2Int.up);
        action = new UnitAction(UnitActionType.Attack, unit0, path, unit1, weaponIndex:0);
    }
}