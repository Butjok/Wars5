using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuView2 : MonoBehaviour {
    public MainMenuButton campaignButton, loadGameButton, settingsButton, aboutButton, quitButton;
    public GameSettingsMenu gameSettingsMenu;
    public Image holdImage;
    public GameObject aboutRoot;
    public ScrollRect aboutScrollRect;
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