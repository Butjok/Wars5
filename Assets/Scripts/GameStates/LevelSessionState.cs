using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public class LevelSessionState : StateMachineState {

    public Level level;
    public string input;
    public MissionName missionName;
    public bool isFreshStart;
    public LevelLogic levelLogic;
    public bool autoplay;
    public IEnumerator autoplayHandler;

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

            autoplayHandler = AutoplayHandler();
            FindState<GameSessionState>().game.StartCoroutine(autoplayHandler);

            yield return levelLogic.OnLevelStart(this);
            yield return StateChange.Push(new PlayerTurnState(stateMachine));
            yield return levelLogic.OnLevelEnd(this);
        }
    }

    public override void Exit() {
        FindState<GameSessionState>().game.StopCoroutine(autoplayHandler);
        level.Dispose();
        LevelView.TryUnloadScene(level.missionName);
        Object.Destroy(level.view.gameObject);
        level.view = null;
    }
    
    public IEnumerator AutoplayHandler() {
        const KeyCode key = KeyCode.Alpha8;
        while (true) {
            yield return null;
            if (Input.GetKeyDown(key)) {
                autoplay = true;
                var onHoldKey = !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
                yield return null;
                while (onHoldKey ? !Input.GetKeyUp(key) : !Input.GetKeyDown(key))
                    yield return null;
                yield return null;
                autoplay = false;
            }
        }
    }
}