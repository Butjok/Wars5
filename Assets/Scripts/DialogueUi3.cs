using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Butjok.CommandLine;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.Video;
using static Gettext;
using Object = UnityEngine.Object;

/*
 * TODO:
 * 1) Add a way to change the speaker's name position.
 * 2) Add speech shaking e.g. because of explosion impact.
 */

public class DialogueUi3 : MonoBehaviour {

    private static DialogueUi3 instance;
    public static DialogueUi3 Instance {
        get {
            if (instance)
                return instance;
            instance = FindObjectOfType<DialogueUi3>(true);
            Assert.IsTrue(instance);
            return instance;
        }
    }

    public TMP_Text speakerName;
    public TMP_Text text;
    public PortraitStack[] portraitStacks = { };
    private AudioSource voiceOverSource, sfxSource;
    public Button buttonPrefab;
    public float buttonSpacing = 25;
    public Image spaceKey;
    public bool ShowSpaceKey {
        set => spaceKey.enabled = value;
    }

    public bool Visible {
        set => gameObject.SetActive(value);
    }
    public AudioSource VoiceOverSource {
        get {
            if (!voiceOverSource) {
                voiceOverSource = gameObject.AddComponent<AudioSource>();
                voiceOverSource.spatialBlend = 0;
            }
            return voiceOverSource;
        }
    }
    public AudioSource SfxSource {
        get {
            if (!sfxSource) {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.spatialBlend = 0;
            }
            return sfxSource;
        }
    }

    [Command]
    public static void StartDialogue() {
        GameStateMachine.Instance.Push(new DialogueState(Instance));
    }
}

public class DialogueState : IDisposableState {

    public static Color GetTextColor(PersonName personName) {
        return personName switch {
            PersonName.Natalie => Color.white,
            PersonName.Vladan => Color.yellow,
            PersonName.JamesWillis => Color.blue,
            PersonName.LjubisaDragovic => Color.red,
            _ => Color.white
        };
    }

    public const int left = 0, right = 1;

    public class SkippableSequence {
        public bool shouldSkip;
    }
    public class Integer {
        public int value;
    }

    public DialogueUi3 ui;
    public Dictionary<PersonName, PortraitStack> portraitStacks = new();

    public DialogueState(DialogueUi3 ui) {
        Assert.IsTrue(ui);
        this.ui = ui;
    }

    public string Text {
        get => ui.text.enabled ? ui.text.text : null;
        set {
            if (value != null) {
                ui.text.enabled = true;
                ui.text.text = value;
            }
            else
                ui.text.enabled = false;
        }
    }
    public PersonName? Speaker {
        set {
            if (value is { } personName) {
                ui.speakerName.enabled = true;
                ui.speakerName.text = _(Persons.GetFirstName(personName));
                ui.text.enabled = true;
                ui.text.color = GetTextColor(personName);
            }
            else {
                ui.speakerName.enabled = false;
                ui.text.enabled = false;
            }
        }
    }

    public void Reset() {
        ui.Visible = true;
        ui.ShowSpaceKey = false;
        Text = null;
        Speaker = null;
    }

    public virtual IEnumerator<StateChange> Run {
        get {

            Reset();

            yield return AddPerson(PersonName.Natalie);
            // using (LinesOf(PersonName.Natalie)) {
            //     yield return Say(_("Hello, my name is Natalie."));
            //     yield return AddPerson(PersonName.Vladan, right);
            //     using (LinesOf(PersonName.Vladan)) {
            //         yield return Say(_("Hello, my name is Vladan."));
            //     }
            //     yield return RemovePerson(PersonName.Vladan);
            //     yield return Say(_("I am a student of the Faculty of Mathematics."));
            // }
            // yield return AddPerson(PersonName.Vladan, left);

            // var video = ShowVideo("encoded2".LoadAs<VideoClip>(), new Vector2(500, 500));
            // var completed = false;
            // video.videoPlayer.loopPointReached += _ => completed = true;
            // yield return WaitWhile(() => !completed);
            // HideVideo(video);

            Speaker = PersonName.Natalie;

            yield return Say("We are going to look at some video!");
            
            var (videoPlayer, _) = ShowVideo("encoded2".LoadAs<VideoClip>(), new Vector2(300, 300));
            var completed = false;
            videoPlayer.loopPointReached += _ => completed = true;
            
            PushSkippableSequence();
            yield return WaitWhile(() => !completed);
            HideVideo(videoPlayer);
            yield return SayAndContinue("So...");
            yield return Pause(1);
            yield return AppendAndContinue(" What do you think?");
            PopSkippableSequence();

            var opinionOnVideo = -1;
            yield return SelectOption(value => opinionOnVideo = value, _("It was good"), _("Kinda meh"));
            if (opinionOnVideo == 0)
                yield return Say("I was thinking the same! Pretty cool, right?");
            else
                yield return Say("Yeah, you're right. It could need some work.");

            var opinionOnApples = -1;
            while (opinionOnApples is not (0 or 1)) {
                yield return SayAndContinue(_("Do you like apples?"));
                yield return SelectOption(value => opinionOnApples = value, _("Yes"), _("No"), _("Explain"));
                switch (opinionOnApples) {
                    case 0:
                        yield return Say(_("I like apples too!"));
                        break;
                    case 1:
                        yield return Say(_("I don't like apples either."));
                        break;
                    case 2:
                        var image = ShowImage("apple".LoadAs<Sprite>(), new Vector2(150, 150));
                        PushSkippableSequence();
                        yield return SayAndContinue(_("This is an apple."));
                        yield return Pause(1);
                        yield return Append(_(" It is a fruit."));
                        yield return AppendAndContinue(_(" It is red."));
                        yield return Pause(1);
                        yield return AppendAndContinue(_(" It is tasty."));
                        yield return Pause(.5f);
                        yield return AppendAndContinue(_("."));
                        yield return Pause(.5f);
                        yield return AppendAndContinue(_("."));
                        yield return Pause(1);
                        yield return Append(_(" Got it?"));
                        PopSkippableSequence();
                        HideImage(image);
                        break;
                }
            }

            yield return AddPerson(PersonName.Vladan, right);

            Speaker = PersonName.Vladan;
            yield return Say(_("Well well well! Look who we have here! Natalie with her apples again!"));

            Speaker = PersonName.Natalie;
            yield return Say(_("No, Vladan! Not you again!"));

            Speaker = null;
            Text = null;

            yield return ClearPersons();
        }
    }
    public void Dispose() {
        ui.Visible = false;
    }

