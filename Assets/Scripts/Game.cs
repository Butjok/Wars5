using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.PostProcessing;

public class Game : MonoBehaviour {

    public Map2D<Unit> units;
    public Map2D<TileType> tiles;
    public Map2D<Building> buildings;
    public List<Player> players = new();
    public int? turn;
    public DefaultLevelLogic levelLogic;
    public Player realPlayer;
    public GameSettings settings;

    public InputCommandsContext input = new();

    private bool initialized;
    private void EnsureInitialized() {
        if (initialized)
            return;
        initialized = true;
        settings = new GameSettings();
    }
    
    public void Awake() {
        EnsureInitialized();
        UpdatePostProcessing();
    }

    public void UpdatePostProcessing() {
        EnsureInitialized();
        PostProcessing.Setup(
            settings.antiAliasing,
            settings.motionBlurShutterAngle,
            settings.enableBloom,
            settings.enableScreenSpaceReflections,
            settings.enableAmbientOcclusion);
    }

    public Player CurrentPlayer {
        get {
            EnsureInitialized();
            Assert.AreNotEqual(0, players.Count);
            Assert.IsTrue(turn != null);
            return players[(int)turn % players.Count];
        }
    }

    public bool TryGetTile(Vector2Int position, out TileType tile) {
        EnsureInitialized();
        return tiles.TryGetValue(position, out tile) && tile != 0;
    }
    public bool TryGetUnit(Vector2Int position, out Unit unit) {
        EnsureInitialized();
        return units.TryGetValue(position, out unit) && unit != null;
    }
    public bool TryGetBuilding(Vector2Int position, out Building building) {
        EnsureInitialized();
        return buildings.TryGetValue(position, out building) && building != null;
    }

    public IEnumerable<Unit> FindUnitsOf(Player player) {
        EnsureInitialized();
        return units.Values.Where(unit => unit.player == player);
    }
    public IEnumerable<Building> FindBuildingsOf(Player player) {
        EnsureInitialized();
        return buildings.Values.Where(building => building.player.v == player);
    }

    public IEnumerable<Vector2Int> AttackPositions(Vector2Int position, Vector2Int range) {
        EnsureInitialized();
        return range.Offsets().Select(offset => offset + position).Where(p => tiles.ContainsKey(p));
    }
}