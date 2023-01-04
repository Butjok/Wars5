using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SavedGamesView : MonoBehaviour {
    
}

public class PersistentData {

    public static PersistentData Read() {
        return PlayerPrefs.GetString(nameof(PersistentData))?.FromJson<PersistentData>() ?? new PersistentData();
    }
    public void Save() {
        PlayerPrefs.SetString(nameof(PersistentData), this.ToJson());

        Sprite sprite;
    }
    
    public bool firstTimeLaunch = true;
    public Campaign campaign = new();
    public List<SavedGame> savedGames = new();
    public GameSettings gameSettings = new();
}

public class SavedGame {
    public string name;
    public DateTime dateTime;
    public string missionName;
    public SerializedLevel serializedLevel;
}
