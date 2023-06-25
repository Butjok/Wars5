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
    public Zone rootZone;

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
    public int Credits { get; private set; }
    public void SetCredits(int value, bool animate = false) {
        Credits = Clamp(value, 0, initialized ? MaxCredits(this) : defaultMaxCredits);
        if (initialized)
            view2.SetCreditsAmount(Credits, animate);
    }

    public int AbilityMeter { get; private set; }
    public void SetAbilityMeter(int value, bool animate = false, bool playSoundOnFull = true) {
        AbilityMeter = Clamp(value, 0, initialized ? defaultMaxAbilityMeter : MaxAbilityMeter(this));
        view2.SetPowerStripeMeter(value, initialized && animate, initialized && playSoundOnFull);
    }

    public int? abilityActivationTurn;
    public Vector2Int unitLookDirection;
    public int side;

    private bool initialized;

    public Player(Level level, ColorName colorName, Team team = Team.None, int credits = 0, PersonName coName = PersonName.Natalie, PlayerView viewPrefab = null,
        PlayerType type = PlayerType.Human, AiDifficulty difficulty = AiDifficulty.Normal, Vector2Int? unitLookDirection = null,
        bool spawnViewPrefab = true, Vector2Int? uiPosition = null, int abilityMeter = 0, int side = 0, int? abilityActivationTurn = null) {

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
        this.side = side;
        this.abilityActivationTurn = abilityActivationTurn;

        Assert.IsFalse(level.players.Any(player => player.colorName == colorName));
        level.players.Add(this);

        if (spawnViewPrefab) {
            viewPrefab = viewPrefab ? viewPrefab : PlayerView.DefaultPrefab;
            Assert.IsTrue(viewPrefab);
            view = Object.Instantiate(viewPrefab, level.view.transform);
            view.Initialize(this);
            view.visible = false;
        }


        view2 = Object.Instantiate(PlayerView2.GetPrefab(coName), level.view.canvas.transform);
        view2.player = this;
        view2.Hide();

        SetAbilityMeter(abilityMeter, false, false);

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