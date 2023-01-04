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

    public List<Mission> missions = new() {
        new Mission {
            name = "Tutorial",
            sceneName = "SampleScene",
            initialState = new SerializedLevel(),
            isCompleted = true
        },
        new Mission {
            name = "FirstMission",
            sceneName = "SampleScene",
            initialState = new SerializedLevel(),
            isAvailable = "Tutorial isCompleted"
        },
        new Mission {
            name = "SecondMission",
            sceneName = "SampleScene",
            initialState = new SerializedLevel(),
            isAvailable = "FirstMission isCompleted"
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