using System.Collections.Generic;
using UnityEngine.Video;
using static Gettext;

public class TutorialStartDialogue : DialogueState {

    public TutorialStartDialogue(StateMachine stateMachine) : base(stateMachine) { }
    public override IEnumerator<StateChange> Enter {
        get {

            var persistentData = stateMachine.Find<GameSessionState>().persistentData;
            var level = stateMachine.Find<LevelSessionState>().level;

            Start();
            yield return AddPerson(PersonName.Natalie);
            Speaker = PersonName.Natalie;
            //yield return SayWait(_("Welcome to Wars3D!"));
            //yield return SayWait(_("This is a strategy game and you are in charge!"));


            if (persistentData.playTutorial) {
                yield return Say(_("Do you want to watch tutorial?"));
                bool yes = default;
                yield return ChooseYesNo(value => yes = value);
                if (yes) {
                    yield return Say(_("Sure thing!"));
                    yield return Wait(.5f);
                    yield return SayWait(_("Let us start with the basics!"));
                    yield return ShowVideoPanel();
                    yield return Wait(.25f);
                    MakeDark();
                    var video = CreateVideo("unit-movement".LoadAs<VideoClip>(), target: VideoPanelImage);                    
                    video.player.playbackSpeed = 1;
                    yield return WaitWhile(() => !video.Completed);
                    yield return SayWait(_("Now that you know the basics, let us get started!"));
                    DestroyVideo(video);
                    yield return Wait(.25f);
                    MakeLight();
                    yield return Wait(.25f);
                    yield return HideVideoPanel();
                }
                else
                    yield return SayWait(_("Sure thing, let us get started!"));
            }

            yield return RemovePerson(PersonName.Natalie);
            End();
        }
    }
}

public class TutorialVladansTurnDialogue : DialogueState {
    public TutorialVladansTurnDialogue(StateMachine stateMachine) : base(stateMachine) { }
    public override IEnumerator<StateChange> Enter {
        get {
            Start();
            yield return AddPerson(PersonName.Vladan);
            Speaker = PersonName.Vladan;
            yield return SayWait(_("Finally! My turn!"));
            yield return RemovePerson(PersonName.Vladan);
            End();
        }
    }
}