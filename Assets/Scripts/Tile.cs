using UnityEngine;
using UnityEngine.Assertions;

public enum TileType { Plain }

public abstract class Building
{
    public Level level;
    public Vector2Int position;
    public Player player;
    public int cp=20;

    protected Building(Level level, Vector2Int position, Player player = null) {
        this.level = level;
        this.position = position;
        this.player = player;
    }
}
public class City : Building
{
    public City(Level level, Vector2Int position, Player player = null) : base(level, position, player) { }
}
public class Hq : Building
{
    public Hq(Level level, Vector2Int position, Player player = null) : base(level, position, player) {
        Assert.IsNotNull(player);
    }
}
public static class BuildingSettings
{
    public static int MaxCp(this Building building) => 20;
}