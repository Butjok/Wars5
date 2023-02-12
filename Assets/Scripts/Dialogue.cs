using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

/*
 
@nata Hello there! @next
      Welcome to the Wars3d! An amazing strategy game! @next

@vlad What are you saying? @next

@nata I dont know what to say... @next
@nervous Probably... @3 @pause we should have done something different... @next

@nata @happy You probably did not know who you are messing with! @next
@nata @normal Enough said. @next

*/

public class Dialogue : IDisposable {

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

    public IEnumerable<StateChange> Play(string script) {
        var tokens = script.Replace("\r", " ").Replace("\n", " ").Replace("  ", " ").Replace("  ", " ").Trim()
            .Split(" ", StringSplitOptions.RemoveEmptyEntries).Where(part=>!string.IsNullOrWhiteSpace(part));
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
                        ui.text.text = stringBuilder.ToString();
                        var start = Time.time;
                        var delay = stack.Pop();
                        while (Time.time < start + delay)
                            yield return StateChange.none;
                        break;
                    case "@normal":
                        break;
                    case "@happy":
                        break;
                    case "@sad":
                        break;
                    case "@nervous":
                        break;
                    case "@worried":
                        break;
                    case "@next":
                        ui.text.text = stringBuilder.ToString();
                        while (!Input.GetKeyDown(KeyCode.Space))
                            yield return StateChange.none;
                        yield return StateChange.none;
                        stringBuilder.Clear();
                        break;
                    default:
                        var parsed = float.TryParse(token.Substring(1), NumberStyles.Any, CultureInfo.InvariantCulture, out var value);
                        Assert.IsTrue(parsed,token);
                        stack.Push(value);
                        break;
                }
            else {
                stringBuilder.Append(token);
                stringBuilder.Append(' ');
            }
        }
    }

    public void SetSpeaker(DialogueSpeaker speaker) {
        
    }
}