using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public static class LoadGameState {

    public static bool shouldBreak;
    
    public static IEnumerator New(Game game) {
        
        shouldBreak = false;

        var saves = PlayerPrefs.GetString("Saves", "[]").FromJson<List<SaveData2>>();

        var menu = Object.FindObjectOfType<LoadGameMenu>(true);
        Assert.IsTrue(menu);

        menu.Show(game, saves);
        
        while (true) {
            yield return null;

            if (shouldBreak) {
                shouldBreak = false;
                
                menu.Hide();
                yield break;
            }
        }
    }
}

public class SaveData2 {
    public string name;
    public DateTime dateTime;
    public string sceneName;
    public string commands;
}