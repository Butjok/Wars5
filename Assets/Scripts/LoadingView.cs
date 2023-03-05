using System;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class LoadingView : MonoBehaviour {

    public LoadingSpinner spinner;
    public UsefulTip usefulTip;
    public Button startButton;
    public TMP_Text startButtonText;
    public Image splashImage;

    private void Start() {
        
        startButton.onClick.AddListener(Launch);
        
        startButtonText = startButton.GetComponentInChildren<TMP_Text>();
        Assert.IsTrue(startButtonText);
        startButtonText.enabled = false;
        startButton.interactable = false;
        
        spinner.gameObject.SetActive(true);
        usefulTip.gameObject.SetActive(true);
    }

    public static Sprite TryGetMissionSplashSprite(MissionName missionName) {
        return missionName switch {
            MissionName.Tutorial or MissionName.FirstMission or MissionName.SecondMission => Resources.Load<Sprite>(missionName.ToString()),
            _ => throw new ArgumentOutOfRangeException(nameof(missionName), missionName, null)
        };
    }

    public MissionName MissionName {
        set {
            splashImage.sprite = TryGetMissionSplashSprite(value);
            if (!splashImage.sprite)
                splashImage.sprite = null;
        }
    }

    [ContextMenu(nameof(SetReady))]
    public void SetReady() {
        Ready = true;
    }
    
    public bool Ready {
        set {
            if (value) {
                startButton.interactable = true;
                startButtonText.enabled = true;
                
                spinner.gameObject.SetActive(false);
                usefulTip.gameObject.SetActive(false);
            }
        }
    }

    public void Launch() {
        Debug.Log("Launch mission!");
    }
}