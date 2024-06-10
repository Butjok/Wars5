using UnityEngine;

public class UnitAttackActionViewTest : MonoBehaviour {

    public UnitAction action;

    private void Awake() {

        // var game = Testing.CreateGame(new Testing.Options {
        //     units = new [] {
        //         (Color.red, UnitType.Infantry, Vector2Int.zero),
        //         (Color.blue, UnitType.Infantry, new Vector2Int(2,2)),
        //     }
        // });
        //
        // var unit0 = game.FindUnitsOf(game.players[0]).First();
        // var unit1 = game.FindUnitsOf(game.players[1]).First();
        //
        // if (unit0.Position is not { } position)
        //     throw new Exception();
        //
        // action = new UnitAction(UnitActionType.Attack, unit0, new[]{ position }, unit1);
    }
}