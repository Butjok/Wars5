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

    public bool Visible {
        set => gameObject.SetActive(value);
    }

    [Command]
    public static void StartDialogue() {
        StateRunner.Instance.Push(new DialogueState(Instance));
    }
}

public class DialogueState : IDisposableState {

    public DialogueUi3 ui;
    public Dictionary<PersonName, PortraitStack> portraitStacks = new();

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
            if (value is {} personName) {
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
            yield return Say(PersonName.Natalie, _("Hello, my name is Natalie."));
            yield return Say(PersonName.Natalie, _("I am a student of the Faculty of Mathematics."));
            yield return Pause(1);
            yield return AddPerson(PersonName.Vladan, right);
            yield return AddPerson(PersonName.LjubisaDragovic, right);
            yield return Say(PersonName.Vladan, _("Hello, my name is Vladan."));
            yield return Pause(2);
            yield return ClearPersons();
            yield break;
        }
    }
    public void Dispose() {
        ui.Visible = false;
    }

    public StateChange AddPerson(PersonName personName, int side = left, Mood mood = default) {
        return StateChange.Push($"Adding {personName}", PersonAdditionState(personName,side,mood));
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
            yield return  StateChange.none;
    }
    public IEnumerator<StateChange> PersonRemovalState(PersonName personName) {
        var portraitStack = portraitStacks[personName];
        portraitStacks.Remove(personName);
        var coroutine = portraitStack.RemovePerson(personName);
        while (!portraitStack.IsCompleted(coroutine))
            yield return  StateChange.none;
    }
    public IEnumerator<StateChange> PersonsClearingState() {
        portraitStacks.Clear();
        var coroutines = new List<(PortraitStack portraitStack, IEnumerator coroutine)>();
        foreach (var portraitStack in ui.portraitStacks)
            coroutines.Add((portraitStack, portraitStack.Clear()));
        while (coroutines.Any(c => !c.portraitStack.IsCompleted(c.coroutine)))
            yield return  StateChange.none;
    }
    public void SetMood(PersonName personName, Mood mood) {
        portraitStacks[personName].SetMood(personName, mood);
    }

    public StateChange Say(PersonName personName, string text, bool wait=true) {
        return StateChange.Push($"{personName} talks", TalkState(personName,text, wait));
    }
    public IEnumerator<StateChange> TalkState(PersonName personName, string text, bool wait=true) {
        Speaker = personName;
        Text = "";
        var from = Text;
        var to = from + text;
        foreach (var c in text) {
            Text += c;
            if (Input.anyKeyDown)
                Text = to;
            yield return  StateChange.none;
        }
        if (wait) {
            while (!Input.anyKeyDown)
                yield return  StateChange.none;
            yield return  StateChange.none;
        }
        Text = null;
        Speaker = null;
    }

}

public static class DialogueConstants {
    public const int left = 0, right = 1;
}