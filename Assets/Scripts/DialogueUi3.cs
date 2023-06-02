using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.Video;
using static DialogueConstants;
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
        StateRunner.Instance.Push(new DialogueState(Instance));
    }
}

public class DialogueState : IDisposableState {

    public DialogueUi3 ui;
    public Dictionary<PersonName, PortraitStack> portraitStacks = new();
    public DialogueContext dialogueContext = new();
    public int? option;

    public DialogueState(DialogueUi3 ui) {
        Assert.IsTrue(ui);
        this.ui = ui;
    }

    [CanBeNull]
    public string Text {
        get => ui.text.enabled ? ui.text.text : null;
        set {
            if (value is { } nonNullString) {
                ui.text.enabled = true;
                ui.text.text = nonNullString;
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
                ui.text.color = personName switch {
                    PersonName.Natalie => Color.white,
                    PersonName.Vladan => Color.yellow,
                    PersonName.JamesWillis => Color.blue,
                    PersonName.LjubisaDragovic => Color.red,
                    _ => Color.white
                };
            }
            else {
                ui.speakerName.enabled = false;
                ui.text.enabled = false;
            }
        }
    }

    public virtual IEnumerator<StateChange> Run {
        get {

            ui.Visible = true;
            Text = null;
            Speaker = null;

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
            while (option is not (0 or 1)) {
                yield return SayContinue(_("Do you like apples?"));
                yield return SelectOption(_("Yes"), _("No"), _("Explain"));
                switch (option) {
                    case 0:
                        yield return Say(_("I like apples too!"));
                        break;
                    case 1:
                        yield return Say(_("I don't like apples either."));
                        break;
                    case 2:
                        var image = ShowImage("apple".LoadAs<Sprite>(), new Vector2(150, 150));
                        yield return SayContinue(_("This is an apple."));
                        yield return Pause(1);
                        yield return AppendContinue(_("It is a fruit."));
                        yield return Pause(1);
                        yield return AppendContinue(_("It is red."));
                        yield return Pause(1);
                        yield return Append(_("It is tasty."));
                        HideImage(image);
                        break;
                }
            }

            yield return Say(_("So we are done here. Goodbye!"));

            Speaker = null;
            Text = null;

            yield return ClearPersons();

            // yield return Say(PersonName.Natalie, _("Hello, my name is Natalie."));
            // yield return Say(PersonName.Natalie, _("I am a student of the Faculty of Mathematics."));
            // yield return Pause(1);
            // yield return AddPerson(PersonName.Vladan, right);
            // yield return AddPerson(PersonName.LjubisaDragovic, right);
            // yield return Say(PersonName.Vladan, _("Hello, my name is Vladan."));
            // yield return Pause(2);
            // yield return ClearPersons();
            // yield break;

            yield break;
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
    public StateChange Pause(float delay) {
        return StateChange.Push("Pause", Wait.ForSeconds(delay));
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

    public StateChange Say(string text, bool waitAnyKeyDown = true, bool append=false) {
        return StateChange.Push("Talk", TalkState(text, waitAnyKeyDown,append));
    }
    public StateChange SayContinue(string text) {
        return Say(text, false, false);
    }
    public StateChange Append(string text) {
        return Say(text, true, true);
    }
    public StateChange AppendContinue(string text) {
        return Say(text, false, true);
    }
    
    public IEnumerator<StateChange> TalkState(string text, bool wait = true, bool append=false) {

        if (append)
            text = " " + text;
        else
            Text = "";
        
        ui.VoiceOverSource.Stop();
        // if (voiceOverClip)
        //     ui.VoiceOverSource.PlayOneShot(voiceOverClip);

        var from = Text;
        foreach (var c in text) {
            Text += c;
            if (Input.anyKeyDown) {
                Text = from + text;
                yield return StateChange.none;
                break;
            }
            yield return StateChange.none;
        }

        ui.VoiceOverSource.Stop();

        if (wait)
            yield return WaitUntilAnyKeyDown();
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
    public void HideVideo((VideoPlayer videoPlayer, RawImage rawImage) video) {
        Object.Destroy(video.videoPlayer.gameObject);
    }

    public StateChange WaitWhile(Func<bool> condition) {
        return StateChange.Push(nameof(WaitWhile), WaitWhileState(condition));
    }
    public IEnumerator<StateChange> WaitWhileState(Func<bool> condition) {
        while (condition())
            yield return StateChange.none;
    }

    public StateChange WaitUntilAnyKeyDown() {
        return WaitWhile(() => !Input.anyKeyDown);
    }

    public StateChange SelectOption(params string[] options) {
        return StateChange.Push(nameof(OptionSelectionState), OptionSelectionState(options));
    }
    public IEnumerator<StateChange> OptionSelectionState(params string[] options) {

        Assert.IsTrue(options.Length > 0);

        var buttons = new List<Button>();
        var totalWidth = 0f;
        for (var i = 0; i < options.Length; i++) {
            var index = i;
            var button = Object.Instantiate(ui.buttonPrefab, ui.transform);
            button.gameObject.SetActive(true);
            button.GetComponentInChildren<TMP_Text>().text = options[index];
            button.onClick.AddListener(() => option = index);
            buttons.Add(button);
            totalWidth += button.GetComponent<RectTransform>().sizeDelta.x;
        }
        totalWidth += ui.buttonSpacing * (buttons.Count - 1);
        Vector2 position = ui.buttonPrefab.GetComponent<RectTransform>().anchoredPosition;
        position -= Vector2.right * totalWidth / 2;
        foreach (var button in buttons) {
            var rectTransform = button.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
            position += Vector2.right * (rectTransform.sizeDelta.x + ui.buttonSpacing);
        }

        option = null;
        while (option == null)
            yield return StateChange.none;

        foreach (var button in buttons)
            Object.Destroy(button.gameObject);
    }
}

public class DialogueContext {
    public Stack<PersonName> personStack = new();
}
public class DialogueSpeaker : IDisposable {
    public DialogueContext dialogueContext;
    public DialogueSpeaker(DialogueContext dialogueContext, PersonName personName) {
        this.dialogueContext = dialogueContext;
        dialogueContext.personStack.Push(personName);
    }
    public void Dispose() {
        dialogueContext.personStack.Pop();
    }
}

public static class DialogueConstants {
    public const int left = 0, right = 1;
}