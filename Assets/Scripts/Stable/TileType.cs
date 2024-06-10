using System;

[Flags]
public enum TileType {

    None = 0,
    
    Plain = 1 << 0,
    Road = 1 << 1,
    Sea = 1 << 2,
    Mountain = 1 << 3,
    Forest = 1 << 4,
    River = 1 << 5,
    Beach = 1 << 6,
    Bridge = 1 << 7,
    BridgeSea = 1 << 8,

    City = 1 << 9,
    Hq = 1 << 10,
    Factory = 1 << 11,
    Airport = 1 << 12,
    Shipyard = 1 << 13,
    MissileSilo = 1 << 14,
    
    MissileStorage = 1 << 15,
    TunnelEntrance = 1 << 16,
    PipeSection = 1 << 17,
    WindTurbine = 1 << 18,
    PowerLineTower = 1 << 19,
    OilSilo = 1 << 20,
    Dam = 1 << 21,
    
    Buildings = City | Hq | Factory | Airport | Shipyard | MissileSilo | MissileStorage
}