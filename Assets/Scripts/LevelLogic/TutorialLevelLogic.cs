using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class TutorialLogic : DefaultLevelLogic {

    public const bool dialogue = true;
    
    public override (ControlFlow controlFlow, IEnumerator state) OnTurnStart(Game2 game) {

        if (!dialogue)
            return (ControlFlow.Ignore, null);
        
        return game.turn switch {

            0 => (ControlFlow.Pause, DialogueState.New(game, new[] {
                new DialogueUi.Speech {
                    speaker = DialogueSpeaker.Natalie,
                    lines = new[] {
                        new DialogueUi.Line {
                            text = "Hello there!",
                            playMusic = new[]{"violin uzicko".LoadAs<AudioClip>()}
                        },
                        new DialogueUi.Line {
                            text = "This is 3dWars!",
                            action = () => CameraRig.Instance.Jump(new Vector2(5, 5)),
                        },
                        new DialogueUi.Line {
                            text = "Something here as well!",
                            action = () => CameraRig.Instance.Jump(new Vector2(0, 0))
                        },
                    }
                },
                new DialogueUi.Speech {
                    speaker = DialogueSpeaker.Vladan,
                    lines = new[] {
                        new DialogueUi.Line {
                            text = "A real strategy game!",
                        },
                    }
                }
            })),

            1 => (ControlFlow.Pause, DialogueState.New(game, new[] {
                new DialogueUi.Speech {
                    speaker = DialogueSpeaker.Vladan,
                    lines = new[] {
                        new DialogueUi.Line {
                            text = "I said no!",
                        },
                    }
                }
            })),

            _ => (ControlFlow.Ignore, null)
        };

        return (ControlFlow.Ignore, null);
    }

    public override IEnumerator OnVictory(Game2 game) {
        return DialogueState.New(game, new[] {
            new DialogueUi.Speech {
                speaker = DialogueSpeaker.Natalie,
                lines = new[] {
                    new DialogueUi.Line {
                        text = "I knew you would win!",
                        moodChange = DialogueSpeaker.Mood.Happy
                    }
                }
            }
        });
    }

    public override IEnumerator OnDefeat(Game2 game) {
        return DialogueState.New(game, new[] {
            new DialogueUi.Speech {
                speaker = DialogueSpeaker.Natalie,
                lines = new[] {
                    new DialogueUi.Line {
                        text = "Oh no...",
                        moodChange = DialogueSpeaker.Mood.Worried
                    },
                    new DialogueUi.Line {
                        text = "You can try to do bla-bla...",
                        moodChange = DialogueSpeaker.Mood.Normal
                    },
                }
            }
        });
    }

    public bool madeFirstMove = false;

    public override (ControlFlow controlFlow, IEnumerator state) OnActionCompletion(Game2 game, UnitAction action) {

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