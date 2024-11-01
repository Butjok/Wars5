using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.Video;
using static Gettext;
using Object = UnityEngine.Object;

public abstract class DialogueState : StateMachineState {

    protected const int left = 0;
    protected const int right = 1;
    public bool popped = false;

    public class SkippableSequence {
        public bool shouldSkip;
    }

    public class TalkState : StateMachineState {
        DialogueState dialogueState;
        public string text, voiceOverClipName;
        public bool append, waitInput;
        public AudioClip voiceOver;
        public TalkState(DialogueState dialogueState, string text, bool append, bool waitInput, string voiceOverClipName, AudioClip voiceOver) : base(dialogueState.stateMachine) {
            this.text = text;
            this.dialogueState = dialogueState;
            this.append = append;
            this.waitInput = waitInput;
            this.voiceOverClipName = voiceOverClipName;
            this.voiceOver = voiceOver;
        }
        public override IEnumerator<StateChange> Enter {
            get {
                var ui = dialogueState.ui;
                var sequence = dialogueState.skippableSequences.Count > 0 ? dialogueState.skippableSequences.Peek() : null;

                //ui.VoiceOverSource.Stop();
                // if (voiceOverClip)
                //     ui.VoiceOverSource.PlayOneShot(voiceOverClip);

                /*VoiceOver.Stop();
                if (voiceOverClipName != null)
                    VoiceOver.PlayOneShot(voiceOverClipName);
                else if (voiceOver != null)
                    VoiceOver.PlayOneShot(voiceOver);*/
                
                var typingAnimation = ui.TextTypingAnimation(append ? ui.speechText.text : "", text);
                while (typingAnimation.MoveNext()) {
                    if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape)) {
                        sequence ??= new SkippableSequence();
                        sequence.shouldSkip = true;
                    }
                    yield return StateChange.none;
                }
                
                /*foreach (var c in text) {
                    ui.Text += c;
                    if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape)) {
                        sequence ??= new SkippableSequence();
                        sequence.shouldSkip = true;
                        yield return StateChange.none;
                        continue;
                    }
                    if (sequence is not { shouldSkip: true })
                        yield return StateChange.none;
                }*/

                if (waitInput) {

                    ui.ShowSpaceBarKey = true;
                    while (!Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.Escape))
                        yield return StateChange.none;
                    VoiceOver.Stop();
                    yield return StateChange.none;
                    ui.ShowSpaceBarKey = false;

                    if (sequence != null)
                        sequence.shouldSkip = false;
                }

