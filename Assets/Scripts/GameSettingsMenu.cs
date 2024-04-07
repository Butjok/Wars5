using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class GameSettingsMenu : MonoBehaviour {

    public RectTransform root;
    public MainMenuView2 mainMenuView;

    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider uiVolumeSlider;

    public Toggle showBattleAnimationToggle;
    public Slider unitSpeedSlider;
    public Toggle antiAliasingToggle;
    public Slider motionBlurShutterAngleSlider;
    public Toggle bloomToggle;
    public Toggle screenSpaceReflectionsToggle;
    public Toggle ambientOcclusionToggle;
    public Toggle shuffleMusicToggle;

    public RectTransform buttonsRoot;
    public Button closeButton;

    public float shakeDuration = 1;
    public Vector3 shakeStrength = new Vector3(5, 0, 0);
    public Ease shakeEase = Ease.Unset;
    public int shakeVibrato = 10;
    public bool shakeFadeOut = false;
    public Tweener shakeTweener;

    public TMP_Text unitSpeedText;

    public const PostProcessLayer.Antialiasing antiAliasing = PostProcessLayer.Antialiasing.TemporalAntialiasing;

    public PersistentData persistentData;

    public Action enqueueCloseCommand;
    public string oldJson;

    public void Show(Settings settings, Action enqueueCloseCommand) {

        this.enqueueCloseCommand = enqueueCloseCommand;

        persistentData = settings.persistentData;
        oldJson = settings.ToJson();

        root.gameObject.SetActive(true);
        mainMenuView.TranslateShowPanel(root);
        
        UpdateControls();
    }

    public void UpdateControls() {

        masterVolumeSlider.SetValueWithoutNotify(persistentData.settings.audio.volume.master);
        musicVolumeSlider.SetValueWithoutNotify(persistentData.settings.audio.volume.music);
        sfxVolumeSlider.SetValueWithoutNotify(persistentData.settings.audio.volume.effects);
        uiVolumeSlider.SetValueWithoutNotify(persistentData.settings.audio.volume.ui);
        shuffleMusicToggle.SetIsOnWithoutNotify(persistentData.settings.audio.shuffleMusic);

        showBattleAnimationToggle.SetIsOnWithoutNotify(persistentData.settings.game.showBattleAnimation);
        unitSpeedSlider.SetValueWithoutNotify(persistentData.settings.game.unitSpeed);
        unitSpeedText.text = Mathf.RoundToInt(persistentData.settings.game.unitSpeed).ToString();

        antiAliasingToggle.SetIsOnWithoutNotify(persistentData.settings.video.enableAntiAliasing);
        motionBlurShutterAngleSlider.SetValueWithoutNotify(persistentData.settings.video.enableMotionBlur ? 270 : 0);
        bloomToggle.SetIsOnWithoutNotify(persistentData.settings.video.enableBloom);
        ambientOcclusionToggle.SetIsOnWithoutNotify(persistentData.settings.video.enableAmbientOcclusion);

        closeButton.interactable = persistentData.settings.ToJson() == oldJson;
    }

    public void UpdateSettings() {

        persistentData.settings.audio.volume.master = masterVolumeSlider.value;
        persistentData.settings.audio.volume.music = musicVolumeSlider.value;
        persistentData.settings.audio.volume.effects = sfxVolumeSlider.value;
        persistentData.settings.audio.volume.ui = uiVolumeSlider.value;
        persistentData.settings.audio.shuffleMusic = shuffleMusicToggle.isOn;

        persistentData.settings.game.showBattleAnimation = showBattleAnimationToggle.isOn;
        persistentData.settings.game.unitSpeed = unitSpeedSlider.value;

        persistentData.settings.video.enableAntiAliasing = antiAliasingToggle.isOn;
        persistentData.settings.video.enableMotionBlur = !Mathf.Approximately(motionBlurShutterAngleSlider.value, 0);
        persistentData.settings.video.enableBloom = bloomToggle.isOn;
        persistentData.settings.video.enableAmbientOcclusion = ambientOcclusionToggle.isOn;
        PostProcessing.Setup(persistentData.settings);

        UpdateControls();
    }

    public void Hide() {
        mainMenuView.TranslateHidePanel(root);
    }

    public bool TryClose() {
        if (oldJson == persistentData.settings.ToJson()) {
            enqueueCloseCommand?.Invoke();
            return true;
        }
        shakeTweener?.Complete();
        shakeTweener = buttonsRoot.GetComponent<RectTransform>()
            .DOShakePosition(shakeDuration, shakeStrength, shakeVibrato, 0, false, shakeFadeOut)
            .SetEase(shakeEase);
        return false;
    }
    public void SetDefaultValues() {
        persistentData.settings = new Settings { persistentData = persistentData.settings.persistentData };
        PostProcessing.Setup(persistentData.settings);
        UpdateControls();
    }
    public void Cancel() {
        persistentData.settings = oldJson.FromJson<Settings>();
        persistentData.settings.persistentData = persistentData;
        PostProcessing.Setup(persistentData.settings);
        enqueueCloseCommand?.Invoke();
    }
    public void Ok() {
        persistentData.Write();
        enqueueCloseCommand?.Invoke();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
            TryClose();
        else if (Input.GetKeyDown(KeyCode.Return))
            Ok();
    }
}