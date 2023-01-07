using System;

[Flags]
public enum UnitType {
    Infantry = 1 << 0,
    AntiTank = 1 << 1,
    Artillery = 1 << 2,
    Apc = 1 << 3,
    TransportHelicopter = 1 << 4,
    AttackHelicopter = 1 << 5,
    FighterJet = 1 << 6,
    Bomber = 1 << 7,
    Recon = 1 << 8,
    LightTank = 1 << 9,
    Rockets = 1 << 10,
}

[Flags]
public enum Team { None = 0, Alpha = 1, Bravo = 2, Charlie = 4, Delta = 8 }
[Flags]
public enum PlayerType { Human, Ai }
public enum AiDifficulty { Normal, Easy, Hard }