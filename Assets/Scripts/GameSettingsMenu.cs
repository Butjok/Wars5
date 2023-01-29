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
        oldSettings = main.persistentData.gameSettings.ShallowCopy();

        root.SetActive(true);
        UpdateControls();
    }

    public void UpdateControls() {

        masterVolumeSlider.SetValueWithoutNotify(main.persistentData.gameSettings.masterVolume);
        musicVolumeSlider.SetValueWithoutNotify(main.persistentData.gameSettings.musicVolume);
        sfxVolumeSlider.SetValueWithoutNotify(main.persistentData.gameSettings.sfxVolume);
        uiVolumeSlider.SetValueWithoutNotify(main.persistentData.gameSettings.uiVolume);

        showBattleAnimationToggle.SetIsOnWithoutNotify(main.persistentData.gameSettings.showBattleAnimation);
        unitSpeedSlider.SetValueWithoutNotify(main.persistentData.gameSettings.unitSpeed);
        unitSpeedText.text = Mathf.RoundToInt( main.persistentData.gameSettings.unitSpeed).ToString();
        antiAliasingToggle.SetIsOnWithoutNotify(main.persistentData.gameSettings.antiAliasing == antiAliasing);
        motionBlurShutterAngleSlider.SetValueWithoutNotify(main.persistentData.gameSettings.motionBlurShutterAngle is { } value ? value : 0);
        bloomToggle.SetIsOnWithoutNotify(main.persistentData.gameSettings.enableBloom);
        screenSpaceReflectionsToggle.SetIsOnWithoutNotify(main.persistentData.gameSettings.enableScreenSpaceReflections);
        ambientOcclusionToggle.SetIsOnWithoutNotify(main.persistentData.gameSettings.enableAmbientOcclusion);
        shuffleMusicToggle.SetIsOnWithoutNotify(main.persistentData.gameSettings.shuffleMusic);
        
        //okButton.interactable =game.settings.DiffersFrom(oldSettings);
        closeButton.interactable = !main.persistentData.gameSettings.DiffersFrom(oldSettings);
    }

    public void UpdateSettings() {

        main.persistentData.gameSettings.masterVolume = masterVolumeSlider.value;
        main.persistentData.gameSettings.musicVolume = musicVolumeSlider.value;
        main.persistentData.gameSettings.sfxVolume = sfxVolumeSlider.value;
        main.persistentData.gameSettings.uiVolume = uiVolumeSlider.value;

        main.persistentData.gameSettings.showBattleAnimation = showBattleAnimationToggle.isOn;
        main.persistentData.gameSettings.unitSpeed = unitSpeedSlider.value;
        main.persistentData.gameSettings.antiAliasing = antiAliasingToggle.isOn ? antiAliasing : PostProcessLayer.Antialiasing.None;
        main.persistentData.gameSettings.motionBlurShutterAngle = Mathf.Approximately(motionBlurShutterAngleSlider.value, 0) ? null : motionBlurShutterAngleSlider.value;
        main.persistentData.gameSettings.enableBloom = bloomToggle.isOn;
        main.persistentData.gameSettings.enableScreenSpaceReflections = screenSpaceReflectionsToggle.isOn;
        main.persistentData.gameSettings.enableAmbientOcclusion = ambientOcclusionToggle.isOn;
        main.persistentData.gameSettings.shuffleMusic = shuffleMusicToggle.isOn;
        
        main.UpdatePostProcessing();

        UpdateControls();
    }

    public void Hide() {
        root.SetActive(false);
    }

    public void Close() {
        if (!main.persistentData.gameSettings.DiffersFrom(oldSettings))
            main.commands.Enqueue(GameSettingsState.close);
        else {
            shakeTweener?.Complete();
            shakeTweener = buttonsRoot.GetComponent<RectTransform>()
                .DOShakePosition(shakeDuration, shakeStrength, shakeVibrato, 0, false, shakeFadeOut)
                .SetEase(shakeEase);
        }
    }
    public void SetDefaultValues() {
        main.persistentData.gameSettings = new GameSettings();
        main.UpdatePostProcessing();
        UpdateControls();
    }
    public void Cancel() {
        main.persistentData.gameSettings = oldSettings;
        main.UpdatePostProcessing();
        main.commands.Enqueue(GameSettingsState.close);
    }
    public void Ok() {
        var persistentData = PersistentData.Read();
        persistentData.gameSettings = main.persistentData.gameSettings;
        persistentData.Save();
        main.commands.Enqueue(GameSettingsState.close);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
            Close();
        else if (Input.GetKeyDown(KeyCode.Return))
            Ok();
    }
}