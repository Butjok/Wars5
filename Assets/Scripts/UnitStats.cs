using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Butjok.CommandLine;
using NReco.Csv;
using UnityEngine;
using UnityEngine.Assertions;

public struct UnitStats {

    [Flags]
    public enum SpecialCommand {
        CanCapture = 1 << 0,
        CanLaunchMissile = 1 << 1,
        CanSupply = 1 << 2
    }

    public MoveCostType moveCostType;
    public int moveCapacity;
    public int fuel;
    public Dictionary<WeaponName, int> ammo;
    public Vector2Int attackRange;
    public int cost;
    public int vision;
    public SpecialCommand specialCommands;
    public int carryCapacity;
    public int weight;
    public HashSet<UnitType> canCarry;
    public int unloadCost;
    public int unloadCapacity;

    private static Dictionary<UnitType, UnitStats> loaded;
    public static Dictionary<UnitType, UnitStats> Loaded {
        get {
            if (loaded == null)
                Load();
            return loaded;
        }
    }

    [Command]
    public static void Load() {

        var input = "UnitStats".LoadAs<TextAsset>().text;

        if (loaded == null)
            loaded = new Dictionary<UnitType, UnitStats>();
        else
            loaded.Clear();

        using var stringReader = new StringReader(input);
        var fields = new CsvReader(stringReader, ",");
        var readHeader = fields.Read();
        Assert.IsTrue(readHeader);
        while (fields.Read()) {
            Assert.AreEqual(14, fields.FieldsCount);

            var entry = new UnitStats();

            var unitType = fields[0].ParseEnum<UnitType>();
            entry.moveCostType = fields[1].ParseEnum<MoveCostType>();

            entry.moveCapacity = fields[2].ParseInt();
            Assert.IsTrue(entry.moveCapacity >= 0, entry.moveCapacity.ToString());

            entry.fuel = fields[3].ParseInt();
            Assert.IsTrue(entry.fuel >= 0, entry.fuel.ToString());

            entry.ammo = new Dictionary<WeaponName, int>();
            var words = fields[4].Separate().ToArray();
            Assert.AreEqual(0, words.Length % 2);
            for (var i = 0; i < words.Length; i += 2) {
                var weaponName = words[i].ParseEnum<WeaponName>();
                var amount = words[i + 1].ParseInt();
                Assert.IsTrue(amount >= 0, $"{weaponName}: {amount}");

                Assert.IsTrue(!entry.ammo.ContainsKey(weaponName), weaponName.ToString());
                entry.ammo.Add(weaponName, amount);
            }

            var attackRangeChunks = fields[5].Split('-');
            Assert.AreEqual(2, attackRangeChunks.Length);
            entry.attackRange = new Vector2Int(attackRangeChunks[0].ParseInt(), attackRangeChunks[1].ParseInt());
            Assert.IsTrue(entry.attackRange[0] <= entry.attackRange[1], entry.attackRange.ToString());
            Assert.IsTrue(entry.attackRange[0] >= 0, entry.attackRange.ToString());

            entry.cost = fields[6].ParseInt();
            Assert.IsTrue(entry.cost >= 0, entry.cost.ToString());

            entry.vision =fields[7].ParseInt();
            Assert.IsTrue(entry.vision >= 0, entry.vision.ToString());

            entry.specialCommands = fields[8].ParseEnum<SpecialCommand>();

            entry.carryCapacity = fields[9].ParseInt();
            entry.weight = fields[10].ParseInt();
            entry.canCarry = new HashSet<UnitType>();
            foreach (var word in fields[11].Separate())
                entry.canCarry.Add(word.ParseEnum<UnitType>());

            entry.unloadCapacity = fields[12].ParseInt();
            entry.unloadCost = fields[13].ParseInt();

            loaded.Add(unitType, entry);
        }
    }
}