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

    public GameSettings gameSettings;
    
    public void Show(Level level) {

        this.level = level;
        gameSettings = PersistentData.Get.gameSettings.ShallowCopy();

        root.SetActive(true);
        UpdateControls();
    }

    public void UpdateControls() {

        masterVolumeSlider.SetValueWithoutNotify(gameSettings.masterVolume);
        musicVolumeSlider.SetValueWithoutNotify(gameSettings.musicVolume);
        sfxVolumeSlider.SetValueWithoutNotify(gameSettings.sfxVolume);
        uiVolumeSlider.SetValueWithoutNotify(gameSettings.uiVolume);

        showBattleAnimationToggle.SetIsOnWithoutNotify(gameSettings.showBattleAnimation);
        unitSpeedSlider.SetValueWithoutNotify(gameSettings.unitSpeed);
        unitSpeedText.text = Mathf.RoundToInt( gameSettings.unitSpeed).ToString();
        antiAliasingToggle.SetIsOnWithoutNotify(gameSettings.antiAliasing == antiAliasing);
        motionBlurShutterAngleSlider.SetValueWithoutNotify(gameSettings.motionBlurShutterAngle is { } value ? value : 0);
        bloomToggle.SetIsOnWithoutNotify(gameSettings.enableBloom);
        screenSpaceReflectionsToggle.SetIsOnWithoutNotify(gameSettings.enableScreenSpaceReflections);
        ambientOcclusionToggle.SetIsOnWithoutNotify(gameSettings.enableAmbientOcclusion);
        shuffleMusicToggle.SetIsOnWithoutNotify(gameSettings.shuffleMusic);
        
        //okButton.interactable =game.settings.DiffersFrom(oldSettings);
        closeButton.interactable = !gameSettings.DiffersFrom(PersistentData.Get.gameSettings);
    }

    public void UpdateSettings() {

        gameSettings.masterVolume = masterVolumeSlider.value;
        gameSettings.musicVolume = musicVolumeSlider.value;
        gameSettings.sfxVolume = sfxVolumeSlider.value;
        gameSettings.uiVolume = uiVolumeSlider.value;

        gameSettings.showBattleAnimation = showBattleAnimationToggle.isOn;
        gameSettings.unitSpeed = unitSpeedSlider.value;
        gameSettings.antiAliasing = antiAliasingToggle.isOn ? antiAliasing : PostProcessLayer.Antialiasing.None;
        gameSettings.motionBlurShutterAngle = Mathf.Approximately(motionBlurShutterAngleSlider.value, 0) ? null : motionBlurShutterAngleSlider.value;
        gameSettings.enableBloom = bloomToggle.isOn;
        gameSettings.enableScreenSpaceReflections = screenSpaceReflectionsToggle.isOn;
        gameSettings.enableAmbientOcclusion = ambientOcclusionToggle.isOn;
        gameSettings.shuffleMusic = shuffleMusicToggle.isOn;
        
        PostProcessing.Setup(gameSettings);
        UpdateControls();
    }

    public void Hide() {
        root.SetActive(false);
    }

    public void Close() {
        if (!PersistentData.Get.gameSettings.DiffersFrom(gameSettings))
            level.commands.Enqueue(GameSettingsState.close);
        else {
            shakeTweener?.Complete();
            shakeTweener = buttonsRoot.GetComponent<RectTransform>()
                .DOShakePosition(shakeDuration, shakeStrength, shakeVibrato, 0, false, shakeFadeOut)
                .SetEase(shakeEase);
        }
    }
    public void SetDefaultValues() {
        gameSettings = new GameSettings();
        PostProcessing.Setup(gameSettings);
        UpdateControls();
    }
    public void Cancel() {
        gameSettings = PersistentData.Get.gameSettings.ShallowCopy();
        PostProcessing.Setup(gameSettings);
        level.commands.Enqueue(GameSettingsState.close);
    }
    public void Ok() {
        PersistentData.Get.gameSettings = gameSettings;
        PersistentData.Save();
        level.commands.Enqueue(GameSettingsState.close);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
            Close();
        else if (Input.GetKeyDown(KeyCode.Return))
            Ok();
    }
}