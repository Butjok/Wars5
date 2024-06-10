using System;
using static Gettext;

public static class TileInfos {
    public static string GetName(TileType tileType) {
        return tileType switch {
            TileType.Plain => _("Plain"),
            TileType.Road => _("Road"),
            TileType.Sea => _("Sea"),
            TileType.Mountain => _("Mountain"),
            TileType.Forest => _("Forest"),
            TileType.River => _("River"),
            TileType.City => _("City"),
            TileType.Hq => _("HQ"),
            TileType.Factory => _("Factory"),
            TileType.Airport => _("Airport"),
            TileType.Shipyard => _("Shipyard"),
            TileType.MissileSilo => _("Missile Silo"),
            TileType.Beach => _("Beach"),
            TileType.Bridge or TileType.BridgeSea => _("Bridge"),
            TileType.MissileStorage => _("Missile Storage"),
            TileType.TunnelEntrance => _("Tunnel Entrance"),
            TileType.PipeSection => _("Pipe"),
            TileType.WindTurbine => _("Wind Turbine"),
            TileType.PowerLineTower => _("Power Line Tower"),
            TileType.OilSilo => _("Oil Silo"),
            TileType.Dam => _("Dam"),
            _ => throw new ArgumentOutOfRangeException(nameof(tileType), tileType, null)
        };
    }
}