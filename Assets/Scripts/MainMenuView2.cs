using System.Collections.Generic;
using UnityEngine;

public class MainMenuView2 : MonoBehaviour {
    public MainMenuButton campaignButton, loadGameButton, settingsButton, aboutButton, quitButton;
    public IEnumerable<MainMenuButton> Buttons {
        get {
            yield return campaignButton;
            yield return loadGameButton;
            yield return settingsButton;
            yield return aboutButton;
            yield return quitButton;
        }
    }
}