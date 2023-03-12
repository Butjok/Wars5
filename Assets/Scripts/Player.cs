using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
using static UnityEngine.Mathf;
using static Rules;

public enum ColorName {
    Red, Green, Blue
}

public static class Colors {

    private static Dictionary<ColorName, Color> palette;
    public static Dictionary<ColorName, Color> Palette {
        get {
            if (palette != null)
                return palette;

            palette = new Dictionary<ColorName, Color>();

            var stack = new DebugStack();
            foreach (var token in Tokenizer.Tokenize("Colors".LoadAs<TextAsset>().text))
                stack.ExecuteToken(token);

            while (stack.Count > 0) {
                var b = stack.Pop<dynamic>();
                var g = stack.Pop<dynamic>();
                var r = stack.Pop<dynamic>();
                var name = stack.Pop<ColorName>();
                Assert.IsFalse(palette.ContainsKey(name), name.ToString());
                palette.Add(name, new Color(r, g, b));
            }

#if DEBUG
            foreach (ColorName name in Enum.GetValues(typeof(ColorName)))
                Assert.IsTrue(palette.ContainsKey(name));
#endif

            return palette;
        }
    }

    public static Color Get(ColorName colorName) {
        var found = Palette.TryGetValue(colorName, out var color);
        Assert.IsTrue(found, colorName.ToString());
        return color;
    }
}

public enum Team {
    None,
    Alpha,
    Bravo,
    Charlie,
    Delta
}

public enum PlayerType { Human, Ai }
public enum AiDifficulty { Normal, Easy, Hard }

public class Player : IDisposable {

    public static readonly HashSet<Player> undisposed = new();

    public readonly Level level;
    public readonly Team team;
    public readonly PersonName coName;
    public readonly PlayerType type;
    public readonly AiDifficulty difficulty;
    public readonly PlayerView view;
    public readonly PlayerView2 view2;
    public readonly Vector2Int uiPosition;

    private ColorName colorName;
    public ColorName ColorName {
        get => colorName;
        set {
            colorName = value;
            if (initialized) {
                var color = Colors.Get(colorName);
                foreach (var unit in level.FindUnitsOf(this))
                    RecursivelyUpdateUnitColor(unit, color);
                foreach (var building in level.FindBuildingsOf(this))
                    building.view.PlayerColor = color;
            }
        }
    }
    private void RecursivelyUpdateUnitColor(Unit unit, Color color) {
        if (unit.Player == this && unit.view)
            unit.view.PlayerColor = color;
        foreach (var cargo in unit.Cargo)
            RecursivelyUpdateUnitColor(cargo, color);
    }

    public Color Color => Colors.Get(ColorName);

    public int maxCredits = defaultMaxCredits;
    private int credits;
    public int Credits {
        get => credits;
        set => credits = Clamp(value, 0, initialized ? MaxCredits(this) : defaultMaxCredits);
    }

    private int abilityMeter;
    public int AbilityMeter {
        get => abilityMeter;
        set => abilityMeter = Clamp(value, 0, initialized ? defaultMaxAbilityMeter : MaxAbilityMeter(this));
    }
    public int? abilityActivationTurn;
    public Vector2Int unitLookDirection;
    public int side;

    private bool initialized;

    public Player(Level level, ColorName colorName, Team team = Team.None, int credits = 0, PersonName coName = PersonName.Natalie, PlayerView viewPrefab = null,
        PlayerType type = PlayerType.Human, AiDifficulty difficulty = AiDifficulty.Normal, Vector2Int? unitLookDirection = null, string name = null,
        bool spawnViewPrefab = true, Vector2Int? uiPosition = null) {

        undisposed.Add(this);

        this.level = level;
        this.colorName = colorName;
        this.team = team;
        Credits = credits;
        this.coName = coName;
        this.type = type;
        this.difficulty = difficulty;
        this.unitLookDirection = unitLookDirection ?? Vector2Int.up;
        this.uiPosition = uiPosition ?? new Vector2Int(0, 0);

        Assert.IsFalse(level.players.Any(player => player.colorName == colorName));
        level.players.Add(this);

        if (spawnViewPrefab) {
            viewPrefab = viewPrefab ? viewPrefab : PlayerView.DefaultPrefab;
            Assert.IsTrue(viewPrefab);
            view = Object.Instantiate(viewPrefab, level.transform);
            view.Initialize(this);
            view.visible = false;
        }

        var canvas = level.GetComponentInChildren<Canvas>();
        Assert.IsTrue(canvas);
        view2 = Object.Instantiate(PlayerView2.GetPrefab(coName), canvas.transform);
        view2.player = this;
        view2.Hide();

        initialized = true;
    }

    public override string ToString() {
        return colorName.ToString();
    }

    public void Dispose() {
        Assert.IsTrue(undisposed.Contains(this));
        undisposed.Remove(this);
        if (view && view.gameObject)
            Object.Destroy(view.gameObject);
    }

    public bool IsAi => type == PlayerType.Ai;
}