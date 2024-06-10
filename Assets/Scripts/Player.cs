using System.Collections.Generic;
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

public class Player : ISpawnable {

    public static readonly HashSet<Player> spawned = new();

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
            if (IsSpawned) {
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
        credits = Clamp(value, 0, IsSpawned ? MaxCredits(this) : defaultMaxCredits);
        if (view)
            view.SetCreditsAmount(Credits, animate);
    }

    private int abilityMeter;
    [DontSave] public int AbilityMeter {
        get => abilityMeter;
        set => SetAbilityMeter(value, false, false);
    }
    public void SetAbilityMeter(int value, bool animate = false, bool playSoundOnFull = true) {
        abilityMeter = Clamp(value, 0, IsSpawned ? defaultMaxAbilityMeter : MaxAbilityMeter(this));
        if (view)
            view.SetPowerStripeMeter(value, MaxAbilityMeter(this), IsSpawned && animate, IsSpawned && playSoundOnFull);
    }

    public int? abilityActivationTurn;
    public Vector2Int unitLookDirection = Vector2Int.up;
    public Side side;

    [DontSave] public bool IsSpawned { get; private set; }

    public void Spawn() {
        Assert.IsFalse(IsSpawned);
        Assert.IsFalse(spawned.Contains(this));
        spawned.Add(this);

        //Assert.IsFalse(level.players.Any(player => player.colorName == colorName));
        //level.players.Add(this);

        var viewPrefab = PlayerView2.GetPrefab(coName);
        Assert.IsTrue(viewPrefab);
        view = Object.Instantiate(viewPrefab, level.view.playerUiRoot);
        AbilityMeter = AbilityMeter;
        view.Hide();

        unitBrainController = new UnitBrainController { player = this };
        orderGenerator = new OrderGenerator(this);

        IsSpawned = true;
    }

    public override string ToString() {
        return ColorName.ToString();
    }

    public void Despawn() {
        Assert.IsTrue(spawned.Contains(this));
        spawned.Remove(this);
        if (view && view.gameObject) {
            Object.Destroy(view.gameObject);
            view = null;
        }
        level.players.Remove(this);

        IsSpawned = false;
    }
}