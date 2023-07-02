using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
public class GitInfoEntry {

    [JsonProperty]
    public string commit;
    [JsonProperty]
    public string author;
    [JsonProperty]
    public string date;
    [JsonProperty]
    public string message;

    public DateTime DateTime => DateTime.Parse(date);
    
    public static bool TryLoad(out List<GitInfoEntry> list) {
        var gitInfoJsonTextAsset = Resources.Load<TextAsset>("GitInfo");
        if (!gitInfoJsonTextAsset) {
            list = null;
            return false;
        }
        var json = gitInfoJsonTextAsset.text;
        list = json.FromJson<List<GitInfoEntry>>().OrderByDescending(e => e.DateTime).ToList();
        return true;
    }
}