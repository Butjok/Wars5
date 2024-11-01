using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Butjok.CommandLine
{
    public class CommandLineGUI : MonoBehaviour
    {
        public List<string> assemblies = new List<string> { "Assembly-CSharp", "CommandLine" };
        public GUISkin guiSkin;
        public bool dontDestroyOnLoad = true;

        public Theme theme;

        [Command]
        public string Theme {
            get => theme.name;
            set {
                var loaded = Resources.Load<Theme>(value);
                if (loaded)
                    theme = loaded;
                else
                    Debug.LogWarning($"Could not load theme {Interpreter.Format(value)}.");
            }
        }
        [Command]
        public int radius = 5;
        [Command]
        public string prefixFormat = "[{0}/{1}] ";
        [Command]
        public bool includeNamespaceName = true;
        [Command]
        public int maximumMultipleValues = 3;
        public bool show;
        [Command] public bool closeOnEnter = true; 

        private string input = "";
        private int index = -1;
        private readonly List<string> matches = new List<string>();
        private Action action;

        public int depth = -1000;

        [Command]
        private void Awake() {
            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
            FetchCommands();
        }

        public void FetchCommands() {
            Commands.Fetch(assemblies.Select(Assembly.Load), includeNamespaceName);
        }

        private void OnApplicationQuit() {
            History.Save();
        }

        private void OnGUI() {

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.F1) {
                show = !show;
                Event.current.Use();
                action = () => {
                    GUI.FocusControl("CommandLineGUI.Input");
                };
                return;
            }
            
            if (!show)
                return;
            
            void FindCompletions() {
                var pattern = string.Join("[^.]*", input.Select(c => Regex.Escape(c.ToString()))) + "[^.]*$";
                var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                matches.Clear();
                matches.AddRange(Commands.Names
                    .Where(n => regex.IsMatch(n))
                    .OrderBy(n => Levenshtein.Distance(input, n))
                    .ThenBy(n => n));
            }

            GUI.skin = guiSkin;
            GUI.depth = depth;

            if (Event.current.type == EventType.KeyDown) {
                switch (Event.current.keyCode) {

                    case KeyCode.UpArrow:
                    case KeyCode.DownArrow: {
                        Event.current.Use();
                        var offset = Event.current.keyCode == KeyCode.UpArrow ? -1 : 1;
                        if (!History.Lookup(offset))
                            break;

                        input = History.Text;
                        index = -1;
                        FindCompletions();
                        action = () => {
                            var textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                            textEditor.selectIndex = textEditor.cursorIndex = 999;
                        };
                        break;
                    }

                    case KeyCode.Escape:
                        Event.current.Use();
                        if (input == "") {
                            show = false;
                            return;
                        }
                        input = "";
                        index = -1;
                        matches.Clear();
                        break;

                    case KeyCode.Return:
                        Event.current.Use();
                        History.Add(input);
                        var value = Interpreter.Execute(input);
                        if (value != null)
                            Debug.Log(value);
                        input = "";
                        index = -1;
                        matches.Clear();
                        if (closeOnEnter)
                            show = false;
                        break;

                    case KeyCode.Tab: {
                        Event.current.Use();
                        if (matches.Count == 0) {
                            index = -1;
                            FindCompletions();
                            break;
                        }
                        var offset = Event.current.modifiers.HasFlag(EventModifiers.Shift) ? -1 : 1;
                        index = (index + matches.Count + offset) % matches.Count;
                        var completion = matches[index];
                        input = completion + ' ';
                        if (Commands.IsVariable(completion))
                            try {
                                input += Interpreter.Format(Commands.Invoke(completion));
                            }
                            catch (MultipleObjectsException e) {
                                input += Interpreter.Format(e.values.Last());
                            }
                            catch (Exception) {
                                // ignored
                            }
                        action = () => {
                            var textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                            textEditor.cursorIndex = int.MaxValue;
                            textEditor.selectIndex = completion.Length + 1;
                        };
                        break;
                    }
                }
            }
            if (Event.current.type == EventType.Repaint && action != null) {
                action();
                action = null;
            }

            var oldInput = input;
            var rect = GUILayoutUtility.GetRect(new GUIContent(input), GUI.skin.textField, GUILayout.Width(Screen.width));
            GUI.SetNextControlName("CommandLineGUI.Input");

            // GUI.depth--;
            input = GUI.TextField(rect, input).Replace("`","");
            // GUI.depth++;
            GUI.Label(rect, Colorizer.Colorize(input, theme), GUI.skin.GetStyle("inputOverlayText"));
            if (input != oldInput) {
                History.SetText(input);
                index = -1;
                FindCompletions();
            }

            var prefix = string.Format(prefixFormat, index + 1, matches.Count);
            var prefixMaxLength = string.Format(prefixFormat, matches.Count, matches.Count).Length;

            var (low, high) = SlidingWindow(matches.Count, index - radius, index + radius);
            for (var i = low;
                i <= high;
                i++) {
                var name = matches[i];
                var info = "";
                if (Commands.IsVariable(name))
                    try {
                        info = Colorizer.Colorize(Interpreter.Format(Commands.Invoke(name)), theme);
                    }
                    catch (MultipleObjectsException e) {
                        info = "[" + string.Join(", ", e.values.Take(maximumMultipleValues).Select(value => Colorizer.Colorize(Interpreter.Format(value), theme)));
                        if (e.values.Count > maximumMultipleValues)
                            info += "...";
                        info += "]";
                    }
                    catch (Exception e) {
                        while (e.InnerException != null)
                            e = e.InnerException;
                        info = $"<i>{e.Message.Split('\n')[0]}</i>";
                    }
                else if (Commands.TryGetCommand(name, out var methodInfo))
                    info = string.Join(" ", methodInfo.GetParameters().Select(parameter => $"<i><</i><i>{parameter.Name}</i><i>></i>"));
                name = Colorizer.Colorize(name, theme);
                GUILayout.Label(index == i
                    ? prefix.PadRight(prefixMaxLength) + $"<b>{name}</b> {info}"
                    : new string(' ', prefixMaxLength) + $"{name} {info}");
            }
        }

        public static (int low, int high) SlidingWindow(int count, int low, int high) {
            if (low < 0 && high >= count) {
                low = 0;
                high = count - 1;
            }
            else if (low < 0) {
                high = Mathf.Min(count - 1, high - low);
                low = 0;
            }
            else if (high >= count) {
                low = Mathf.Max(0, low - high + count - 1);
                high = count - 1;
            }
            return (low, high);
        }

    }
}