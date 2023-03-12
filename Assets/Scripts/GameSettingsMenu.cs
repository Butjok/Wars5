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
        oldSettings = level.persistentData.gameSettings.ShallowCopy();

        root.SetActive(true);
        UpdateControls();
    }

    public void UpdateControls() {

        masterVolumeSlider.SetValueWithoutNotify(level.persistentData.gameSettings.masterVolume);
        musicVolumeSlider.SetValueWithoutNotify(level.persistentData.gameSettings.musicVolume);
        sfxVolumeSlider.SetValueWithoutNotify(level.persistentData.gameSettings.sfxVolume);
        uiVolumeSlider.SetValueWithoutNotify(level.persistentData.gameSettings.uiVolume);

        showBattleAnimationToggle.SetIsOnWithoutNotify(level.persistentData.gameSettings.showBattleAnimation);
        unitSpeedSlider.SetValueWithoutNotify(level.persistentData.gameSettings.unitSpeed);
        unitSpeedText.text = Mathf.RoundToInt( level.persistentData.gameSettings.unitSpeed).ToString();
        antiAliasingToggle.SetIsOnWithoutNotify(level.persistentData.gameSettings.antiAliasing == antiAliasing);
        motionBlurShutterAngleSlider.SetValueWithoutNotify(level.persistentData.gameSettings.motionBlurShutterAngle is { } value ? value : 0);
        bloomToggle.SetIsOnWithoutNotify(level.persistentData.gameSettings.enableBloom);
        screenSpaceReflectionsToggle.SetIsOnWithoutNotify(level.persistentData.gameSettings.enableScreenSpaceReflections);
        ambientOcclusionToggle.SetIsOnWithoutNotify(level.persistentData.gameSettings.enableAmbientOcclusion);
        shuffleMusicToggle.SetIsOnWithoutNotify(level.persistentData.gameSettings.shuffleMusic);
        
        //okButton.interactable =game.settings.DiffersFrom(oldSettings);
        closeButton.interactable = !level.persistentData.gameSettings.DiffersFrom(oldSettings);
    }

    public void UpdateSettings() {

        level.persistentData.gameSettings.masterVolume = masterVolumeSlider.value;
        level.persistentData.gameSettings.musicVolume = musicVolumeSlider.value;
        level.persistentData.gameSettings.sfxVolume = sfxVolumeSlider.value;
        level.persistentData.gameSettings.uiVolume = uiVolumeSlider.value;

        level.persistentData.gameSettings.showBattleAnimation = showBattleAnimationToggle.isOn;
        level.persistentData.gameSettings.unitSpeed = unitSpeedSlider.value;
        level.persistentData.gameSettings.antiAliasing = antiAliasingToggle.isOn ? antiAliasing : PostProcessLayer.Antialiasing.None;
        level.persistentData.gameSettings.motionBlurShutterAngle = Mathf.Approximately(motionBlurShutterAngleSlider.value, 0) ? null : motionBlurShutterAngleSlider.value;
        level.persistentData.gameSettings.enableBloom = bloomToggle.isOn;
        level.persistentData.gameSettings.enableScreenSpaceReflections = screenSpaceReflectionsToggle.isOn;
        level.persistentData.gameSettings.enableAmbientOcclusion = ambientOcclusionToggle.isOn;
        level.persistentData.gameSettings.shuffleMusic = shuffleMusicToggle.isOn;
        
        level.UpdatePostProcessing();

        UpdateControls();
    }

    public void Hide() {
        root.SetActive(false);
    }

    public void Close() {
        if (!level.persistentData.gameSettings.DiffersFrom(oldSettings))
            level.commands.Enqueue(GameSettingsState.close);
        else {
            shakeTweener?.Complete();
            shakeTweener = buttonsRoot.GetComponent<RectTransform>()
                .DOShakePosition(shakeDuration, shakeStrength, shakeVibrato, 0, false, shakeFadeOut)
                .SetEase(shakeEase);
        }
    }
    public void SetDefaultValues() {
        level.persistentData.gameSettings = new GameSettings();
        level.UpdatePostProcessing();
        UpdateControls();
    }
    public void Cancel() {
        level.persistentData.gameSettings = oldSettings;
        level.UpdatePostProcessing();
        level.commands.Enqueue(GameSettingsState.close);
    }
    public void Ok() {
        var persistentData = PersistentData.Read();
        persistentData.gameSettings = level.persistentData.gameSettings;
        persistentData.Save();
        level.commands.Enqueue(GameSettingsState.close);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
            Close();
        else if (Input.GetKeyDown(KeyCode.Return))
            Ok();
    }
}