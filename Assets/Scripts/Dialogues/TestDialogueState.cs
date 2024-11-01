using System.Collections.Generic;
using UnityEngine;
using static Gettext;

public class TestDialogueState : DialogueState {

    public TestDialogueState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Enter {
        get {
            Start();

            yield return AddPerson(PersonName.Natalie);
            // using (LinesOf(PersonName.Natalie)) {
            //     yield return Say(_("Hello, my name is Natalie."));
            //     yield return AddPerson(PersonName.Vladan, right);
            //     using (LinesOf(PersonName.Vladan)) {
            //         yield return Say(_("Hello, my name is Vladan."));
            //     }
            //     yield return RemovePerson(PersonName.Vladan);
            //     yield return Say(_("I am a student of the Faculty of Mathematics."));
            // }
            // yield return AddPerson(PersonName.Vladan, left);

            // var video = ShowVideo("encoded2".LoadAs<VideoClip>(), new Vector2(500, 500));
            // var completed = false;
            // video.videoPlayer.loopPointReached += _ => completed = true;
            // yield return WaitWhile(() => !completed);
            // HideVideo(video);

            Speaker = PersonName.Natalie;

            yield return SayWait("We are going to look at some video!");

            // var (videoPlayer, _) = ShowVideo("encoded2".LoadAs<VideoClip>(), new Vector2(300, 300));
            // var completed = false;
            // videoPlayer.loopPointReached += _ => completed = true;

            PushSkippableSequence();
            // yield return WaitWhile(() => !completed);
            // HideVideo(videoPlayer);
            yield return Say("So...");
            yield return Wait(1);
            yield return Append(" What do you think?");
            PopSkippableSequence();

            var opinionOnVideo = -1;
            yield return ChooseOption(value => opinionOnVideo = value, _("It was good"), _("Kinda meh"));
            if (opinionOnVideo == 0)
                yield return SayWait("I was thinking the same! Pretty cool, right?");
            else
                yield return SayWait("Yeah, you're right. It could need some work.");

            var opinionOnApples = -1;
            while (opinionOnApples is not (0 or 1)) {
                yield return Say(_("Do you like apples?"));
                yield return ChooseOption(value => opinionOnApples = value, _("Yes"), _("No"), _("Explain"));
                switch (opinionOnApples) {
                    case 0:
                        yield return SayWait(_("I like apples too!"));
                        break;
                    case 1:
                        yield return SayWait(_("I don't like apples either."));
                        break;
                    case 2:
                        var image = ShowImage("apple".LoadAs<Sprite>(), new Vector2(150, 150));
                        PushSkippableSequence();
                        yield return Say(_("This is an apple."));
                        yield return Wait(1);
                        yield return AppendWait(_(" It is a fruit."));
                        yield return Append(_(" It is red."));
                        yield return Wait(1);
                        yield return Append(_(" It is tasty."));
                        yield return Wait(.5f);
                        yield return Append(_("."));
                        yield return Wait(.5f);
                        yield return Append(_("."));
                        yield return Wait(1);
                        yield return AppendWait(_(" Got it?"));
                        PopSkippableSequence();
                        HideImage(image);
                        break;
                }
            }

            yield return AddPerson(PersonName.Vladan, DialogueUi4.Side.Right);

            Speaker = PersonName.Vladan;
            yield return SayWait(_("Well well well! Look who we have here! Natalie with her apples again!"));

            Speaker = PersonName.Natalie;
            yield return SayWait(_("No, Vladan! Not you again!"));

            Speaker = null;
            SetText("");

            yield return ClearPersons();
            End();
        }
    }
}