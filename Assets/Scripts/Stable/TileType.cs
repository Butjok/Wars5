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

    City = 1 << 6,
    Hq = 1 << 7,
    Factory = 1 << 8,
    Airport = 1 << 9,
    Shipyard = 1 << 10,
    
    MissileSilo = 1 << 11,
    
    Buildings = City | Hq | Factory | Airport | Shipyard | MissileSilo
}