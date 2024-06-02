using System.Collections.Generic;
using System.IO;
using System.Linq;
using Butjok.CommandLine;
using NReco.Csv;
using UnityEngine;
using UnityEngine.Assertions;
using Table = System.Collections.Generic.Dictionary<(UnitType attackerType, UnitType targetType, WeaponName weaponName), float>;

public static class DamageTable {

    public static Table Data { get; private set; }
    public static bool IsLoaded => Data != null;
    static DamageTable() {
        Load();
    }

    [Command]
    public static void Load() {
        var input = "DamageTable".LoadAs<TextAsset>().text;

        if (Data == null)
            Data = new Table();
        else
            Data.Clear();

        using var stringReader = new StringReader(input);
        var fields = new CsvReader(stringReader, ",");
        var readHeader = fields.Read();
        Assert.IsTrue(readHeader);
        while (fields.Read()) {
            Assert.AreEqual(3, fields.FieldsCount);

            var attackerType = fields[0].ParseEnum<UnitType>();
            var targetType = fields[1].ParseEnum<UnitType>();
            var words = fields[2].Separate().ToArray();
            Assert.AreEqual(0, words.Length % 2);
            for (var i = 0; i < words.Length; i += 2) {
                var weaponName = words[i].ParseEnum<WeaponName>();
                var damage = words[i + 1].ParseInt();
                Data.Add((attackerType, targetType, weaponName), (float)damage / 100);
            }
        }
    }
}