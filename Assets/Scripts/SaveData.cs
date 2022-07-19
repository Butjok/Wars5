using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.PostProcessing;

public class SaveData {

	public string name;
	public string sceneName;
	public DateTime dateTime;
	public string json;

	public SerializedLevel Level {
		get => json.FromJson<SerializedLevel>();
		set => json = value.ToJson();
	}

	public override string ToString() {
		return $"{name} {dateTime}";
	}
}

public static class SaveDataManager {

	public const string playerPrefsNamesKey = nameof(SaveDataManager) + "." + nameof(Names);
	public static string[] Names {
		get {
			var json = PlayerPrefs.GetString(playerPrefsNamesKey);
			return string.IsNullOrWhiteSpace(json) ? Array.Empty<string>() : json.FromJson<string[]>();
		}
	}
	public const string playerPrefsSavePrefix = nameof(SaveDataManager) + ".Save.";

	public static IEnumerable<SaveData> All => Names.Select(Read);

	private static Dictionary<string, SaveData> cache = new();

	public static SaveData Read(string name) {
		if (!cache.TryGetValue(name, out var record)) {
			var json = PlayerPrefs.GetString(playerPrefsSavePrefix + name);
			Assert.IsNotNull(json);
			record = cache[name] = json.FromJson<SaveData>();
			record.name = name;
		}
		return record;
	}

	public static void Save(string name, SaveData data) {
		cache[name] = data;
		PlayerPrefs.SetString(playerPrefsSavePrefix + name, data.ToJson());
		PlayerPrefs.SetString(playerPrefsNamesKey, Names.Union(new[] { name }).ToArray().ToJson());
	}
}

