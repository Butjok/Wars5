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

    public Game game;
    public GameSettings oldSettings;
    public GameObject root;

    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

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

    public void Show(Game game) {

        this.game = game;
        oldSettings = game.settings.ShallowCopy();

        root.SetActive(true);
        UpdateControls();
    }

    public void UpdateControls() {

        masterVolumeSlider.SetValueWithoutNotify(game.settings.masterVolume);
        musicVolumeSlider.SetValueWithoutNotify(game.settings.musicVolume);
        sfxVolumeSlider.SetValueWithoutNotify(game.settings.sfxVolume);

        showBattleAnimationToggle.SetIsOnWithoutNotify(game.settings.showBattleAnimation);
        unitSpeedSlider.SetValueWithoutNotify(game.settings.unitSpeed);
        unitSpeedText.text = Mathf.RoundToInt( game.settings.unitSpeed).ToString();
        antiAliasingToggle.SetIsOnWithoutNotify(game.settings.antiAliasing == antiAliasing);
        motionBlurShutterAngleSlider.SetValueWithoutNotify(game.settings.motionBlurShutterAngle is { } value ? value : 0);
        bloomToggle.SetIsOnWithoutNotify(game.settings.enableBloom);
        screenSpaceReflectionsToggle.SetIsOnWithoutNotify(game.settings.enableScreenSpaceReflections);
        ambientOcclusionToggle.SetIsOnWithoutNotify(game.settings.enableAmbientOcclusion);
        shuffleMusicToggle.SetIsOnWithoutNotify(game.settings.shuffleMusic);
        
        //okButton.interactable =game.settings.DiffersFrom(oldSettings);
        closeButton.interactable = !game.settings.DiffersFrom(oldSettings);
    }

    public void UpdateSettings() {

        game.settings.masterVolume = masterVolumeSlider.value;
        game.settings.musicVolume = musicVolumeSlider.value;
        game.settings.sfxVolume = sfxVolumeSlider.value;

        game.settings.showBattleAnimation = showBattleAnimationToggle.isOn;
        game.settings.unitSpeed = unitSpeedSlider.value;
        game.settings.antiAliasing = antiAliasingToggle.isOn ? antiAliasing : PostProcessLayer.Antialiasing.None;
        game.settings.motionBlurShutterAngle = Mathf.Approximately(motionBlurShutterAngleSlider.value, 0) ? null : motionBlurShutterAngleSlider.value;
        game.settings.enableBloom = bloomToggle.isOn;
        game.settings.enableScreenSpaceReflections = screenSpaceReflectionsToggle.isOn;
        game.settings.enableAmbientOcclusion = ambientOcclusionToggle.isOn;
        game.settings.shuffleMusic = shuffleMusicToggle.isOn;
        
        game.UpdatePostProcessing();

        UpdateControls();
    }

    public void Hide() {
        root.SetActive(false);
    }

    public void Close() {
        if (!game.settings.DiffersFrom(oldSettings))
            GameSettingsState.shouldExit = true;
        else {
            shakeTweener?.Complete();
            shakeTweener = buttonsRoot.GetComponent<RectTransform>()
                .DOShakePosition(shakeDuration, shakeStrength, shakeVibrato, 0, false, shakeFadeOut)
                .SetEase(shakeEase);
        }
    }
    public void SetDefaultValues() {
        game.settings = new GameSettings();
        game.UpdatePostProcessing();
        UpdateControls();
    }
    public void Cancel() {
        game.settings = oldSettings;
        game.UpdatePostProcessing();
        GameSettingsState.shouldExit = true;
    }
    public void Ok() {
        game.settings.Save();
        GameSettingsState.shouldExit = true;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
            Close();
        else if (Input.GetKeyDown(KeyCode.Return))
            Ok();
    }
}