using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.Video;
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
        public override IEnumerator<StateChange> Entry {
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
            var coroutine = portraitStack.RemovePerson(personName);
            completed = () => portraitStack.IsCompleted(coroutine);
        }
        public override IEnumerator<StateChange> Entry {
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
        public override IEnumerator<StateChange> Entry {
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
        public override IEnumerator<StateChange> Entry {
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
                    if (Input.anyKeyDown) {
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
                    while (!Input.anyKeyDown)
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

    public class PauseState : StateMachineState {
        public DialogueState dialogueState;
        public Func<bool> condition;
        public PauseState(DialogueState dialogueState, Func<bool> condition) : base(dialogueState.stateMachine) {
            this.dialogueState = dialogueState;
            this.condition = condition;
        }
        public override IEnumerator<StateChange> Entry {
            get {
                var skipGroup = dialogueState.skippableSequences.Count > 0 ? dialogueState.skippableSequences.Peek() : null;
                while (skipGroup is not { shouldSkip: true } && condition()) {
                    if (Input.anyKeyDown) {
                        skipGroup ??= new SkippableSequence();
                        skipGroup.shouldSkip = true;
                    }
                    yield return StateChange.none;
                }
            }
        }
    }

    public class OptionSelectionState : StateMachineState {
        public DialogueState dialogueState;
        public string[] options;
        public Action<int> setter;
        public OptionSelectionState(DialogueState dialogueState, string[] options, Action<int> setter) : base(dialogueState.stateMachine) {
            Assert.IsTrue(options.Length > 0);
            this.dialogueState = dialogueState;
            this.options = options;
            this.setter = setter;
        }
        public override IEnumerator<StateChange> Entry {
            get {
                var ui = dialogueState.ui;
                
                setter(-1);

                var buttons = new List<Button>();
                var wasSet = false;
                var totalWidth = 0f;
                for (var i = 0; i < options.Length; i++) {
                    var index = i;
                    var button = Object.Instantiate(ui.buttonPrefab, ui.transform);
                    button.gameObject.SetActive(true);
                    button.GetComponentInChildren<TMP_Text>().text = options[index];
                    button.onClick.AddListener(() => {
                        setter(index);
                        wasSet = true;
                    });
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

                while (!wasSet)
                    yield return StateChange.none;

                foreach (var button in buttons)
                    Object.Destroy(button.gameObject);
            }
        }
    }

    public Dictionary<PersonName, PortraitStack> portraitStacks = new();
    public DialogueUi3 ui;
    protected DialogueState(StateMachine stateMachine) : base(stateMachine) {
        ui = stateMachine.TryFind<LevelSessionState>().level.view.dialogueUi;
    }
    
    public override void Exit() {
        ui.Visible = false;
    }

    protected StateChange AddPerson(PersonName personName, int side = left, Mood mood = default) {
        var portraitStack = ui.portraitStacks[side];
        portraitStacks.Add(personName, portraitStack);
        return StateChange.Push(new PersonAdditionState(this, personName, side, mood));
    }
    public StateChange RemovePerson(PersonName personName) {
        var portraitStack = portraitStacks[personName];
        portraitStacks.Remove(personName);
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

    protected StateChange Say(string text, bool waitInput = true, bool append = false) {
        return StateChange.Push(new TalkState(this, text, append, waitInput));
    }
    protected StateChange SayAndContinue(string text) {
        return Say(text, false);
    }
    protected StateChange Append(string text) {
        return Say(text, true, true);
    }
    protected StateChange AppendAndContinue(string text) {
        return Say(text, false, true);
    }

    public Stack<SkippableSequence> skippableSequences = new();
    protected void PushSkippableSequence() {
        skippableSequences.Push(new SkippableSequence());
    }
    protected void PopSkippableSequence() {
        skippableSequences.Pop();
    }

    protected StateChange WaitWhile(Func<bool> condition) {
        return StateChange.Push(new PauseState(this, condition));
    }
    protected StateChange Pause(float delay) {
        var startTime = Time.time;
        return WaitWhile(() => Time.time - startTime < delay);
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

    protected (VideoPlayer videoPlayer, RawImage image) ShowVideo(VideoClip videoClip, Vector2 size, Vector2 position = default) {

        var go = new GameObject($"Video{videoClip}");
        go.transform.SetParent(ui.transform);

        var videoPlayer = go.AddComponent<VideoPlayer>();
        videoPlayer.clip = videoClip;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        var renderTexture = new RenderTexture((int)videoClip.width, (int)videoClip.height, 0);
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.transform.SetParent(ui.transform);
        videoPlayer.Play();

        var rawImage = go.AddComponent<RawImage>();
        rawImage.texture = renderTexture;
        // fits horizontally
        rawImage.rectTransform.sizeDelta = new Vector2(size.x, size.x * ((float)videoClip.height / videoClip.width));
        rawImage.rectTransform.anchoredPosition = position;

        return (videoPlayer, rawImage);
    }
    protected void HideVideo(VideoPlayer videoPlayer) {
        Object.Destroy(videoPlayer.gameObject);
    }

    protected StateChange SelectOption(Action<int> setter, params string[] options) {
        return StateChange.Push(new OptionSelectionState(this, options, setter));
    }

    protected void Show() {
        ui.Visible = true;
        ui.Reset();
    }
    protected void Hide() {
        ui.Visible = false;
    }
    protected PersonName? Speaker {
        set => ui.Speaker = value;
    }
    protected string Text {
        set => ui.Text = value;
    }
}