    public StateChange AddPerson(PersonName personName, int side = left, Mood mood = default) {
        return StateChange.Push($"Adding {personName}", PersonAdditionState(personName, side, mood));
    }
    public StateChange RemovePerson(PersonName personName) {
        return StateChange.Push($"Removing {personName}", PersonRemovalState(personName));
    }
    public StateChange ClearPersons() {
        return StateChange.Push("Clearing persons", PersonsClearingState());
    }

    public IEnumerator<StateChange> PersonAdditionState(PersonName personName, int side = left, Mood mood = default) {
        var portraitStack = ui.portraitStacks[side];
        portraitStacks.Add(personName, portraitStack);
        var (_, coroutine) = portraitStack.AddPerson(personName, mood);
        while (!portraitStack.IsCompleted(coroutine))
            yield return StateChange.none;
    }
    public IEnumerator<StateChange> PersonRemovalState(PersonName personName) {
        var portraitStack = portraitStacks[personName];
        portraitStacks.Remove(personName);
        var coroutine = portraitStack.RemovePerson(personName);
        while (!portraitStack.IsCompleted(coroutine))
            yield return StateChange.none;
    }
    public IEnumerator<StateChange> PersonsClearingState() {
        portraitStacks.Clear();
        var coroutines = new List<(PortraitStack portraitStack, IEnumerator coroutine)>();
        foreach (var portraitStack in ui.portraitStacks)
            coroutines.Add((portraitStack, portraitStack.Clear()));
        while (coroutines.Any(c => !c.portraitStack.IsCompleted(c.coroutine)))
            yield return StateChange.none;
    }
    public void SetMood(PersonName personName, Mood mood) {
        portraitStacks[personName].SetMood(personName, mood);
    }
    public void PlaySoundEffect(AudioClip audioClip) {
        ui.SfxSource.PlayOneShot(audioClip);
    }

    public StateChange Say(string text, bool waitInput = true, bool append = false) {
        return StateChange.Push(nameof(TalkState), TalkState(text, waitInput, append));
    }
    public StateChange SayAndContinue(string text) {
        return Say(text, false, false);
    }
    public StateChange Append(string text) {
        return Say(text, true, true);
    }
    public StateChange AppendAndContinue(string text) {
        return Say(text, false, true);
    }

    public Stack<SkippableSequence> skippableSequences = new();
    public void PushSkippableSequence() {
        skippableSequences.Push(new SkippableSequence());
    }
    public void PopSkippableSequence() {
        skippableSequences.Pop();
    }

    public IEnumerator<StateChange> TalkState(string text, bool waitInput = true, bool append = false) {

        var sequence = skippableSequences.Count > 0 ? skippableSequences.Peek() : null;

        if (!append)
            Text = "";

        ui.VoiceOverSource.Stop();
        // if (voiceOverClip)
        //     ui.VoiceOverSource.PlayOneShot(voiceOverClip);

        foreach (var c in text) {
            Text += c;
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

    public StateChange WaitWhile(Func<bool> condition) {
        return StateChange.Push(nameof(PauseState), PauseState(condition));
    }
    public StateChange Pause(float delay) {
        var startTime = Time.time;
        return WaitWhile(() => Time.time - startTime < delay);
    }
    public IEnumerator<StateChange> PauseState(Func<bool> condition) {
        var skipGroup = skippableSequences.Count > 0 ? skippableSequences.Peek() : null;
        var startTime = Time.time;
        while (skipGroup is not { shouldSkip: true } && condition()) {
            if (Input.anyKeyDown) {
                skipGroup ??= new SkippableSequence();
                skipGroup.shouldSkip = true;
            }
            yield return StateChange.none;
        }
    }

    public Image ShowImage(Sprite sprite, Vector2? size = null, Vector2 position = default) {
        var go = new GameObject($"Image{sprite}");
        go.transform.SetParent(ui.transform);
        var image = go.AddComponent<Image>();
        image.sprite = sprite;
        image.rectTransform.sizeDelta = size ?? new Vector2(sprite.texture.width, sprite.texture.height);
        image.rectTransform.anchoredPosition = position;
        image.preserveAspect = true;
        return image;
    }
    public void HideImage(Image image) {
        Object.Destroy(image.gameObject);
    }

    public (VideoPlayer videoPlayer, RawImage image) ShowVideo(VideoClip videoClip, Vector2 size, Vector2 position = default) {

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
    public void HideVideo(VideoPlayer videoPlayer) {
        Object.Destroy(videoPlayer.gameObject);
    }

    public StateChange SelectOption(Action<int> setter, params string[] options) {
        return StateChange.Push(nameof(OptionSelectionState), OptionSelectionState(setter, options));
    }
    public IEnumerator<StateChange> OptionSelectionState(Action<int> setter, params string[] options) {

        Assert.IsTrue(options.Length > 0);
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