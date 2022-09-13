using UnityEngine.Assertions;

public abstract class LevelLogic {

    public Game2 game;

    protected LevelLogic(Game2 game) {
        Assert.IsTrue(game);
        this.game = game;
    }

    public virtual bool OnTurnStart() {
        return false;
    }
    public virtual bool OnTurnEnd() {
        return false;
    }
    public virtual bool OnActionCompletion(UnitAction action) {
        return false;
    }
}

public class Tutorial2 : LevelLogic {

    public Tutorial2(Game2 game) : base(game) { }

    public override bool OnTurnStart() {

        if (game.Turn == 0) {
            game.state.PauseTo(new DialogueState(game, new[] {
                new DialogueUi.Speech {
                    speaker = DialogueSpeaker.Natalie,
                    lines = new[] {
                        new DialogueUi.Line { text = "Hello there!" },
                        new DialogueUi.Line { text = "Welcome!" },
                    },
                }
            }));
            return true;
        }

        return false;
    }
}