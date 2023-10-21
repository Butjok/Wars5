using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.Video;
using static Gettext;
using Object = UnityEngine.Object;

public abstract class DialogueState : StateMachineState {

    protected const int left = 0;
    protected const int right = 1;

    public class SkippableSequence {
        public bool shouldSkip;
    }

    public class PersonAdditionState : StateMachineState {
        public Func<bool> completed;
        public PersonAdditionState(DialogueState dialogueState, PersonName personName, int side, Mood mood = default) : base(dialogueState.stateMachine) {
            var portraitStack = dialogueState.ui.portraitStacks[side];
            var (_, coroutine) = portraitStack.AddPerson(personName, mood);
            completed = () => portraitStack.IsCompleted(coroutine);
        }
        public override IEnumerator<StateChange> Enter {
            get {
                while (!completed())
                    yield return StateChange.none;
            }
        }
    }

    public class PersonRemovalState : StateMachineState {
        public Func<bool> completed;
        public PersonRemovalState(DialogueState dialogueState, PersonName personName) : base(dialogueState.stateMachine) {
            var portraitStack = dialogueState.portraitStacks[personName];
            dialogueState.portraitStacks.Remove(personName);
            var coroutine = portraitStack.RemovePerson(personName);
            completed = () => portraitStack.IsCompleted(coroutine);
        }
        public override IEnumerator<StateChange> Enter {
            get {
                while (!completed())
                    yield return StateChange.none;
            }
        }
    }

    public class PersonsClearingState : StateMachineState {
        public Func<bool> completed;
        public PersonsClearingState(DialogueState dialogueState) : base(dialogueState.stateMachine) {
            var coroutines = new List<(PortraitStack portraitStack, IEnumerator coroutine)>();
            foreach (var portraitStack in dialogueState.ui.portraitStacks)
                coroutines.Add((portraitStack, portraitStack.Clear()));
            completed = () => coroutines.All(pair => pair.portraitStack.IsCompleted(pair.coroutine));
        }
        public override IEnumerator<StateChange> Enter {
            get {
                while (!completed())
                    yield return StateChange.none;
            }
        }
    }

    public class TalkState : StateMachineState {
        DialogueState dialogueState;
        public string text;
        public bool append, waitInput;
        public TalkState(DialogueState dialogueState, string text, bool append, bool waitInput) : base(dialogueState.stateMachine) {
            this.text = text;
            this.dialogueState = dialogueState;
            this.append = append;
            this.waitInput = waitInput;
        }
        public override IEnumerator<StateChange> Enter {
            get {
                var ui = dialogueState.ui;
                var sequence = dialogueState.skippableSequences.Count > 0 ? dialogueState.skippableSequences.Peek() : null;

                if (!append)
                    ui.Text = "";

                ui.VoiceOverSource.Stop();
                // if (voiceOverClip)
                //     ui.VoiceOverSource.PlayOneShot(voiceOverClip);

                foreach (var c in text) {
                    ui.Text += c;
                    if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape)) {
                        sequence ??= new SkippableSequence();
                        sequence.shouldSkip = true;
                        yield return StateChange.none;
                        continue;
                    }
                    if (sequence is not { shouldSkip: true })
                        yield return StateChange.none;
                }

                if (waitInput) {

                    ui.ShowSpaceKey = true;
                    while (!Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.Escape))
                        yield return StateChange.none;
                    yield return StateChange.none;
                    ui.ShowSpaceKey = false;

                    if (sequence != null)
                        sequence.shouldSkip = false;
                }

                ui.VoiceOverSource.Stop();
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
                for (var i = 0; i < options.Length; i++) {
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
                }

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
    private DialogueUi3 ui;
    protected DialogueState(StateMachine stateMachine) : base(stateMachine) {
        ui = stateMachine.TryFind<LevelSessionState>().level.view.dialogueUi;
    }

    public override void Exit() {
        ui.Visible = false;
    }

    public static int GetDefaultSide(PersonName personName) {
        return personName switch {
            PersonName.Natalie => left,
            PersonName.Vladan => right,
            _ => left
        };
    }

    protected StateChange AddPerson(PersonName personName, int side = -1, Mood mood = default) {
        side = side == -1 ? GetDefaultSide(personName) : side;
        var portraitStack = ui.portraitStacks[side];
        portraitStacks.Add(personName, portraitStack);
        return StateChange.Push(new PersonAdditionState(this, personName, side, mood));
    }
    public StateChange RemovePerson(PersonName personName) {
        return StateChange.Push(new PersonRemovalState(this, personName));
    }
    protected StateChange ClearPersons() {
        portraitStacks.Clear();
        return StateChange.Push(new PersonsClearingState(this));
    }

    public void SetMood(PersonName personName, Mood mood) {
        portraitStacks[personName].SetMood(personName, mood);
    }
    public void PlaySoundEffect(AudioClip audioClip) {
        ui.SfxSource.PlayOneShot(audioClip);
    }

    protected StateChange SayWait(string text, bool waitInput = true, bool append = false) {
        return StateChange.Push(new TalkState(this, text, append, waitInput));
    }
    protected StateChange Say(string text) {
        return SayWait(text, false);
    }
    protected StateChange AppendWait(string text) {
        return SayWait(text, true, true);
    }
    protected StateChange Append(string text) {
        return SayWait(text, false, true);
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

    protected RawImage VideoPanelImage => ui.videoPanelImage;
    protected StateChange ShowVideoPanel() {
        return StateChange.Push(new WaitForCoroutineState(stateMachine, ui.ShowVideoPanel()));
    }
    protected StateChange HideVideoPanel() {
        return StateChange.Push(new WaitForCoroutineState(stateMachine, ui.HideVideoPanel()));
    }

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
        ui.MakeDark();
    }
    protected void MakeLight() {
        ui.MakeLight();
    }

    protected StateChange ChooseOption(Action<int> setter, params string[] options) {
        return StateChange.Push(new OptionSelectionState(this, options, setter));
    }
    protected StateChange ChooseYesNo(Action<bool> setter) {
        return StateChange.Push(new OptionSelectionState(this, new[] { _("Yes"), _("No") }, i => setter(i == 0), confirmIndex: 0, cancelIndex: 1));
    }

    protected void Start() {
        ui.Visible = true;
        ui.Reset();

        stateMachine.Find<LevelSessionState>().level.view.cameraRig.enabled = false;
    }
    protected void End() {
        ui.Visible = false;

        stateMachine.Find<LevelSessionState>().level.view.cameraRig.enabled = true;
    }
    protected PersonName? Speaker {
        set => ui.Speaker = value;
    }
    protected string Text {
        set => ui.Text = value;
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