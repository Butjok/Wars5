using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class TutorialLogic : DefaultLevelLogic {

    public bool showDialogue;

    public TutorialLogic(bool showDialogue) {
        this.showDialogue = showDialogue;
    }

    public override (ControlFlow controlFlow, IEnumerator state) OnTurnStart(Game game) {

        return game.turn switch {

            0 => (ControlFlow.Pause,
                showDialogue
                    ? DialogueState.New(game, new[] {
                        new DialogueUi.Speech {
                            speaker = DialogueSpeaker.Natalie,
                            lines = new[] {
                                new DialogueUi.Line {
                                    text = "Welcome to Wars3d!",
                                    playMusic = new[] { "violin uzicko" }
                                },
                                new DialogueUi.Line {
                                    action = () => {
                                        if (CameraRig.Instance)
                                            CameraRig.Instance.Jump(new Vector2Int(2, 1));
                                    }
                                },
                                new DialogueUi.Line {
                                    text = "This is tank! It can shoot!"
                                },
                                new DialogueUi.Line {
                                    text = "Please select it. And move over to this position."
                                },
                                new DialogueUi.Line {
                                    action = () => {
                                        if (CameraRig.Instance)
                                            CameraRig.Instance.Jump(new Vector2(5,0));
                                    }
                                },
                            }
                        }
                    })
                    : PlayMusic(new[] { "violin uzicko" })),

            _ => base.OnTurnStart(game)
        };
        
        /*return game.turn switch {

            0 => (ControlFlow.Pause,
                showDialogue
                    ? DialogueState.New(game, new[] {
                        new DialogueUi.Speech {
                            speaker = DialogueSpeaker.Natalie,
                            lines = new[] {
                                new DialogueUi.Line {
                                    text = "Welcome to Wars3d!",
                                    playMusic = new[] { "violin uzicko" }
                                },
                            }
                        }
                    })
                    : PlayMusic(new[] { "violin uzicko" })),

            _ => base.OnTurnStart(game)
        };*/
    }

    public override IEnumerator OnVictory(Game game) {
        return DialogueState.New(game, new[] {
            new DialogueUi.Speech {
                speaker = DialogueSpeaker.Natalie,
                lines = new[] {
                    new DialogueUi.Line {
                        text = "I knew you would win!",
                        changeMood = DialogueSpeaker.Mood.Happy,
                        playMusic = new[] { "fast uzicko" }
                    }
                },
            }
        });
    }

    public override IEnumerator OnDefeat(Game game) {
        return DialogueState.New(game, new[] {
            new DialogueUi.Speech {
                speaker = DialogueSpeaker.Natalie,
                lines = new[] {
                    new DialogueUi.Line {
                        text = "Oh no...",
                        changeMood = DialogueSpeaker.Mood.Worried
                    },
                    new DialogueUi.Line {
                        text = "You can try to do bla-bla...",
                        changeMood = DialogueSpeaker.Mood.Normal
                    },
                }
            }
        });
    }

    public bool madeFirstMove = false;

    public override (ControlFlow controlFlow, IEnumerator state) OnActionCompletion(Game game, UnitAction action) {

        /*if (!madeFirstMove) {
            madeFirstMove = true;
            return (ControlFlow.Pause, DialogueState.New(game, new[] {
                new DialogueUi.Speech {
                    speaker = DialogueSpeaker.Natalie,
                    lines = new[] {
                        new DialogueUi.Line {
                            text = "That's right! You made your first move!",
                            action = () => {
                                Assert.IsTrue(action.unit.position.v != null, "action.unit.position.v != null");
                                CameraRig.Instance.Jump((Vector2Int)action.unit.position.v);
                            }
                        },
                    }
                }
            }));
        }*/

        return (ControlFlow.Ignore, null);
    }
}