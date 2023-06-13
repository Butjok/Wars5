using System;
using System.Collections.Generic;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public class LevelSessionState : StateMachineState {

    public Level level;
    public string input;
    public MissionName missionName;
    public bool isFreshStart;
    public LevelLogic levelLogic;

    public LevelSessionState(StateMachine stateMachine, string input, MissionName missionName, bool isFreshStart) : base(stateMachine) {
        this.input = input;
        this.missionName = missionName;
        this.isFreshStart = isFreshStart;
        levelLogic = missionName switch {
            MissionName.Tutorial => new TutorialLevelLogic(),
            _ => new LevelLogic()
        };
        levelLogic = new TutorialLevelLogic();
    }

    public override IEnumerator<StateChange> Enter {
        get {
            level = new Level { missionName = missionName };

            LevelView.TryLoadScene(level.missionName);
            level.view = LevelView.TryInstantiate();
            Assert.IsTrue(level.view);
            LevelReader.ReadInto(level, input);

            yield return levelLogic.OnLevelStart(this);
            yield return StateChange.Push(new PlayerTurnState(stateMachine));
            yield return levelLogic.OnLevelEnd(this);
        }
    }

    public override void Exit() {
        level.Dispose();
        LevelView.TryUnloadScene(level.missionName);
        Object.Destroy(level.view.gameObject);
        level.view = null;
    }
}