public enum UnitType {
    Infantry,
    AntiTank,
    Artillery,
    Apc,
    // TransportHelicopter = 1 << 4,
    // AttackHelicopter = 1 << 5,
    // FighterJet = 1 << 6,
    // Bomber = 1 << 7,
    Recon,
    LightTank,
    Rockets,
    MediumTank,

    TransportHelicopter,
    AttackHelicopter,
    FighterJet,
    Bomber
}

public enum MoveType {
    Foot,
    Tires,
    Tracks,
    Air
}