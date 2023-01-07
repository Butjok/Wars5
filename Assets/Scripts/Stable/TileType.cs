using System;

[Flags]
public enum TileType {

    Plain = 1 << 0,
    Road = 1 << 1,
    Sea = 1 << 2,
    Mountain = 1 << 3,

    City = 1 << 4,
    Hq = 1 << 5,
    Factory = 1 << 6,
    Airport = 1 << 7,
    Shipyard = 1 << 8,
    
    Buildings = City | Hq | Factory | Airport | Shipyard
}