using UnityEngine;
using UnityEngine.Assertions;

public class GameMenu : MonoBehaviour {

    public Game game;

    public void Show(Game game) {
        Assert.IsTrue(game);
        this.game = game;
        gameObject.SetActive(true);
    }
    public void Hide() {
        gameObject.SetActive(false);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
            Cancel();
    }

    public void Cancel() {
        game.input.cancel = true;
    }
}