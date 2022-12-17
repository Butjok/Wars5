using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using Butjok.CommandLine;
using UnityEngine.Assertions;

public class AiScriptInterpreter {

    private readonly AiScriptLexer lexer = new(null);
    private readonly AiScriptParser parser = new(null);
    public readonly Dictionary<string, dynamic> environment;
    
    public AiScriptInterpreter(Dictionary<string, dynamic> environment=null) {
        lexer.RemoveErrorListeners();
        parser.RemoveErrorListeners();
        lexer.AddErrorListener(new ExceptionThrower<int>());
        parser.AddErrorListener(new ExceptionThrower<IToken>());
        this.environment = environment ?? new Dictionary<string, dynamic>();
    }

    public dynamic Evaluate(string input) {
        lexer.SetInputStream( new AntlrInputStream("(do " + input + ")"));
        parser.TokenStream = new CommonTokenStream(lexer);
        return EvaluateExpression(AiScriptVisitor.instance.Visit(parser.expression()));
    }

    private dynamic EvaluateExpression(dynamic expression) {
        switch (expression) {

            case bool value:
                return value;

            case int value:
                return value;

            case AiScriptVisitor.Symbol value:
                return environment[value.name];

            case string value:
                return value;

            case List<dynamic> list:

                if (list.Count == 0)
                    return new List<dynamic>();

                else {
                    var rest = list.Skip(1).ToArray();
                    if (list[0] is AiScriptVisitor.Symbol head)
                        switch (head.name) {

                            case "quote":
                                Assert.AreEqual(1, rest.Length);
                                return rest[0];

                            case "if":
                                Assert.AreEqual(3, rest.Length);
                                return EvaluateExpression(EvaluateExpression(rest[0]) ? rest[1] : rest[2]);

                            case "when":
                                Assert.AreEqual(2, rest.Length);
                                return EvaluateExpression(rest[0]) ? EvaluateExpression(rest[1]) : null;

                            case "do":
                                dynamic result = null;
                                foreach (var subexpression in rest)
                                    result = EvaluateExpression(subexpression);
                                return result;

                            case "set!":
                                Assert.AreEqual(2, rest.Length);
                                var symbol = EvaluateExpression(rest[0]) as AiScriptVisitor.Symbol;
                                Assert.IsNotNull(symbol);
                                return environment[symbol.name] = EvaluateExpression(rest[1]);

                            case "+":
                            case "*":
                            case "/":
                            case "=":
                                Assert.AreEqual(2, rest.Length);
                                var a = EvaluateExpression(rest[0]);
                                var b = EvaluateExpression(rest[1]);
                                return head.name switch {
                                    "+" => a + b,
                                    "-" => a - b,
                                    "*" => a * b,
                                    "/" => a / b,
                                    "=" => a == b
                                };

                            case "-":
                                Assert.IsTrue(rest.Length is 1 or 2);
                                return rest.Length == 1 ? -EvaluateExpression(rest[0]) : EvaluateExpression(rest[0]) - EvaluateExpression(rest[1]);

                            case "not":
                                Assert.IsTrue(rest.Length == 1);
                                return !EvaluateExpression(rest[0]);
                        }
                    
                    return null;
                }

            default:
                throw new ArgumentOutOfRangeException(expression.ToString());
        }
    }
    
    private class ExceptionThrower<T> : IAntlrErrorListener<T> {
        public void SyntaxError(TextWriter output, IRecognizer recognizer, T offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e) {
            throw new SyntaxErrorException(msg);
        }
    }
}