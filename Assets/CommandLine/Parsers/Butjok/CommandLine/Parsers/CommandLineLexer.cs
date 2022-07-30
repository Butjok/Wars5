//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.9.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from /Users/butjok/CommandLine/Assets/CommandLine/CommandLine.g4 by ANTLR 4.9.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace Butjok.CommandLine.Parsers {
using System;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.9.1")]
[System.CLSCompliant(false)]
public partial class CommandLineLexer : Lexer {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		Asterisk=1, DoubleAmpersand=2, DoubleVerticalBar=3, Exclamation=4, False=5, 
		Float2=6, Float3=7, ForwardSlash=8, Int2=9, Int3=10, LeftParenthesis=11, 
		LeftSquareBracket=12, Minus=13, Null=14, Percent=15, Plus=16, Rgb=17, 
		RightParenthesis=18, RightSquareBracket=19, True=20, Tilde=21, Identifier=22, 
		Integer=23, Real=24, String=25, Whitespace=26;
	public static string[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static string[] modeNames = {
		"DEFAULT_MODE"
	};

	public static readonly string[] ruleNames = {
		"Asterisk", "DoubleAmpersand", "DoubleVerticalBar", "Exclamation", "False", 
		"Float2", "Float3", "ForwardSlash", "Int2", "Int3", "LeftParenthesis", 
		"LeftSquareBracket", "Minus", "Null", "Percent", "Plus", "Rgb", "RightParenthesis", 
		"RightSquareBracket", "True", "Tilde", "Identifier", "Integer", "Real", 
		"String", "Whitespace", "INT"
	};


	public CommandLineLexer(ICharStream input)
	: this(input, Console.Out, Console.Error) { }

	public CommandLineLexer(ICharStream input, TextWriter output, TextWriter errorOutput)
	: base(input, output, errorOutput)
	{
		Interpreter = new LexerATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	private static readonly string[] _LiteralNames = {
		null, "'*'", "'&&'", "'||'", "'!'", "'false'", "'float2'", "'float3'", 
		"'/'", "'int2'", "'int3'", "'('", "'['", "'-'", "'null'", "'%'", "'+'", 
		"'rgb'", "')'", "']'", "'true'", "'~'"
	};
	private static readonly string[] _SymbolicNames = {
		null, "Asterisk", "DoubleAmpersand", "DoubleVerticalBar", "Exclamation", 
		"False", "Float2", "Float3", "ForwardSlash", "Int2", "Int3", "LeftParenthesis", 
		"LeftSquareBracket", "Minus", "Null", "Percent", "Plus", "Rgb", "RightParenthesis", 
		"RightSquareBracket", "True", "Tilde", "Identifier", "Integer", "Real", 
		"String", "Whitespace"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "CommandLine.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ChannelNames { get { return channelNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override string SerializedAtn { get { return new string(_serializedATN); } }

	static CommandLineLexer() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}
	private static char[] _serializedATN = {
		'\x3', '\x608B', '\xA72A', '\x8133', '\xB9ED', '\x417C', '\x3BE7', '\x7786', 
		'\x5964', '\x2', '\x1C', '\xBF', '\b', '\x1', '\x4', '\x2', '\t', '\x2', 
		'\x4', '\x3', '\t', '\x3', '\x4', '\x4', '\t', '\x4', '\x4', '\x5', '\t', 
		'\x5', '\x4', '\x6', '\t', '\x6', '\x4', '\a', '\t', '\a', '\x4', '\b', 
		'\t', '\b', '\x4', '\t', '\t', '\t', '\x4', '\n', '\t', '\n', '\x4', '\v', 
		'\t', '\v', '\x4', '\f', '\t', '\f', '\x4', '\r', '\t', '\r', '\x4', '\xE', 
		'\t', '\xE', '\x4', '\xF', '\t', '\xF', '\x4', '\x10', '\t', '\x10', '\x4', 
		'\x11', '\t', '\x11', '\x4', '\x12', '\t', '\x12', '\x4', '\x13', '\t', 
		'\x13', '\x4', '\x14', '\t', '\x14', '\x4', '\x15', '\t', '\x15', '\x4', 
		'\x16', '\t', '\x16', '\x4', '\x17', '\t', '\x17', '\x4', '\x18', '\t', 
		'\x18', '\x4', '\x19', '\t', '\x19', '\x4', '\x1A', '\t', '\x1A', '\x4', 
		'\x1B', '\t', '\x1B', '\x4', '\x1C', '\t', '\x1C', '\x3', '\x2', '\x3', 
		'\x2', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x4', '\x3', 
		'\x4', '\x3', '\x4', '\x3', '\x5', '\x3', '\x5', '\x3', '\x6', '\x3', 
		'\x6', '\x3', '\x6', '\x3', '\x6', '\x3', '\x6', '\x3', '\x6', '\x3', 
		'\a', '\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', '\a', 
		'\x3', '\a', '\x3', '\b', '\x3', '\b', '\x3', '\b', '\x3', '\b', '\x3', 
		'\b', '\x3', '\b', '\x3', '\b', '\x3', '\t', '\x3', '\t', '\x3', '\n', 
		'\x3', '\n', '\x3', '\n', '\x3', '\n', '\x3', '\n', '\x3', '\v', '\x3', 
		'\v', '\x3', '\v', '\x3', '\v', '\x3', '\v', '\x3', '\f', '\x3', '\f', 
		'\x3', '\r', '\x3', '\r', '\x3', '\xE', '\x3', '\xE', '\x3', '\xF', '\x3', 
		'\xF', '\x3', '\xF', '\x3', '\xF', '\x3', '\xF', '\x3', '\x10', '\x3', 
		'\x10', '\x3', '\x11', '\x3', '\x11', '\x3', '\x12', '\x3', '\x12', '\x3', 
		'\x12', '\x3', '\x12', '\x3', '\x13', '\x3', '\x13', '\x3', '\x14', '\x3', 
		'\x14', '\x3', '\x15', '\x3', '\x15', '\x3', '\x15', '\x3', '\x15', '\x3', 
		'\x15', '\x3', '\x16', '\x3', '\x16', '\x3', '\x17', '\x3', '\x17', '\a', 
		'\x17', '\x84', '\n', '\x17', '\f', '\x17', '\xE', '\x17', '\x87', '\v', 
		'\x17', '\x3', '\x17', '\x3', '\x17', '\x3', '\x17', '\a', '\x17', '\x8C', 
		'\n', '\x17', '\f', '\x17', '\xE', '\x17', '\x8F', '\v', '\x17', '\a', 
		'\x17', '\x91', '\n', '\x17', '\f', '\x17', '\xE', '\x17', '\x94', '\v', 
		'\x17', '\x3', '\x18', '\x5', '\x18', '\x97', '\n', '\x18', '\x3', '\x18', 
		'\x3', '\x18', '\x3', '\x19', '\x5', '\x19', '\x9C', '\n', '\x19', '\x3', 
		'\x19', '\x3', '\x19', '\x3', '\x19', '\x3', '\x19', '\x3', '\x19', '\x3', 
		'\x19', '\x3', '\x19', '\x3', '\x19', '\x3', '\x19', '\x5', '\x19', '\xA7', 
		'\n', '\x19', '\x3', '\x1A', '\x3', '\x1A', '\x3', '\x1A', '\x3', '\x1A', 
		'\a', '\x1A', '\xAD', '\n', '\x1A', '\f', '\x1A', '\xE', '\x1A', '\xB0', 
		'\v', '\x1A', '\x3', '\x1A', '\x3', '\x1A', '\x3', '\x1B', '\x6', '\x1B', 
		'\xB5', '\n', '\x1B', '\r', '\x1B', '\xE', '\x1B', '\xB6', '\x3', '\x1B', 
		'\x3', '\x1B', '\x3', '\x1C', '\x6', '\x1C', '\xBC', '\n', '\x1C', '\r', 
		'\x1C', '\xE', '\x1C', '\xBD', '\x2', '\x2', '\x1D', '\x3', '\x3', '\x5', 
		'\x4', '\a', '\x5', '\t', '\x6', '\v', '\a', '\r', '\b', '\xF', '\t', 
		'\x11', '\n', '\x13', '\v', '\x15', '\f', '\x17', '\r', '\x19', '\xE', 
		'\x1B', '\xF', '\x1D', '\x10', '\x1F', '\x11', '!', '\x12', '#', '\x13', 
		'%', '\x14', '\'', '\x15', ')', '\x16', '+', '\x17', '-', '\x18', '/', 
		'\x19', '\x31', '\x1A', '\x33', '\x1B', '\x35', '\x1C', '\x37', '\x2', 
		'\x3', '\x2', '\b', '\x5', '\x2', '\x43', '\\', '\x61', '\x61', '\x63', 
		'|', '\x6', '\x2', '\x32', ';', '\x43', '\\', '\x61', '\x61', '\x63', 
		'|', '\t', '\x2', '$', '$', '^', '^', '\x64', '\x64', 'h', 'h', 'p', 'p', 
		't', 't', 'v', 'v', '\x5', '\x2', '\x2', '!', '$', '$', '^', '^', '\x5', 
		'\x2', '\v', '\f', '\xF', '\xF', '\"', '\"', '\x3', '\x2', '\x32', ';', 
		'\x2', '\xC8', '\x2', '\x3', '\x3', '\x2', '\x2', '\x2', '\x2', '\x5', 
		'\x3', '\x2', '\x2', '\x2', '\x2', '\a', '\x3', '\x2', '\x2', '\x2', '\x2', 
		'\t', '\x3', '\x2', '\x2', '\x2', '\x2', '\v', '\x3', '\x2', '\x2', '\x2', 
		'\x2', '\r', '\x3', '\x2', '\x2', '\x2', '\x2', '\xF', '\x3', '\x2', '\x2', 
		'\x2', '\x2', '\x11', '\x3', '\x2', '\x2', '\x2', '\x2', '\x13', '\x3', 
		'\x2', '\x2', '\x2', '\x2', '\x15', '\x3', '\x2', '\x2', '\x2', '\x2', 
		'\x17', '\x3', '\x2', '\x2', '\x2', '\x2', '\x19', '\x3', '\x2', '\x2', 
		'\x2', '\x2', '\x1B', '\x3', '\x2', '\x2', '\x2', '\x2', '\x1D', '\x3', 
		'\x2', '\x2', '\x2', '\x2', '\x1F', '\x3', '\x2', '\x2', '\x2', '\x2', 
		'!', '\x3', '\x2', '\x2', '\x2', '\x2', '#', '\x3', '\x2', '\x2', '\x2', 
		'\x2', '%', '\x3', '\x2', '\x2', '\x2', '\x2', '\'', '\x3', '\x2', '\x2', 
		'\x2', '\x2', ')', '\x3', '\x2', '\x2', '\x2', '\x2', '+', '\x3', '\x2', 
		'\x2', '\x2', '\x2', '-', '\x3', '\x2', '\x2', '\x2', '\x2', '/', '\x3', 
		'\x2', '\x2', '\x2', '\x2', '\x31', '\x3', '\x2', '\x2', '\x2', '\x2', 
		'\x33', '\x3', '\x2', '\x2', '\x2', '\x2', '\x35', '\x3', '\x2', '\x2', 
		'\x2', '\x3', '\x39', '\x3', '\x2', '\x2', '\x2', '\x5', ';', '\x3', '\x2', 
		'\x2', '\x2', '\a', '>', '\x3', '\x2', '\x2', '\x2', '\t', '\x41', '\x3', 
		'\x2', '\x2', '\x2', '\v', '\x43', '\x3', '\x2', '\x2', '\x2', '\r', 'I', 
		'\x3', '\x2', '\x2', '\x2', '\xF', 'P', '\x3', '\x2', '\x2', '\x2', '\x11', 
		'W', '\x3', '\x2', '\x2', '\x2', '\x13', 'Y', '\x3', '\x2', '\x2', '\x2', 
		'\x15', '^', '\x3', '\x2', '\x2', '\x2', '\x17', '\x63', '\x3', '\x2', 
		'\x2', '\x2', '\x19', '\x65', '\x3', '\x2', '\x2', '\x2', '\x1B', 'g', 
		'\x3', '\x2', '\x2', '\x2', '\x1D', 'i', '\x3', '\x2', '\x2', '\x2', '\x1F', 
		'n', '\x3', '\x2', '\x2', '\x2', '!', 'p', '\x3', '\x2', '\x2', '\x2', 
		'#', 'r', '\x3', '\x2', '\x2', '\x2', '%', 'v', '\x3', '\x2', '\x2', '\x2', 
		'\'', 'x', '\x3', '\x2', '\x2', '\x2', ')', 'z', '\x3', '\x2', '\x2', 
		'\x2', '+', '\x7F', '\x3', '\x2', '\x2', '\x2', '-', '\x81', '\x3', '\x2', 
		'\x2', '\x2', '/', '\x96', '\x3', '\x2', '\x2', '\x2', '\x31', '\x9B', 
		'\x3', '\x2', '\x2', '\x2', '\x33', '\xA8', '\x3', '\x2', '\x2', '\x2', 
		'\x35', '\xB4', '\x3', '\x2', '\x2', '\x2', '\x37', '\xBB', '\x3', '\x2', 
		'\x2', '\x2', '\x39', ':', '\a', ',', '\x2', '\x2', ':', '\x4', '\x3', 
		'\x2', '\x2', '\x2', ';', '<', '\a', '(', '\x2', '\x2', '<', '=', '\a', 
		'(', '\x2', '\x2', '=', '\x6', '\x3', '\x2', '\x2', '\x2', '>', '?', '\a', 
		'~', '\x2', '\x2', '?', '@', '\a', '~', '\x2', '\x2', '@', '\b', '\x3', 
		'\x2', '\x2', '\x2', '\x41', '\x42', '\a', '#', '\x2', '\x2', '\x42', 
		'\n', '\x3', '\x2', '\x2', '\x2', '\x43', '\x44', '\a', 'h', '\x2', '\x2', 
		'\x44', '\x45', '\a', '\x63', '\x2', '\x2', '\x45', '\x46', '\a', 'n', 
		'\x2', '\x2', '\x46', 'G', '\a', 'u', '\x2', '\x2', 'G', 'H', '\a', 'g', 
		'\x2', '\x2', 'H', '\f', '\x3', '\x2', '\x2', '\x2', 'I', 'J', '\a', 'h', 
		'\x2', '\x2', 'J', 'K', '\a', 'n', '\x2', '\x2', 'K', 'L', '\a', 'q', 
		'\x2', '\x2', 'L', 'M', '\a', '\x63', '\x2', '\x2', 'M', 'N', '\a', 'v', 
		'\x2', '\x2', 'N', 'O', '\a', '\x34', '\x2', '\x2', 'O', '\xE', '\x3', 
		'\x2', '\x2', '\x2', 'P', 'Q', '\a', 'h', '\x2', '\x2', 'Q', 'R', '\a', 
		'n', '\x2', '\x2', 'R', 'S', '\a', 'q', '\x2', '\x2', 'S', 'T', '\a', 
		'\x63', '\x2', '\x2', 'T', 'U', '\a', 'v', '\x2', '\x2', 'U', 'V', '\a', 
		'\x35', '\x2', '\x2', 'V', '\x10', '\x3', '\x2', '\x2', '\x2', 'W', 'X', 
		'\a', '\x31', '\x2', '\x2', 'X', '\x12', '\x3', '\x2', '\x2', '\x2', 'Y', 
		'Z', '\a', 'k', '\x2', '\x2', 'Z', '[', '\a', 'p', '\x2', '\x2', '[', 
		'\\', '\a', 'v', '\x2', '\x2', '\\', ']', '\a', '\x34', '\x2', '\x2', 
		']', '\x14', '\x3', '\x2', '\x2', '\x2', '^', '_', '\a', 'k', '\x2', '\x2', 
		'_', '`', '\a', 'p', '\x2', '\x2', '`', '\x61', '\a', 'v', '\x2', '\x2', 
		'\x61', '\x62', '\a', '\x35', '\x2', '\x2', '\x62', '\x16', '\x3', '\x2', 
		'\x2', '\x2', '\x63', '\x64', '\a', '*', '\x2', '\x2', '\x64', '\x18', 
		'\x3', '\x2', '\x2', '\x2', '\x65', '\x66', '\a', ']', '\x2', '\x2', '\x66', 
		'\x1A', '\x3', '\x2', '\x2', '\x2', 'g', 'h', '\a', '/', '\x2', '\x2', 
		'h', '\x1C', '\x3', '\x2', '\x2', '\x2', 'i', 'j', '\a', 'p', '\x2', '\x2', 
		'j', 'k', '\a', 'w', '\x2', '\x2', 'k', 'l', '\a', 'n', '\x2', '\x2', 
		'l', 'm', '\a', 'n', '\x2', '\x2', 'm', '\x1E', '\x3', '\x2', '\x2', '\x2', 
		'n', 'o', '\a', '\'', '\x2', '\x2', 'o', ' ', '\x3', '\x2', '\x2', '\x2', 
		'p', 'q', '\a', '-', '\x2', '\x2', 'q', '\"', '\x3', '\x2', '\x2', '\x2', 
		'r', 's', '\a', 't', '\x2', '\x2', 's', 't', '\a', 'i', '\x2', '\x2', 
		't', 'u', '\a', '\x64', '\x2', '\x2', 'u', '$', '\x3', '\x2', '\x2', '\x2', 
		'v', 'w', '\a', '+', '\x2', '\x2', 'w', '&', '\x3', '\x2', '\x2', '\x2', 
		'x', 'y', '\a', '_', '\x2', '\x2', 'y', '(', '\x3', '\x2', '\x2', '\x2', 
		'z', '{', '\a', 'v', '\x2', '\x2', '{', '|', '\a', 't', '\x2', '\x2', 
		'|', '}', '\a', 'w', '\x2', '\x2', '}', '~', '\a', 'g', '\x2', '\x2', 
		'~', '*', '\x3', '\x2', '\x2', '\x2', '\x7F', '\x80', '\a', '\x80', '\x2', 
		'\x2', '\x80', ',', '\x3', '\x2', '\x2', '\x2', '\x81', '\x85', '\t', 
		'\x2', '\x2', '\x2', '\x82', '\x84', '\t', '\x3', '\x2', '\x2', '\x83', 
		'\x82', '\x3', '\x2', '\x2', '\x2', '\x84', '\x87', '\x3', '\x2', '\x2', 
		'\x2', '\x85', '\x83', '\x3', '\x2', '\x2', '\x2', '\x85', '\x86', '\x3', 
		'\x2', '\x2', '\x2', '\x86', '\x92', '\x3', '\x2', '\x2', '\x2', '\x87', 
		'\x85', '\x3', '\x2', '\x2', '\x2', '\x88', '\x89', '\a', '\x30', '\x2', 
		'\x2', '\x89', '\x8D', '\t', '\x2', '\x2', '\x2', '\x8A', '\x8C', '\t', 
		'\x3', '\x2', '\x2', '\x8B', '\x8A', '\x3', '\x2', '\x2', '\x2', '\x8C', 
		'\x8F', '\x3', '\x2', '\x2', '\x2', '\x8D', '\x8B', '\x3', '\x2', '\x2', 
		'\x2', '\x8D', '\x8E', '\x3', '\x2', '\x2', '\x2', '\x8E', '\x91', '\x3', 
		'\x2', '\x2', '\x2', '\x8F', '\x8D', '\x3', '\x2', '\x2', '\x2', '\x90', 
		'\x88', '\x3', '\x2', '\x2', '\x2', '\x91', '\x94', '\x3', '\x2', '\x2', 
		'\x2', '\x92', '\x90', '\x3', '\x2', '\x2', '\x2', '\x92', '\x93', '\x3', 
		'\x2', '\x2', '\x2', '\x93', '.', '\x3', '\x2', '\x2', '\x2', '\x94', 
		'\x92', '\x3', '\x2', '\x2', '\x2', '\x95', '\x97', '\a', '/', '\x2', 
		'\x2', '\x96', '\x95', '\x3', '\x2', '\x2', '\x2', '\x96', '\x97', '\x3', 
		'\x2', '\x2', '\x2', '\x97', '\x98', '\x3', '\x2', '\x2', '\x2', '\x98', 
		'\x99', '\x5', '\x37', '\x1C', '\x2', '\x99', '\x30', '\x3', '\x2', '\x2', 
		'\x2', '\x9A', '\x9C', '\a', '/', '\x2', '\x2', '\x9B', '\x9A', '\x3', 
		'\x2', '\x2', '\x2', '\x9B', '\x9C', '\x3', '\x2', '\x2', '\x2', '\x9C', 
		'\xA6', '\x3', '\x2', '\x2', '\x2', '\x9D', '\x9E', '\x5', '\x37', '\x1C', 
		'\x2', '\x9E', '\x9F', '\a', '\x30', '\x2', '\x2', '\x9F', '\xA0', '\x5', 
		'\x37', '\x1C', '\x2', '\xA0', '\xA7', '\x3', '\x2', '\x2', '\x2', '\xA1', 
		'\xA2', '\a', '\x30', '\x2', '\x2', '\xA2', '\xA7', '\x5', '\x37', '\x1C', 
		'\x2', '\xA3', '\xA4', '\x5', '\x37', '\x1C', '\x2', '\xA4', '\xA5', '\a', 
		'\x30', '\x2', '\x2', '\xA5', '\xA7', '\x3', '\x2', '\x2', '\x2', '\xA6', 
		'\x9D', '\x3', '\x2', '\x2', '\x2', '\xA6', '\xA1', '\x3', '\x2', '\x2', 
		'\x2', '\xA6', '\xA3', '\x3', '\x2', '\x2', '\x2', '\xA7', '\x32', '\x3', 
		'\x2', '\x2', '\x2', '\xA8', '\xAE', '\a', '$', '\x2', '\x2', '\xA9', 
		'\xAA', '\a', '^', '\x2', '\x2', '\xAA', '\xAD', '\t', '\x4', '\x2', '\x2', 
		'\xAB', '\xAD', '\n', '\x5', '\x2', '\x2', '\xAC', '\xA9', '\x3', '\x2', 
		'\x2', '\x2', '\xAC', '\xAB', '\x3', '\x2', '\x2', '\x2', '\xAD', '\xB0', 
		'\x3', '\x2', '\x2', '\x2', '\xAE', '\xAC', '\x3', '\x2', '\x2', '\x2', 
		'\xAE', '\xAF', '\x3', '\x2', '\x2', '\x2', '\xAF', '\xB1', '\x3', '\x2', 
		'\x2', '\x2', '\xB0', '\xAE', '\x3', '\x2', '\x2', '\x2', '\xB1', '\xB2', 
		'\a', '$', '\x2', '\x2', '\xB2', '\x34', '\x3', '\x2', '\x2', '\x2', '\xB3', 
		'\xB5', '\t', '\x6', '\x2', '\x2', '\xB4', '\xB3', '\x3', '\x2', '\x2', 
		'\x2', '\xB5', '\xB6', '\x3', '\x2', '\x2', '\x2', '\xB6', '\xB4', '\x3', 
		'\x2', '\x2', '\x2', '\xB6', '\xB7', '\x3', '\x2', '\x2', '\x2', '\xB7', 
		'\xB8', '\x3', '\x2', '\x2', '\x2', '\xB8', '\xB9', '\b', '\x1B', '\x2', 
		'\x2', '\xB9', '\x36', '\x3', '\x2', '\x2', '\x2', '\xBA', '\xBC', '\t', 
		'\a', '\x2', '\x2', '\xBB', '\xBA', '\x3', '\x2', '\x2', '\x2', '\xBC', 
		'\xBD', '\x3', '\x2', '\x2', '\x2', '\xBD', '\xBB', '\x3', '\x2', '\x2', 
		'\x2', '\xBD', '\xBE', '\x3', '\x2', '\x2', '\x2', '\xBE', '\x38', '\x3', 
		'\x2', '\x2', '\x2', '\r', '\x2', '\x85', '\x8D', '\x92', '\x96', '\x9B', 
		'\xA6', '\xAC', '\xAE', '\xB6', '\xBD', '\x3', '\x2', '\x3', '\x2',
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
} // namespace Butjok.CommandLine.Parsers
