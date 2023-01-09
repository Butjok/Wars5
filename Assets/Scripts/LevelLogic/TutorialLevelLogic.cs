using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class TutorialLogic : LevelLogic {

    public bool showDialogue;

    public TutorialLogic(bool showDialogue) {
        this.showDialogue = showDialogue;
    }

    public override (ControlFlow controlFlow, IEnumerator state) OnTurnStart(Main main) {

        CameraRig.TryFind(out var cameraRig);
        
        return main.turn switch {

            0 => (ControlFlow.Pause,
                showDialogue
                    ? DialogueState.Run(main, new[] {
                        new DialogueUi.Speech {
                            speaker = DialogueSpeaker.Natalie,
                            lines = new[] {
                                new DialogueUi.Line {
                                    text = "Welcome to Wars3d!",
                                    playMusic = new[] { "violin uzicko" }
                                },
                                new DialogueUi.Line {
                                    action = () => {
                                        if (cameraRig)
                                            cameraRig.Jump(new Vector2Int(2, 1).ToVector3Int());
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
                                        if (cameraRig)
                                            cameraRig.Jump(new Vector2Int(5,0).ToVector3Int());
                                    }
                                },
                            }
                        }
                    })
                    : PlayMusic(new[] { "violin uzicko" })),

            _ => base.OnTurnStart(main)
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

    public override IEnumerator OnVictory(Main main, UnitAction winningAction) {
        return DialogueState.Run(main, new[] {
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

    public override IEnumerator OnDefeat(Main main, UnitAction defeatingAction) {
        return DialogueState.Run(main, new[] {
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

    public override (ControlFlow controlFlow, IEnumerator state) OnActionCompletion(Main main, UnitAction action) {

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