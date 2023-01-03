using System.Globalization;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Object = System.Object;

public class GameSettingsMenu : MonoBehaviour {

    public Main main;
    public GameSettings oldSettings;
    public GameObject root;

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

    public void Show(Main main) {

        this.main = main;
        oldSettings = main.settings.ShallowCopy();

        root.SetActive(true);
        UpdateControls();
    }

    public void UpdateControls() {

        masterVolumeSlider.SetValueWithoutNotify(main.settings.masterVolume);
        musicVolumeSlider.SetValueWithoutNotify(main.settings.musicVolume);
        sfxVolumeSlider.SetValueWithoutNotify(main.settings.sfxVolume);
        uiVolumeSlider.SetValueWithoutNotify(main.settings.uiVolume);

        showBattleAnimationToggle.SetIsOnWithoutNotify(main.settings.showBattleAnimation);
        unitSpeedSlider.SetValueWithoutNotify(main.settings.unitSpeed);
        unitSpeedText.text = Mathf.RoundToInt( main.settings.unitSpeed).ToString();
        antiAliasingToggle.SetIsOnWithoutNotify(main.settings.antiAliasing == antiAliasing);
        motionBlurShutterAngleSlider.SetValueWithoutNotify(main.settings.motionBlurShutterAngle is { } value ? value : 0);
        bloomToggle.SetIsOnWithoutNotify(main.settings.enableBloom);
        screenSpaceReflectionsToggle.SetIsOnWithoutNotify(main.settings.enableScreenSpaceReflections);
        ambientOcclusionToggle.SetIsOnWithoutNotify(main.settings.enableAmbientOcclusion);
        shuffleMusicToggle.SetIsOnWithoutNotify(main.settings.shuffleMusic);
        
        //okButton.interactable =game.settings.DiffersFrom(oldSettings);
        closeButton.interactable = !main.settings.DiffersFrom(oldSettings);
    }

    public void UpdateSettings() {

        main.settings.masterVolume = masterVolumeSlider.value;
        main.settings.musicVolume = musicVolumeSlider.value;
        main.settings.sfxVolume = sfxVolumeSlider.value;
        main.settings.uiVolume = uiVolumeSlider.value;

        main.settings.showBattleAnimation = showBattleAnimationToggle.isOn;
        main.settings.unitSpeed = unitSpeedSlider.value;
        main.settings.antiAliasing = antiAliasingToggle.isOn ? antiAliasing : PostProcessLayer.Antialiasing.None;
        main.settings.motionBlurShutterAngle = Mathf.Approximately(motionBlurShutterAngleSlider.value, 0) ? null : motionBlurShutterAngleSlider.value;
        main.settings.enableBloom = bloomToggle.isOn;
        main.settings.enableScreenSpaceReflections = screenSpaceReflectionsToggle.isOn;
        main.settings.enableAmbientOcclusion = ambientOcclusionToggle.isOn;
        main.settings.shuffleMusic = shuffleMusicToggle.isOn;
        
        main.UpdatePostProcessing();

        UpdateControls();
    }

    public void Hide() {
        root.SetActive(false);
    }

    public void Close() {
        if (!main.settings.DiffersFrom(oldSettings))
            GameSettingsState.shouldBreak = true;
        else {
            shakeTweener?.Complete();
            shakeTweener = buttonsRoot.GetComponent<RectTransform>()
                .DOShakePosition(shakeDuration, shakeStrength, shakeVibrato, 0, false, shakeFadeOut)
                .SetEase(shakeEase);
        }
    }
    public void SetDefaultValues() {
        main.settings = new GameSettings();
        main.UpdatePostProcessing();
        UpdateControls();
    }
    public void Cancel() {
        main.settings = oldSettings;
        main.UpdatePostProcessing();
        GameSettingsState.shouldBreak = true;
    }
    public void Ok() {
        main.settings.Save();
        GameSettingsState.shouldBreak = true;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
            Close();
        else if (Input.GetKeyDown(KeyCode.Return))
            Ok();
    }
}