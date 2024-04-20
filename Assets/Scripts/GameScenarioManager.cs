using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using UnityEngine;

public class GameScenarioManager : MonoBehaviour {

    public interface IGameScenario {
        void Execute(Game game);
    }
    
    public class GameScenarioCode : IGameScenario {
        public string code;
        public void Execute(Game game) {
            var stack = new Stack();
            foreach (var token in Tokenizer.Tokenize(code))
                switch (token) {
                    case "find-unit-view-at": {
                        var position = (Vector2Int)stack.Pop();
                        var unitViews = FindObjectsOfType<UnitView>();
                        var unitView = unitViews.FirstOrDefault(unitView => unitView.transform.position.ToVector2Int() == position);
                        stack.Push(unitView);
                        break;
                    }
                    case "move-to": {
                        float speed = (dynamic)stack.Pop();
                        var to = (Vector2Int)stack.Pop();
                        var unitView = (UnitView)stack.Pop();
                        var from = unitView.transform.position.ToVector2Int();
                        var path = new List<Vector2Int> { from };
                        path.AddRange(Woo.Traverse2D(from, to));
                        var animation = unitView.MoveAlong(path, speed:speed);
                        break;
                    }
                    case "spawn-unit": {
                        var position = (Vector2Int)stack.Pop();
                        var unitType = (UnitType)stack.Pop();
                        var playerColor = (ColorName)stack.Pop();
                        var player = game.Level.players.First(player => player.ColorName == playerColor);
                        var unit = new Unit(player, unitType, position:position);
                        break;
                    }
                    default:
                        stack.ExecuteToken(token);
                        break;
                }
        }
    }

    [Command]
    public void ExecuteScenario(string code) {
        var scenario = new GameScenarioCode { code = code };
        scenario.Execute(Game.Instance);
    }
}