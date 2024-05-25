using System;
using System.Collections.Generic;
using System.Linq;
using SaveGame;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
using static UnityEngine.Mathf;
using static Rules;

public enum ColorName {
    Red,
    Green,
    Blue
}
public enum Team {
    None,
    Alpha,
    Bravo,
    Charlie,
    Delta
}
public enum AiDifficulty {
    Normal,
    Easy,
    Hard
}

public class Player : IDisposable {

    public static readonly HashSet<Player> undisposed = new();

    public Level level;
    public Team team;
    public PersonName coName;
    public AiDifficulty? difficulty;
    [DontSave] public bool IsAi => difficulty != null;
    public Vector2Int uiPosition;
    [DontSave] public Zone rootZone;
    [DontSave] public PlayerView2 view;
    [DontSave] public UnitBrainController unitBrainController;
    [DontSave] public OrderGenerator orderGenerator;

    private ColorName colorName;
    [DontSave] public ColorName ColorName {
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

    [DontSave] public Color Color => Colors.Get(ColorName);
    [DontSave] public Color UiColor => Colors.GetUi(ColorName);

    public int maxCredits = defaultMaxCredits;
    private int credits;
    [DontSave] public int Credits {
        get => credits;
        set => SetCredits(value);
    }
    public void SetCredits(int value, bool animate = false) {
        credits = Clamp(value, 0, initialized ? MaxCredits(this) : defaultMaxCredits);
        if (view)
            view.SetCreditsAmount(Credits, animate);
    }

    private int abilityMeter;
    [DontSave] public int AbilityMeter {
        get => abilityMeter;
        set => SetAbilityMeter(value, false, false);
    }
    public void SetAbilityMeter(int value, bool animate = false, bool playSoundOnFull = true) {
        abilityMeter = Clamp(value, 0, initialized ? defaultMaxAbilityMeter : MaxAbilityMeter(this));
        if (view)
            view.SetPowerStripeMeter(value, MaxAbilityMeter(this), initialized && animate, initialized && playSoundOnFull);
    }

    public int? abilityActivationTurn;
    public Vector2Int unitLookDirection = Vector2Int.up;
    public Side side;

    [DontSave] private bool initialized;

    public void Initialize() {
        Assert.IsFalse(initialized);
        Assert.IsFalse(undisposed.Contains(this));
        undisposed.Add(this);

        //Assert.IsFalse(level.players.Any(player => player.colorName == colorName));
        //level.players.Add(this);

        var viewPrefab = PlayerView2.GetPrefab(coName);
        Assert.IsTrue(viewPrefab);
        view = Object.Instantiate(viewPrefab, level.view.playerUiRoot);
        AbilityMeter = AbilityMeter;
        view.Hide();

        unitBrainController = new UnitBrainController { player = this };
        orderGenerator = new OrderGenerator(this);

        initialized = true;
    }

    public override string ToString() {
        return ColorName.ToString();
    }

    public void Dispose() {
        Assert.IsTrue(undisposed.Contains(this));
        undisposed.Remove(this);
        if (view && view.gameObject) {
            Object.Destroy(view.gameObject);
            view = null;
        }
        level.players.Remove(this);

        initialized = false;
    }
}