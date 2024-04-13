using UnityEngine;

public class CampaignOverviewLauncher : MonoBehaviour {
    public void Start() {
        var game = Game.Instance;
        if (game.stateMachine.Count == 0) {
            game.stateMachine.Push(new GameSessionState(game));
            game.EnqueueCommand(GameSessionState.Command.StartCampaignOverview);
        }
    }    
}