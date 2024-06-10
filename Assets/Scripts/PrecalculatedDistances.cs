using UnityEngine;

public class PrecalculatedDistances : MonoBehaviour {

    /*private static PrecalculatedDistances instance;
    public static PrecalculatedDistances Instance {
        get {
            if (instance == null) {
                var gameObject = new GameObject(nameof(PrecalculatedDistances));
                instance = gameObject.AddComponent<PrecalculatedDistances>();
            }
            return instance;
        }
    }

    [Command]
    public static void Calculate() {
        var level = Game.Instance.Level;
        level.precalculatedDistances = Calculate(level.tiles, level.missionName);
        Write(level.missionName, level.precalculatedDistances);
    }

    public static Dictionary<(MoveType, Vector2Int, Vector2Int), int> Calculate(Dictionary<Vector2Int, TileType> tiles, MissionName missionName) {

        var result = new Dictionary<(MoveType, Vector2Int, Vector2Int), int>();

        var positions = tiles.Keys.ToList();
        var count = tiles.Count;
        int Index(int from, int to) => count * from + to;

        foreach (var moveType in Enum.GetValues(typeof(MoveType)).Cast<MoveType>()) {

            var distances = new int[count * count];
            for (var i = 0; i < count; i++)
            for (var j = 0; j < count; j++)
                distances[Index(i, j)] = i == j
                    ? 0
                    : (positions[i] - positions[j]).ManhattanLength() == 1 && Rules.TryGetMoveCost(moveType, tiles[positions[j]], out var cost)
                        ? cost
                        : infinity;

            for (var k = 0; k < count; k++)
            for (var i = 0; i < count; i++)
            for (var j = 0; j < count; j++)
                distances[Index(i, j)] = Mathf.Min(distances[Index(i, j)], distances[Index(i, k)] + distances[Index(k, j)]);

            for (var i = 0; i < count; i++)
            for (var j = 0; j < count; j++)
                if (distances[Index(i, j)] < infinity)
                    result.Add((moveType, positions[i], positions[j]), distances[Index(i, j)]);
        }

        return result;
    }

    public static void Write(MissionName missionName, Dictionary<(MoveType, Vector2Int, Vector2Int), int> distances) {

        using var output = new FileStream(Path(missionName, ".bin"), FileMode.Create);
        using var writer = new BinaryWriter(output);

        foreach (var ((moveType, from, to), distance) in distances)
            if (distance != infinity) {
                writer.Write((short)moveType);
                writer.Write((short)from.x);
                writer.Write((short)from.y);
                writer.Write((short)to.x);
                writer.Write((short)to.y);
                writer.Write((short)distance);
            }

        Debug.Log($"Saved to: {output.Name}");
    }

    public static Dictionary<(MoveType, Vector2Int, Vector2Int), int> Load(byte[] buffer) {
        var shorts = new short[buffer.Length / sizeof(short)];
        Buffer.BlockCopy(buffer, 0, shorts, 0, buffer.Length);
        var count = shorts.Length / 6;
        var result = new Dictionary<(MoveType, Vector2Int, Vector2Int), int>(count);
        for (var i = 0; i < count; i++) {
            var moveType = (MoveType)shorts[6 * i];
            var fromX = shorts[6 * i + 1];
            var fromY = shorts[6 * i + 2];
            var toX = shorts[6 * i + 3];
            var toY = shorts[6 * i + 4];
            var distance = shorts[6 * i + 5];
            result.Add((moveType, new Vector2Int(fromX, fromY), new Vector2Int(toX, toY)), distance);
        }
        return result;
    }

    public static bool TryLoad(MissionName missionName, out Dictionary<(MoveType, Vector2Int, Vector2Int), int> result) {
        if (Exists(missionName, ".bin")) {
            result = Load(File.ReadAllBytes(Path(missionName, ".bin")));
            return true;
        }
        result = null;
        return false;
    }

    public static string Path(MissionName missionName, string extension) {
        return System.IO.Path.Combine(Application.streamingAssetsPath, "Distances", $"{missionName}{extension}");
    }
    public static bool Exists(MissionName missionName, string extension) {
        return File.Exists(Path(missionName, extension));
    }*/
}