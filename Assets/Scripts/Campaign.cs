using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class Campaign {

    public class Mission {
        public string name;
        public string sceneName;
        public string isAvailable = "true";
        public bool isCompleted;
        public Type levelLogicType = typeof(LevelLogic);
        public SerializedLevel initialState;
    }

    public static Campaign Load() => PlayerPrefs.GetString(nameof(Campaign))?.FromJson<Campaign>() ?? new Campaign();
    public void Save() => PlayerPrefs.SetString(nameof(Campaign), this.ToJson()); 

    public List<Mission> missions = new() {
        new Mission {
            name = "Tutorial",
            sceneName = "SampleScene",
            initialState = new SerializedLevel(),
            isCompleted = true
        },
        new Mission {
            name = "FirstBattle",
            sceneName = "SampleScene",
            initialState = new SerializedLevel(),
            isAvailable = "Tutorial isCompleted"
        }
    };
    
    public Mission this[string missionName] {
        get {
            var mission = missions.SingleOrDefault(mission => mission.name == missionName);
            Assert.IsNotNull(mission, missionName);
            return mission;
        }
    }

    public bool IsAvailable(string missionName) {
        var stack = PostfixInterpreter.Execute(this[missionName].isAvailable, (token, stack) => {
            switch (token) {
                case "isCompleted":
                    stack.Push(this[stack.Pop<string>()].isCompleted);
                    return true;
                case "isAvailable":
                    stack.Push(IsAvailable(stack.Pop<string>()));
                    return true;
                default:
                    return false;
            }
        });
        Assert.AreEqual(1, stack.Count);
        return stack.Pop<bool>();
    }
}