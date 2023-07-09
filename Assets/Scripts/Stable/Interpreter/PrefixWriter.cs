using System;
using System.Collections.Generic;
using System.IO;

public class PrefixWriter {

    public Stack<int?> indentLevels = new();
    public const int indent = 4;
    public TextWriter writer;

    public PrefixWriter(TextWriter writer) {
        indentLevels.Push(0);
        this.writer = writer;
    }

    public string Tabs => indentLevels.Peek() is { } indentLevel ? new string(' ', indentLevel * indent) : "";
    public PrefixWriter BlankLine() {
        writer.WriteLine();
        return this;
    }
    public PrefixWriter BeginCommand(string commandName) {
        writer.Write(Tabs);
        if (commandName[0] is '.' or ':')
            writer.Write(new string(' ', indent));
        writer.Write(commandName + ' ');
        return this;
    }
    public PrefixWriter Command(string commandName) {
        return BeginCommand(commandName).NoBlock();
    }
    public PrefixWriter PushPrefix(string prefix) {
        return Command(prefix + "...");
    }
    public PrefixWriter PopPrefix() {
        return Command("...");
    }
    public PrefixWriter PopAndExecutePrefix() {
        return Command("..!");
    }
    public PrefixWriter Command(string commandName, params object[] values) {
        BeginCommand(commandName).InlineBlock();
        foreach (var value in values)
            Value(value);
        return End();
    }
    public PrefixWriter Value(object value) {
        return BeginCommand(PostfixInterpreter.Format(value)).NoBlock();
    }
    public PrefixWriter Comment(string comment) {
        comment = comment.Replace("\r", "").Trim();
        if (string.IsNullOrWhiteSpace(comment))
            return this;
        foreach (var line in comment.Split('\n')) {
            writer.Write(Tabs);
            writer.WriteLine("// " + line.TrimEnd());
        }
        return this;
    }
    public PrefixWriter NoBlock() {
        if (indentLevels.Peek() is { })
            writer.WriteLine();
        return this;
    }
    private PrefixWriter Block(bool indented) {
        if (indented) {
            if (indentLevels.Peek() is not { } previousIndentLevel)
                throw new Exception();
            indentLevels.Push(previousIndentLevel + 1);
        }
        else
            indentLevels.Push(null);
        writer.Write("{ ");
        if (indented)
            writer.WriteLine();
        return this;
    }
    public PrefixWriter IndentedBlock() => Block(true);
    public PrefixWriter InlineBlock() => Block(false);
    public PrefixWriter InlineBlock(object value) {
        return InlineBlock().Value(value).End();
    }
    public PrefixWriter End() {
        if (indentLevels.Pop() != null) {
            writer.Write(Tabs);
            writer.WriteLine("}");
        }
        else {
            writer.Write("} ");
            if (indentLevels.Peek() != null)
                writer.WriteLine();
        }
        return this;
    }
}