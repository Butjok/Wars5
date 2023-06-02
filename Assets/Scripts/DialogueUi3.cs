using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using static DialogueConstants;
using static Gettext;

/*
 * TODO:
 * 1) Add a way to change the speaker's name position.
 * 2) Add speech shaking e.g. because of explosion impact.
 * 3) Being able to display images and videos anywhere on the screen.
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
            using (LinesOf(PersonName.Natalie)) {
                yield return Say(_("Hello, my name is Natalie."));
                yield return AddPerson(PersonName.Vladan, right);
                using (LinesOf(PersonName.Vladan)) {
                    yield return Say(_("Hello, my name is Vladan."));
                }
                yield return RemovePerson(PersonName.Vladan);
                yield return Say(_("I am a student of the Faculty of Mathematics."));
            }
            yield return AddPerson(PersonName.Vladan, left);
            using (LinesOf(PersonName.Vladan)) {
                yield return Say(_("Hello, my name is Vladan."));
            }
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

    public DialogueSpeaker LinesOf(PersonName personName) {
        return new DialogueSpeaker(dialogueContext, personName);
    }
    public StateChange Say(PersonName personName, string text, bool wait = true, AudioClip voiceOverClip = null) {
        return StateChange.Push($"{personName} talks", TalkState(personName, text, wait, voiceOverClip));
    }
    public StateChange Say(string text, bool wait = true, AudioClip voiceOverClip = null) {
        var personName = dialogueContext.personStack.Peek();
        return StateChange.Push($"{personName} talks", TalkState(personName, text, wait, voiceOverClip));
    }
    public IEnumerator<StateChange> TalkState(PersonName personName, string text, bool wait = true, AudioClip voiceOverClip = null) {

        Speaker = personName;
        Text = "";

        ui.VoiceOverSource.Stop();
        if (voiceOverClip)
            ui.VoiceOverSource.PlayOneShot(voiceOverClip);

        var from = Text;
        var to = from + text;
        foreach (var c in text) {
            Text += c;
            if (Input.anyKeyDown)
                Text = to;
            yield return StateChange.none;
        }

        ui.VoiceOverSource.Stop();

        if (wait) {
            while (!Input.anyKeyDown)
                yield return StateChange.none;
            yield return StateChange.none;
        }
        Text = null;
        Speaker = null;
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