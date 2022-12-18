using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;
using HostFunction = System.Func<dynamic[], AiScriptInterpreter.Environment, dynamic>;
using Symbol = AiScriptVisitor.Symbol;

public class AiScriptInterpreter {

    private static HostFunction BinaryFunction(Func<dynamic, dynamic, dynamic> function) {
        return (arguments, _) => {
            Assert.AreEqual(2, arguments.Length);
            return function(arguments[0], arguments[1]);
        };
    }

    private readonly AiScriptLexer lexer = new(null);
    private readonly AiScriptParser parser = new(null);
    public readonly Environment environment = new() {
        ["+"] = BinaryFunction((a, b) => a + b),
        ["*"] = BinaryFunction((a, b) => a * b),
        ["/"] = BinaryFunction((a, b) => a / b),
        ["display"] = (HostFunction)((arguments, _) => {
            Debug.Log(string.Join(" ", arguments));
            return null;
        })
    };

    public AiScriptInterpreter() {
        lexer.RemoveErrorListeners();
        parser.RemoveErrorListeners();
        lexer.AddErrorListener(new ExceptionThrower<int>());
        parser.AddErrorListener(new ExceptionThrower<IToken>());
    }

    public dynamic Evaluate(string input) {
        lexer.SetInputStream(new AntlrInputStream("(do " + input + ")"));
        parser.TokenStream = new CommonTokenStream(lexer);
        return EvaluateExpression(AiScriptVisitor.instance.Visit(parser.expression()), environment);
    }

    public class Environment : Dictionary<string, dynamic> {
        public Environment parent;
        public Environment FindContaining(string variableName) {
            var environment = this;
            for (; environment != null && !environment.ContainsKey(variableName); environment = environment.parent) { }
            return environment;
        }
    }
    private class InterpretedFunction {
        public string[] parameters;
        public dynamic body;
        public Environment environment;
    }

    private dynamic EvaluateExpression(dynamic expression, Environment environment) {
        switch (expression) {

            case bool value:
                return value;

            case int value:
                return value;

            case Symbol variable:
                var containing = environment.FindContaining(variable.name);
                Assert.IsNotNull(containing, variable.name);
                return containing[variable.name];

            case string value:
                return value;

            case dynamic[] list:

                if (list.Length == 0)
                    return null;

                else {
                    var rest = list.Skip(1).ToArray();
                    if (list[0] is Symbol head)
                        switch (head.name) {

                            case "function":
                                Assert.IsTrue(rest.Length >= 2);
                                var parameters = rest[0] as dynamic[];
                                Assert.IsNotNull(parameters);
                                Assert.IsTrue(parameters.All(item => item is Symbol));
                                var parameterNames = parameters.Cast<Symbol>().Select(item => item.name).ToArray();
                                Assert.AreEqual(parameterNames.Length, parameterNames.Distinct().Count());
                                var body = new List<dynamic> { new Symbol { name = "do" } };
                                body.AddRange(rest.Skip(1));
                                return new InterpretedFunction {
                                    parameters = parameters.Select(item => ((Symbol)item).name).ToArray(),
                                    body = body.ToArray(),
                                    environment = environment
                                };

                            case "quote":
                                Assert.AreEqual(1, rest.Length);
                                return rest[0];

                            case "if":
                                Assert.IsTrue(rest.Length is 2 or 3);
                                return EvaluateExpression(rest[0], environment)
                                    ? EvaluateExpression(rest[1], environment)
                                    : (rest.Length == 3
                                        ? EvaluateExpression(rest[2], environment)
                                        : null);

                            case "set!":
                                Assert.AreEqual(2, rest.Length);
                                var symbol = rest[0] as Symbol;
                                Assert.IsNotNull(symbol);
                                var containingEnvironment = environment.FindContaining(symbol.name);
                                Assert.IsNotNull(containingEnvironment);
                                var value = EvaluateExpression(rest[1], environment);
                                containingEnvironment[symbol.name] = value;
                                return containingEnvironment[symbol.name] = value;
                        }

                    var function = EvaluateExpression(list[0], this.environment);
                    var arguments = rest.Select(item => EvaluateExpression(item, environment)).ToArray();

                    switch (function) {
                        case InterpretedFunction interpretedFunction: {
                            Assert.AreEqual(interpretedFunction.parameters.Length, arguments.Length);
                            var closure = new Environment { parent = interpretedFunction.environment };
                            for (var i = 0; i < interpretedFunction.parameters.Length; i++)
                                closure[interpretedFunction.parameters[i]] = arguments[i];
                            return EvaluateExpression(interpretedFunction.body, closure);
                        }

                        case HostFunction hostFunction:
                            return hostFunction(arguments, environment);

                        default:
                            throw new Exception(function.ToString());
                    }
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