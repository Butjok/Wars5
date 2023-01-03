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

    public Level level;
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

    public void Show(Level level) {

        this.level = level;
        oldSettings = level.settings.ShallowCopy();

        root.SetActive(true);
        UpdateControls();
    }

    public void UpdateControls() {

        masterVolumeSlider.SetValueWithoutNotify(level.settings.masterVolume);
        musicVolumeSlider.SetValueWithoutNotify(level.settings.musicVolume);
        sfxVolumeSlider.SetValueWithoutNotify(level.settings.sfxVolume);
        uiVolumeSlider.SetValueWithoutNotify(level.settings.uiVolume);

        showBattleAnimationToggle.SetIsOnWithoutNotify(level.settings.showBattleAnimation);
        unitSpeedSlider.SetValueWithoutNotify(level.settings.unitSpeed);
        unitSpeedText.text = Mathf.RoundToInt( level.settings.unitSpeed).ToString();
        antiAliasingToggle.SetIsOnWithoutNotify(level.settings.antiAliasing == antiAliasing);
        motionBlurShutterAngleSlider.SetValueWithoutNotify(level.settings.motionBlurShutterAngle is { } value ? value : 0);
        bloomToggle.SetIsOnWithoutNotify(level.settings.enableBloom);
        screenSpaceReflectionsToggle.SetIsOnWithoutNotify(level.settings.enableScreenSpaceReflections);
        ambientOcclusionToggle.SetIsOnWithoutNotify(level.settings.enableAmbientOcclusion);
        shuffleMusicToggle.SetIsOnWithoutNotify(level.settings.shuffleMusic);
        
        //okButton.interactable =game.settings.DiffersFrom(oldSettings);
        closeButton.interactable = !level.settings.DiffersFrom(oldSettings);
    }

    public void UpdateSettings() {

        level.settings.masterVolume = masterVolumeSlider.value;
        level.settings.musicVolume = musicVolumeSlider.value;
        level.settings.sfxVolume = sfxVolumeSlider.value;
        level.settings.uiVolume = uiVolumeSlider.value;

        level.settings.showBattleAnimation = showBattleAnimationToggle.isOn;
        level.settings.unitSpeed = unitSpeedSlider.value;
        level.settings.antiAliasing = antiAliasingToggle.isOn ? antiAliasing : PostProcessLayer.Antialiasing.None;
        level.settings.motionBlurShutterAngle = Mathf.Approximately(motionBlurShutterAngleSlider.value, 0) ? null : motionBlurShutterAngleSlider.value;
        level.settings.enableBloom = bloomToggle.isOn;
        level.settings.enableScreenSpaceReflections = screenSpaceReflectionsToggle.isOn;
        level.settings.enableAmbientOcclusion = ambientOcclusionToggle.isOn;
        level.settings.shuffleMusic = shuffleMusicToggle.isOn;
        
        level.UpdatePostProcessing();

        UpdateControls();
    }

    public void Hide() {
        root.SetActive(false);
    }

    public void Close() {
        if (!level.settings.DiffersFrom(oldSettings))
            GameSettingsState.shouldBreak = true;
        else {
            shakeTweener?.Complete();
            shakeTweener = buttonsRoot.GetComponent<RectTransform>()
                .DOShakePosition(shakeDuration, shakeStrength, shakeVibrato, 0, false, shakeFadeOut)
                .SetEase(shakeEase);
        }
    }
    public void SetDefaultValues() {
        level.settings = new GameSettings();
        level.UpdatePostProcessing();
        UpdateControls();
    }
    public void Cancel() {
        level.settings = oldSettings;
        level.UpdatePostProcessing();
        GameSettingsState.shouldBreak = true;
    }
    public void Ok() {
        level.settings.Save();
        GameSettingsState.shouldBreak = true;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
            Close();
        else if (Input.GetKeyDown(KeyCode.Return))
            Ok();
    }
}