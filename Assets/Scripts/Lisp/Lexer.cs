using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Lisp {
    
    public static class Lexer {

        private const string whitespaces = " \r\n\t";
        private const string symbolCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz+-*/!.";
        private const string digits = "0123456789";

        private static bool IsWhitespace(char ch) => whitespaces.Contains(ch);
        private static bool IsSymbolCharacter(char ch) => symbolCharacters.Contains(ch);
        private static bool IsDigit(char ch) => digits.Contains(ch);

        private const char singleLineComment = ';';
        private const char leftParen = '(';
        private const char rightParen = ')';
        private const char quote = '\'';
        private const char strQuote = '"';
        private const char escape = '\\';
        private const char keyword = ':';
        private const char floatSeparator = '.';

        private static Dictionary<char, char> escapeCharacters = new Dictionary<char, char> {
            { escape, escape },
            {'r', '\r'},
            { 'n', '\n' },
            { 't', '\t' },
            { '"', '"' },
        };

        public static IEnumerable<Token> Tokenize(string text) {
            for (var i = 0; i < text.Length;) {
                if (IsWhitespace(text[i]))
                    i = EatWhitespace(text, i);
                else if (text[i] == singleLineComment)
                    i = EatSingleLineComment(text, i);
                else if (IsDigit(text[i])) {
                    Token token;
                    i = EatNumber(text, i, out token);
                    yield return token;
                }
                else if (IsSymbolCharacter(text[i])) {
                    string name;
                    i = EatSymbol(text, i, out name);
                    yield return new SymbolToken { name = name };
                }
                else {
                    switch (text[i]) {
                        case leftParen:
                            i++;
                            yield return new LeftParenToken();
                            break;
                        case rightParen:
                            i++;
                            yield return new RightParenToken();
                            break;
                        case quote:
                            i++;
                            yield return new QuoteToken();
                            break;
                        case strQuote:
                            string val;
                            i = EatString(text, i + 1, out val);
                            yield return new StrToken { val = val };
                            break;
                        case keyword: {
                            string name;
                            i = EatSymbol(text, i + 1, out name);
                            yield return new KeywordToken { name = name };
                            break;
                        }
                        default: {
                            Debug.Log("bad character " + text[i]);
                            i++;
                            break;
                        }
                    }
                }
            }
        }
        
        private static int EatWhitespace(string text, int start) {
            var i = start;
            for (; i < text.Length && IsWhitespace(text[i]); i++) { }
            return i;
        }
        
        private static int EatSingleLineComment(string text, int start) {
            var i = start;
            for (; i < text.Length; i++) {
                if (text[i] == '\n') {
                    i++;
                    break;
                }
            }
            return i;
        }
        
        private static int EatNumber(string text, int start, out Token token) {
            string firstPart;
            var offset = EatNumericPart(text, start, out firstPart);
            if (offset < text.Length && text[offset] == floatSeparator) {
                string secondPart;
                offset = EatNumericPart(text, offset + 1, out secondPart);
                token = new FloatToken { val = float.Parse($"{firstPart}.{secondPart}") };
            }
            else {
                token = new IntToken { val = int.Parse(firstPart) };
            }
            return offset;
        }
        
        private static int EatNumericPart(string text, int start, out string str) {
            str = "";
            var i = start;
            for (; i < text.Length && IsDigit(text[i]); i++) {
                str += text[i];
            }
            return i;
        }
        
        private static int EatString(string text, int start, out string val) {
            val = "";
            var i = start;
            for (; i < text.Length; i++) {
                if (text[i] == strQuote) {
                    i++;
                    break;
                }
                if (text[i] == escape && i + 1 < text.Length) {
                    var escapeCharacter = text[++i];
                    Assert.IsTrue(escapeCharacters.ContainsKey(escapeCharacter));
                    val += escapeCharacters[escapeCharacter];
                }
                else {
                    val += text[i];
                }
            }
            return i;
        }
        
        private static int EatSymbol(string text, int start, out string name) {
            name = "";
            var i = start;
            for (; i < text.Length && IsSymbolCharacter(text[i]); i++) {
                name += text[i];
            }
            return i;
        }
    }
}