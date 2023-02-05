using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Antlr4.Runtime;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;
using SyntaxErrorException = System.Data.SyntaxErrorException;

public class ExpressionEvaluator : ArithmeticBaseVisitor<float> {

    private class ExceptionThrower<T> : IAntlrErrorListener<T> {
        public void SyntaxError(TextWriter output, IRecognizer recognizer, T offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e) {
            throw new SyntaxErrorException(msg);
        }
    }

    private static readonly ArithmeticLexer lexer;
    private static readonly ArithmeticParser parser;
    private static readonly ExpressionEvaluator evaluator = new();

    static ExpressionEvaluator() {
        lexer = new ArithmeticLexer(null);
        parser = new ArithmeticParser(null);
        lexer.RemoveErrorListeners();
        parser.RemoveErrorListeners();
        lexer.AddErrorListener(new ExceptionThrower<int>());
        parser.AddErrorListener(new ExceptionThrower<IToken>());
    }

    public static float Evaluate(string text, params (string name, float value)[] variables) {
        
        evaluator.variables.Clear();
        foreach (var (name, value) in variables)
            evaluator.variables.Add(name, value);

        lexer.SetInputStream(new AntlrInputStream(text));
        parser.TokenStream = new CommonTokenStream(lexer);
        return evaluator.Visit(parser.expression());
    }

    [Command]
    public static float Evaluate(string text) {
        return Evaluate(text, ("pi", 3.1415f));
    }

    public readonly Dictionary<string, float> variables = new();

    public override float VisitPower(ArithmeticParser.PowerContext context) {
        return Mathf.Pow(Visit(context.expression(0)), Visit(context.expression(1)));
    }
    public override float VisitMultiplication(ArithmeticParser.MultiplicationContext context) {
        var a = Visit(context.expression(0));
        var b = Visit(context.expression(1));
        return context.@operator.Text switch {
            "*" => a * b,
            "/" => a / b,
            _ => throw new ArgumentOutOfRangeException(context.@operator.Text)
        };
    }
    public override float VisitSummation(ArithmeticParser.SummationContext context) {
        var a = Visit(context.expression(0));
        var b = Visit(context.expression(1));
        return context.@operator.Text switch {
            "+" => a + b,
            "-" => a - b,
            _ => throw new ArgumentOutOfRangeException(context.@operator.Text)
        };
    }
    public override float VisitGrouping(ArithmeticParser.GroupingContext context) {
        return Visit(context.expression());
    }
    public override float VisitBaseExpression(ArithmeticParser.BaseExpressionContext context) {
        var value = Visit(context.atom());
        return context.@operator is { Text: "-" } ? -value : value;
    }
    public override float VisitNumber(ArithmeticParser.NumberContext context) {
        var text = context.GetText();
        var parsed = float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var result);
        Assert.IsTrue(parsed, $"cannot parse float: {text}");
        return result;
    }
    public override float VisitVariable(ArithmeticParser.VariableContext context) {
        var name = context.GetText();
        var found = variables.TryGetValue(name, out var value);
        Assert.IsTrue(found, $"cannot find variable '{name}'");
        return value;
    }
}