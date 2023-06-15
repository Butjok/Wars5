using System.Linq;
using Butjok.CommandLine;
using UnityEngine;

public enum UnitStateType {
    Staying, Joining, Healing, Attacking, Capturing, Supplying
}

public class Ai {

    public Game game;
    public Level level;

    public Ai(Game game, Level level) {
        this.game = game;
        this.level = level;
    }

    [Command]
    public static void ShowDistanceField(int cap = 3) {
        var game = Game.Instance;
        var level = Game.Instance.stateMachine.Find<LevelEditorSessionState>().level;
        var obstacles = DistanceField.FindObstacles(level, TileType.Mountain | TileType.River | TileType.Sea);
        var distanceField = DistanceField.Calculate(obstacles, level);
        var cappedDistanceField = distanceField.Map(a => Mathf.Min(a, cap)); 
        game.StartCoroutine(DebugDraw.DrawUntilKey(KeyCode.Alpha9, () => DebugDraw.DistanceField(cappedDistanceField, DebugDraw.ColorDirection.DarkToLight)));
    }

    [Command]
    public static void ShowUnitInfluence() {
        var game = Game.Instance;
        var level = Game.Instance.stateMachine.Find<LevelEditorSessionState>().level;
        if (level.view.cameraRig.camera.TryGetMousePosition(out Vector2Int position) && level.TryGetUnit(position, out var unit)) {
            var field = DistanceField.CalculateUnitInfluence(unit);
            game.StartCoroutine(DebugDraw.DrawUntilKey(KeyCode.Alpha9, () => DebugDraw.DistanceField(field, DebugDraw.ColorDirection.DarkToLight)));
        }
    }
    
    [Command]
    public static void ShowUnitChokeField() {
        var game = Game.Instance;
        var level = Game.Instance.stateMachine.Find<LevelEditorSessionState>().level;
        var pathFinder = new PathFinder();
        if (level.view.cameraRig.camera.TryGetMousePosition(out Vector2Int position) && level.TryGetUnit(position, out var unit)) {
            
            var obstacles = DistanceField.FindObstacles(level, TileType.Mountain | TileType.River | TileType.Sea);
            obstacles.UnionWith(level.units.Keys.Where(position => level.units[position].Player != unit.Player));
            var chokeField = DistanceField.Calculate(obstacles, level);
            game.StartCoroutine(DebugDraw.DrawUntilKey(KeyCode.Alpha9, () => DebugDraw.DistanceField(chokeField, DebugDraw.ColorDirection.DarkToLight)));
        }
    }

    [Command]
    public static void ShowVision() {
        var game = Game.Instance;
        var level = Game.Instance.stateMachine.Find<LevelEditorSessionState>().level;
        if (level.view.cameraRig.camera.TryGetMousePosition(out Vector2Int position) && level.TryGetUnit(position, out var unit)) {
            var vision = Vision.Calculate(unit.Player);
            game.StartCoroutine(DebugDraw.DrawUntilKey(KeyCode.Alpha9, () => DebugDraw.DistanceField(vision.ToDictionary(p=>p,p=>1f), DebugDraw.ColorDirection.DarkToLight)));
        }
    }
}