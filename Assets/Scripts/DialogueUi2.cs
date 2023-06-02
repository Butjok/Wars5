using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class DialoguePlayer : IDisposable {

    public const string personNamePrefix = "person:";
    public const string moodNamePrefix = "mood:";

    // Special keywords to control the dialogue flow.
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
    public const string nervous = " @nervous ";

    public static string Pause(float delay) => $" @{delay.ToString(CultureInfo.InvariantCulture)} {pause} ";

    public DialogueUi ui;
    public DialoguePlayer() {

        ui = DialogueUi.Instance;

        moods.Clear();

        ui.ShowSpaceBarKey = false;
        ui.ClearText();
        ui.Show = true;
    }
    public void Dispose() {
        ui.Show = false;
    }

    public StringBuilder stringBuilder = new();
    public PersonName speaker;
    public Stack<float> stack = new();
    public Dictionary<PersonName, Mood> moods = new();

    public IEnumerable<StateChange> Play(string script) {

        var textChanged = true;
        void FlushText() {
            if (!textChanged)
                return;
            textChanged = false;
            ui.AppendText(stringBuilder.ToString());
            stringBuilder.Clear();
        }

        var tokens = script.Replace("\r", " ").Replace("\n", " ").Replace("  ", " ").Replace("  ", " ").Trim()
            .Split(" ", StringSplitOptions.RemoveEmptyEntries).Where(part => !string.IsNullOrWhiteSpace(part));
        foreach (var token in tokens) {

            if (token[0] == '@')
                switch (token) {

                    case "@nata":
                        SetSpeaker(PersonName.Natalie);
                        break;

                    case "@vlad":
                        SetSpeaker(PersonName.Vladan);
                        break;

                    case "@pause":
                        FlushText();
                        var start = Time.time;
                        var delay = stack.Pop();
                        while (Time.time < start + delay)
                            yield return StateChange.none;
                        break;

                    case "@next":
                        FlushText();
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

                        else if (token.StartsWith(personNamePrefix)) {
                            var personName = token.Substring(personNamePrefix.Length);
                            Assert.IsTrue(Enum.TryParse(personName, true, out PersonName person), $"Invalid person name: {personName}");
                            SetSpeaker(person);
                        }

                        else if (token.StartsWith(moodNamePrefix)) {
                            var moodName = token.Substring(moodNamePrefix.Length);
                            Assert.IsTrue(Enum.TryParse(moodName, true, out Mood mood), $"Invalid mood name: {moodName}");

                        }

                        else if (Enum.TryParse(token.Substring(1), true, out Mood mood)) {
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

    public Mood GetMood(PersonName speaker, Mood defaultMood = default) {
        if (!moods.TryGetValue(speaker, out var mood)) {
            mood = defaultMood;
            moods.Add(speaker, mood);
        }
        return mood;
    }

    public void UpdatePortrait() {
        var mood = GetMood(speaker);
        var portrait = Persons.TryGetPortrait(speaker, mood);
        // TODO:
        ui.portrait.text = $"{Persons.GetFirstName(speaker)} [{GetMood(speaker)}]";
    }

    public void SetSpeaker(PersonName speaker) {
        this.speaker = speaker;
        UpdatePortrait();
    }
}