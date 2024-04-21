using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using UnityEngine;
using static Gettext;

public class BorderIncidentScenario : MonoBehaviour {

    [Command]
    public void Execute() {
        StartCoroutine(Animation(null));
    }
    
    public const float unitSpeed = 2;

    public IEnumerator<bool> MoveUnitView(UnitView view, Vector2Int to) {
        var path = new List<Vector2Int> { view.Position };
        path.AddRange(Woo.Traverse2D(view.Position, to));
        var movement= view.MoveAlong(path, speed: unitSpeed);
        while (!movement())
            yield return false;
        view.Position = path[^1];
        yield return true;
    }

    public IEnumerator Animation(Action onComplete) {
        var game = Game.Instance;
        var stateMachine = game.stateMachine;
        var level = game.Level;
        var bluePlayer = level.players.Single(p => p.ColorName == ColorName.Blue);

        var recon = level.units[new Vector2Int(21, 9)];
        {
            var movement = MoveUnitView(recon.view, new Vector2Int(27, 7));
            while (movement.MoveNext() && !movement.Current)
                yield return null;
        }

        var infantry = new Unit(bluePlayer, UnitType.Infantry, new Vector2Int(27, 8));
        {
            var movement = MoveUnitView(infantry.view, new Vector2Int(29, 8));
            while (movement.MoveNext() && !movement.Current)
                yield return null;
        }
        
        game.stateMachine.Push( new BorderIncidentDialogueState(game.stateMachine));

        onComplete?.Invoke();
    }
}

public class BorderIncidentDialogueState : DialogueState {
    public BorderIncidentDialogueState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Enter {
        get {
            Start();
            yield return AddPerson(PersonName.BlueOfficer, DialogueUi4.Side.Left);
            yield return AddPerson(PersonName.RedOfficer, DialogueUi4.Side.Right);
            
            Speaker = PersonName.BlueOfficer;
            yield return SayWait( _("It is a good morning, isn't it?"));
            Speaker = PersonName.RedOfficer;
            yield return Say( _("Well, it was until you showed up."));
            yield return Wait(1);
            yield return AppendWait( _(" It is going to be a good morning for me, at least."));
            Speaker = PersonName.BlueOfficer;
            yield return Say( _("Grumpy as always, I see."));
            yield return Wait(1);
            yield return AppendWait( _(" I was ordered to patrol this area. I am not here to bother you."));
            Speaker = PersonName.RedOfficer;
            yield return SayWait( _("I am not here to be bothered by you."));
            Speaker = PersonName.BlueOfficer;
            yield return SayWait( _("Nice talking to you, as always."));
            
            End();
        }
    }
}