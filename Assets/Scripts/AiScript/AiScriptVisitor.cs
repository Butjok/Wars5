using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Assertions;

public class AiScriptVisitor : AiScriptBaseVisitor<dynamic> {

    public static readonly AiScriptVisitor instance = new();
    
    public class Symbol {
        public string name;
        public override string ToString() => name;
    }

    public static readonly Dictionary<char, char> unescape = new() {
        ['\"'] = '\"',
        ['\\'] = '\\',
        ['b'] = '\b',
        ['n'] = '\n',
        ['f'] = '\f',
        ['r'] = '\r',
        ['t'] = '\t',
    };

    public override dynamic VisitBoolean(AiScriptParser.BooleanContext context) {
        return context.GetText() == "#t";
    }

    public override dynamic VisitInteger(AiScriptParser.IntegerContext context) {
        return int.Parse(context.GetText());
    }

    public override dynamic VisitSymbol(AiScriptParser.SymbolContext context) {
        return new Symbol { name = context.GetText() };
    }

    public override dynamic VisitString(AiScriptParser.StringContext context) {
        var sb = new StringBuilder();
        var text = context.GetText();
        for (var i = 1; i < text.Length - 1; i++)
            sb.Append(text[i] != '\\'
                ? text[i].ToString()
                : unescape.TryGetValue(text[++i], out var c)
                    ? c.ToString()
                    : "\\" + text[i]);
        return sb.ToString();
    }

    public override dynamic VisitQuote(AiScriptParser.QuoteContext context) {
        return new[] { new Symbol { name = "quote" }, Visit(context.expression())};
    }

    public override dynamic VisitList(AiScriptParser.ListContext context) {
        var expressions = context.expression().Select(Visit).ToArray();
        if (expressions.Length >= 3 && expressions[0] is Symbol { name: "let" }) {
            var pairs = expressions[1] as dynamic[];
            Assert.IsNotNull(pairs);
            var body = expressions.Skip(2).ToArray();
            var function = new List<dynamic> {  };
            var result = new List<dynamic> { };
            return null;
        }
        else
            return expressions;
    }
}