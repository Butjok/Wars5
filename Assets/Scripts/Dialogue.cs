using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

public class Dialogue : IDisposable {

    public const string next = " @next ";
    public const string nata = " @nata ";
    public const string pause = " @pause ";
    public const string vlad = " @vlad ";
    public const string happy = " @happy ";
    public const string normal = " @normal ";
    public const string mad = " @mad ";
    public const string shocked = " @shocked ";
    public const string sad = " @sad ";
    public const string laughing = " @laughing ";
    public const string intimate = " @intimate ";
    public const string worried = " @worried ";
    public const string crying = " @crying ";
    public static string Pause(float delay) => $" @{delay.ToString(CultureInfo.InvariantCulture)} {pause} ";
    
    public DialogueUi ui;

    public Dialogue() {
        ui = DialogueUi.Instance;
        ui.Show = true;
    }
    public void Dispose() {
        ui.Show = false;
    }

    public StringBuilder stringBuilder = new();
    public DialogueSpeaker speaker;
    public Stack<float> stack = new();
    public Dictionary<DialogueSpeaker, DialogueSpeaker.Mood> moods = new();

    public IEnumerable<StateChange> Play(string script) {

        ui.ShowSpaceBarKey = false;
        ui.ClearText();
        
        var textChanged = true;
        
        var tokens = script.Replace("\r", " ").Replace("\n", " ").Replace("  ", " ").Replace("  ", " ").Trim()
            .Split(" ", StringSplitOptions.RemoveEmptyEntries).Where(part => !string.IsNullOrWhiteSpace(part));
        foreach (var token in tokens) {

            if (token[0] == '@')
                switch (token) {

                    case "@nata":
                        SetSpeaker(DialogueSpeaker.Natalie);
                        break;

                    case "@vlad":
                        SetSpeaker(DialogueSpeaker.Vladan);
                        break;

                    case "@pause":
                        if (textChanged) {
                            textChanged = false;
                            ui.AppendText(stringBuilder.ToString());
                            stringBuilder.Clear();
                        }
                        var start = Time.time;
                        var delay = stack.Pop();
                        while (Time.time < start + delay)
                            yield return StateChange.none;
                        break;

                    case "@next":
                        if (textChanged) {
                            textChanged = false;
                            ui.AppendText(stringBuilder.ToString());
                            stringBuilder.Clear();
                        }
                        ui.ShowSpaceBarKey = true;
                        while (!Input.GetKeyDown(KeyCode.Space))
                            yield return StateChange.none;
                        yield return StateChange.none;
                        ui.ShowSpaceBarKey = false;
                        ui.ClearText();
                        break;

                    default: {
                        if (float.TryParse(token.Substring(1), NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                            stack.Push(value);
                        else if (Enum.TryParse(token.Substring(1), true, out DialogueSpeaker.Mood mood)) {
                            moods[speaker] = mood;
                            UpdatePortrait();
                        }
                        else
                            throw new AssertionException(token, null);
                        break;
                    }
                }

            else {
                stringBuilder.Append(token);
                stringBuilder.Append(' ');
                textChanged = true;
            }
        }
    }
    
    public DialogueSpeaker.Mood GetMood(DialogueSpeaker speaker, DialogueSpeaker.Mood defaultMood = default) {
        if (!moods.TryGetValue(speaker, out var mood)) {
            mood = defaultMood;
            moods.Add(speaker, mood);
        }
        return mood;
    }
    
    public void UpdatePortrait() {
        var mood = GetMood(speaker);
        if (!speaker.TryGetPortrait(mood, out var portrait) && !speaker.TryGetPortrait(DialogueSpeaker.Mood.Normal, out portrait))
            portrait = null;
        // TODO:
        ui.portrait.text = $"{speaker.name} [{GetMood(speaker)}]";
    }

    public void SetSpeaker(DialogueSpeaker speaker) {
        this.speaker = speaker;
        UpdatePortrait();
    }
}