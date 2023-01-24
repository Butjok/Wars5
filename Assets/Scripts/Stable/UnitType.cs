using System;

public enum UnitType {
    Infantry,
    AntiTank,
    Artillery ,
    Apc ,
    // TransportHelicopter = 1 << 4,
    // AttackHelicopter = 1 << 5,
    // FighterJet = 1 << 6,
    // Bomber = 1 << 7,
    Recon ,
    LightTank ,
    Rockets ,
    MediumTank 
}

public enum Team {
    None , 
    Alpha ,
    Bravo , 
    Charlie , 
    Delta 
}

public enum PlayerType { Human, Ai }
public enum AiDifficulty { Normal, Easy, Hard }