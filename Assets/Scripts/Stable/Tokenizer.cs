using System.Collections.Generic;
using UnityEngine.Assertions;

public struct Tokenizer {

    public struct Token {

        public readonly string source;
        public readonly int start, length;
        private string text;

        public Token(string source, int start, int length) {
            this.source = source;
            this.start = start;
            this.length = length;
            text = null;
        }

        public override string ToString() {
            return text ??= source.Substring(start, length);
        }

        public char this[int index] {
            get {
                Assert.IsTrue(index >= 0 && index < length);
                return source[start + index];
            }
        }

        public static implicit operator string(Token token) => token.ToString();

        public (int line, int column) CalculateLocation() {
            var line = 0;
            var column = 0;
            for (var i = 0; i < start; i++)
                switch (source[i]) {
                    case '\n': {
                        line++;
                        column = 0;
                        break;
                    }
                    case '\r':
                        break;
                    default:
                        column++;
                        break;
                }
            return (line, column);
        }
    }

    public readonly string input;
    private int index;

    public Tokenizer(string input) {
        Assert.IsNotNull(input);
        this.input = input;
        index = 0;
    }

    public bool TryReadNextToken(out Token token) {

        while (index < input.Length && input[index] is ' ' or '\t' or '\r' or '\n')
            index++;

        if (index >= input.Length) {
            token = default;
            return false;
        }

        var start = index;
        var end = start;
        while (index < input.Length && input[index] is not (' ' or '\t' or '\r' or '\n')) {
            end = index;
            index++;
        }
        var length = end - start + 1;
        token = new Token(input, start, length);
        return true;
    }

    public static IEnumerable<Token> Tokenize(string input) {
        var tokenizer = new Tokenizer(input);
        while (tokenizer.TryReadNextToken(out var token))
            yield return token;
    }
}