                //ui.VoiceOverSource.Stop();
            }
        }
    }

    public class WaitState : StateMachineState {
        public DialogueState dialogueState;
        public Func<bool> condition;
        public float startTime;
        public WaitState(DialogueState dialogueState, Func<bool> condition) : base(dialogueState.stateMachine) {
            this.dialogueState = dialogueState;
            this.condition = condition;
        }
        public WaitState(DialogueState dialogueState, float duration) : base(dialogueState.stateMachine) {
            this.dialogueState = dialogueState;
            condition = () => Time.time < startTime + duration;
        }
        public override IEnumerator<StateChange> Enter {
            get {
                startTime = Time.time;
                var skipGroup = dialogueState.skippableSequences.Count > 0 ? dialogueState.skippableSequences.Peek() : null;
                if (skipGroup != null)
                    while (skipGroup is not { shouldSkip: true } && condition()) {
                        if (Input.GetKeyDown(KeyCode.Space)) {
                            skipGroup ??= new SkippableSequence();
                            skipGroup.shouldSkip = true;
                        }
                        yield return StateChange.none;
                    }
                else
                    while (condition())
                        yield return StateChange.none;
            }
        }
    }

    public class OptionSelectionState : StateMachineState {
        public DialogueState dialogueState;
        public string[] options;
        public Action<int> setter;
        public int? confirmIndex, cancelIndex;
        public OptionSelectionState(DialogueState dialogueState, string[] options, Action<int> setter,
            int? confirmIndex = null, int? cancelIndex = null) : base(dialogueState.stateMachine) {
            Assert.IsTrue(options.Length > 0);
            this.dialogueState = dialogueState;
            this.options = options;
            this.setter = setter;
            this.confirmIndex = confirmIndex;
            this.cancelIndex = cancelIndex;
        }
        public override IEnumerator<StateChange> Enter {
            get {
                var ui = dialogueState.ui;

                setter(-1);
                var wasSet = false;

                void Set(int index) {
                    setter(index);
                    wasSet = true;
                }

                var buttons = new List<Button>();
                var totalWidth = 0f;
                /*for (var i = 0; i < options.Length; i++) {
                    var index = i;
                    var button = Object.Instantiate(ui.buttonPrefab, ui.transform);
                    button.gameObject.SetActive(true);
                    button.GetComponentInChildren<TMP_Text>().text = options[index];
                    button.onClick.AddListener(() => Set(index));
                    buttons.Add(button);
                    totalWidth += button.GetComponent<RectTransform>().sizeDelta.x;
                }
                totalWidth += ui.buttonSpacing * (buttons.Count - 1);
                var position = ui.buttonPrefab.GetComponent<RectTransform>().anchoredPosition;
                position -= Vector2.right * totalWidth / 2;
                foreach (var button in buttons) {
                    var rectTransform = button.GetComponent<RectTransform>();
                    rectTransform.anchoredPosition = position;
                    position += Vector2.right * (rectTransform.sizeDelta.x + ui.buttonSpacing);
                }*/

                while (!wasSet) {
                    if (confirmIndex is { } actualConfirmIndex && Input.GetKeyDown(KeyCode.Return))
                        Set(actualConfirmIndex);
                    else if (cancelIndex is { } actualCancelIndex && Input.GetKeyDown(KeyCode.Escape))
                        Set(actualCancelIndex);
                    yield return StateChange.none;
                }

                foreach (var button in buttons)
                    Object.Destroy(button.gameObject);
            }
        }
    }

    private Dictionary<PersonName, PortraitStack> portraitStacks = new();
    public DialogueUi4 ui;
    protected DialogueState(StateMachine stateMachine) : base(stateMachine) {
        ui = stateMachine.TryFind<LevelSessionState>().level.view.newDialogueUi;
    }

    public override void Exit() {
        ui.Hide();
        popped = true;
    }

    public static DialogueUi4.Side GetDefaultSide(PersonName personName) {
        return personName switch {
            PersonName.Natalie => DialogueUi4.Side.Left,
            PersonName.Vladan => DialogueUi4.Side.Right,
            _ =>   DialogueUi4.Side.Left,
        };
    }

    protected StateChange AddPerson(PersonName personName, DialogueUi4.Side ?side = null, Mood mood = default, bool flipped=false) {
        var actualSide = side ?? GetDefaultSide(personName);
        ui.SetPortrait(actualSide, personName, mood, flipped:flipped);
        return StateChange.none;
    }
    public StateChange RemovePerson(DialogueUi4.Side side, bool flipped=false) {
        ui.SetPortrait(side,null, flipped:flipped);
        return StateChange.none;
    }
    protected StateChange ClearPersons() {
        RemovePerson(DialogueUi4.Side.Left);
        RemovePerson(DialogueUi4.Side.Right);
        return StateChange.none;
    }

    public void SetMood(PersonName personName, Mood mood) {
        portraitStacks[personName].SetMood(personName, mood);
    }
    public void PlaySoundEffect(AudioClip audioClip) {
        //ui.SfxSource.PlayOneShot(audioClip);
    }

    protected StateChange SayWait(string text, bool waitInput = true, bool append = false, string voiceOverClipName = null, AudioClip voiceOver = null) {
        return StateChange.Push(new TalkState(this, text, append, waitInput,  voiceOverClipName, voiceOver));
    }
    protected StateChange Say(string text, AudioClip voiceOver = null) {
        return SayWait(text, false,  false, voiceOver: voiceOver);
    }
    protected StateChange AppendWait(string text,  AudioClip voiceOver=null) {
        return SayWait(text, true, true, voiceOver: voiceOver);
    }
    protected StateChange Append(string text,  AudioClip voiceOver=null) {
        return SayWait(text, false, true, voiceOver: voiceOver);
    }

    public Stack<SkippableSequence> skippableSequences = new();
    protected void PushSkippableSequence() {
        skippableSequences.Push(new SkippableSequence());
    }
    protected void PopSkippableSequence() {
        skippableSequences.Pop();
    }

    protected StateChange WaitWhile(Func<bool> condition) {
        return StateChange.Push(new WaitState(this, condition));
    }
    protected StateChange Wait(float delay) {
        var startTime = Time.time;
        return WaitWhile(() => Time.time - startTime < delay);
    }
    protected StateChange Wait(IEnumerator coroutine) {
        return StateChange.Push(new WaitForCoroutineState(stateMachine, coroutine));
    }

    protected Image ShowImage(Sprite sprite, Vector2? size = null, Vector2 position = default) {
        var go = new GameObject($"Image{sprite}");
        go.transform.SetParent(ui.transform);
        var image = go.AddComponent<Image>();
        image.sprite = sprite;
        image.rectTransform.sizeDelta = size ?? new Vector2(sprite.texture.width, sprite.texture.height);
        image.rectTransform.anchoredPosition = position;
        image.preserveAspect = true;
        return image;
    }
    protected void HideImage(Image image) {
        Object.Destroy(image.gameObject);
    }

    public class Video {
        public VideoPlayer player;
        public RawImage rawImage;
        private Func<bool> completed;
        public Video(VideoPlayer videoPlayer, RawImage image, Func<bool> completed) {
            player = videoPlayer;
            rawImage = image;
            this.completed = completed;
        }
        public bool Completed => completed();
    }

    /*protected RawImage VideoPanelImage => ui.videoPanelImage;
    protected StateChange ShowVideoPanel() {
        return StateChange.Push(new WaitForCoroutineState(stateMachine, ui.ShowVideoPanel()));
    }
    protected StateChange HideVideoPanel() {
        return StateChange.Push(new WaitForCoroutineState(stateMachine, ui.HideVideoPanel()));
    }*/

    protected Video CreateVideo(VideoClip videoClip, float? width = null, Vector2 position = default, RawImage target = null) {

        var go = new GameObject($"Video{videoClip}");
        go.transform.SetParent(ui.transform);

        var videoPlayer = go.AddComponent<VideoPlayer>();
        videoPlayer.clip = videoClip;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        var renderTexture = new RenderTexture((int)videoClip.width, (int)videoClip.height, 0);
        renderTexture.useMipMap = true;
        renderTexture.autoGenerateMips = true;
        renderTexture.wrapMode= TextureWrapMode.Clamp;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.transform.SetParent(ui.transform);
        bool completed = false;
        videoPlayer.loopPointReached += _ => completed = true;
        videoPlayer.Play();
        videoPlayer.playbackSpeed = 0;

        float actualWidth;
        if (target)
            actualWidth = target.rectTransform.sizeDelta.x;
        else {
            Assert.IsTrue(width != null);
            actualWidth = (float)width;
            target = go.AddComponent<RawImage>();
            target.rectTransform.anchoredPosition = position;
        }
        // fits horizontally
        target.rectTransform.sizeDelta = new Vector2(actualWidth, actualWidth * ((float)videoClip.height / videoClip.width));
        target.enabled = true;
        target.texture = renderTexture;

        return new Video(videoPlayer, target, () => completed);
    }
    protected void DestroyVideo(Video video) {
        video.rawImage.enabled = false;
        Object.Destroy(video.player.targetTexture);
        Object.Destroy(video.player.gameObject);
    }

    protected void MakeDark() {
        //ui.MakeDark();
    }
    protected void MakeLight() {
        //ui.MakeLight();
    }

    protected StateChange ChooseOption(Action<int> setter, params string[] options) {
        return StateChange.Push(new OptionSelectionState(this, options, setter));
    }
    protected StateChange ChooseYesNo(Action<bool> setter) {
        return StateChange.Push(new OptionSelectionState(this, new[] { _("Yes"), _("No") }, i => setter(i == 0), confirmIndex: 0, cancelIndex: 1));
    }

    protected void Start() {
        ui.Show();
        ui.SetSpeaker(null);
        ui.ClearText();

        //stateMachine.Find<LevelSessionState>().level.view.cameraRig.enabled = false;
    }
    protected void End() {
        ui.Hide();

        //stateMachine.Find<LevelSessionState>().level.view.cameraRig.enabled = true;
    }
    protected PersonName? Speaker {
        set => ui.SetSpeaker(value, true);
    }
    protected void SetText(string text) {
        ui.TypeText(text);
    }
    protected void AppendText(string text) {
        ui.TypeText(text, ui.speechText.text);
    }
}

public class WaitForCoroutineState : StateMachineState {
    public IEnumerator coroutine;
    public WaitForCoroutineState(StateMachine stateMachine, IEnumerator coroutine) : base(stateMachine) {
        this.coroutine = coroutine;
    }
    public override IEnumerator<StateChange> Enter {
        get {
            while (coroutine.MoveNext())
                yield return StateChange.none;
        }
    }
}