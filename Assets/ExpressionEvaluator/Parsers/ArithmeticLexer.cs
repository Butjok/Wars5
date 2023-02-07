//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.11.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from /Users/butjok/Documents/GitHub/Wars5/Assets/ExpressionEvaluator/Arithmetic.g4 by ANTLR 4.11.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using System;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.11.1")]
[System.CLSCompliant(false)]
public partial class ArithmeticLexer : Lexer {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		T__0=1, T__1=2, T__2=3, T__3=4, T__4=5, T__5=6, T__6=7, VARIABLE=8, SCIENTIFIC_NUMBER=9, 
		Whitespace=10;
	public static string[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static string[] modeNames = {
		"DEFAULT_MODE"
	};

	public static readonly string[] ruleNames = {
		"T__0", "T__1", "T__2", "T__3", "T__4", "T__5", "T__6", "VARIABLE", "VALID_ID_START", 
		"VALID_ID_CHAR", "SCIENTIFIC_NUMBER", "NUMBER", "UNSIGNED_INTEGER", "E", 
		"SIGN", "Whitespace"
	};


	public ArithmeticLexer(ICharStream input)
	: this(input, Console.Out, Console.Error) { }

	public ArithmeticLexer(ICharStream input, TextWriter output, TextWriter errorOutput)
	: base(input, output, errorOutput)
	{
		Interpreter = new LexerATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	private static readonly string[] _LiteralNames = {
		null, "'^'", "'*'", "'/'", "'+'", "'-'", "'('", "')'"
	};
	private static readonly string[] _SymbolicNames = {
		null, null, null, null, null, null, null, null, "VARIABLE", "SCIENTIFIC_NUMBER", 
		"Whitespace"
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

	public override string GrammarFileName { get { return "Arithmetic.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ChannelNames { get { return channelNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override int[] SerializedAtn { get { return _serializedATN; } }

	static ArithmeticLexer() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}
	private static int[] _serializedATN = {
		4,0,10,99,6,-1,2,0,7,0,2,1,7,1,2,2,7,2,2,3,7,3,2,4,7,4,2,5,7,5,2,6,7,6,
		2,7,7,7,2,8,7,8,2,9,7,9,2,10,7,10,2,11,7,11,2,12,7,12,2,13,7,13,2,14,7,
		14,2,15,7,15,1,0,1,0,1,1,1,1,1,2,1,2,1,3,1,3,1,4,1,4,1,5,1,5,1,6,1,6,1,
		7,1,7,5,7,50,8,7,10,7,12,7,53,9,7,1,8,3,8,56,8,8,1,9,1,9,3,9,60,8,9,1,
		10,1,10,1,10,3,10,65,8,10,1,10,1,10,3,10,69,8,10,1,11,4,11,72,8,11,11,
		11,12,11,73,1,11,1,11,4,11,78,8,11,11,11,12,11,79,3,11,82,8,11,1,12,4,
		12,85,8,12,11,12,12,12,86,1,13,1,13,1,14,1,14,1,15,4,15,94,8,15,11,15,
		12,15,95,1,15,1,15,0,0,16,1,1,3,2,5,3,7,4,9,5,11,6,13,7,15,8,17,0,19,0,
		21,9,23,0,25,0,27,0,29,0,31,10,1,0,4,3,0,65,90,95,95,97,122,2,0,69,69,
		101,101,2,0,43,43,45,45,3,0,9,10,13,13,32,32,101,0,1,1,0,0,0,0,3,1,0,0,
		0,0,5,1,0,0,0,0,7,1,0,0,0,0,9,1,0,0,0,0,11,1,0,0,0,0,13,1,0,0,0,0,15,1,
		0,0,0,0,21,1,0,0,0,0,31,1,0,0,0,1,33,1,0,0,0,3,35,1,0,0,0,5,37,1,0,0,0,
		7,39,1,0,0,0,9,41,1,0,0,0,11,43,1,0,0,0,13,45,1,0,0,0,15,47,1,0,0,0,17,
		55,1,0,0,0,19,59,1,0,0,0,21,61,1,0,0,0,23,71,1,0,0,0,25,84,1,0,0,0,27,
		88,1,0,0,0,29,90,1,0,0,0,31,93,1,0,0,0,33,34,5,94,0,0,34,2,1,0,0,0,35,
		36,5,42,0,0,36,4,1,0,0,0,37,38,5,47,0,0,38,6,1,0,0,0,39,40,5,43,0,0,40,
		8,1,0,0,0,41,42,5,45,0,0,42,10,1,0,0,0,43,44,5,40,0,0,44,12,1,0,0,0,45,
		46,5,41,0,0,46,14,1,0,0,0,47,51,3,17,8,0,48,50,3,19,9,0,49,48,1,0,0,0,
		50,53,1,0,0,0,51,49,1,0,0,0,51,52,1,0,0,0,52,16,1,0,0,0,53,51,1,0,0,0,
		54,56,7,0,0,0,55,54,1,0,0,0,56,18,1,0,0,0,57,60,3,17,8,0,58,60,2,48,57,
		0,59,57,1,0,0,0,59,58,1,0,0,0,60,20,1,0,0,0,61,68,3,23,11,0,62,64,3,27,
		13,0,63,65,3,29,14,0,64,63,1,0,0,0,64,65,1,0,0,0,65,66,1,0,0,0,66,67,3,
		25,12,0,67,69,1,0,0,0,68,62,1,0,0,0,68,69,1,0,0,0,69,22,1,0,0,0,70,72,
		2,48,57,0,71,70,1,0,0,0,72,73,1,0,0,0,73,71,1,0,0,0,73,74,1,0,0,0,74,81,
		1,0,0,0,75,77,5,46,0,0,76,78,2,48,57,0,77,76,1,0,0,0,78,79,1,0,0,0,79,
		77,1,0,0,0,79,80,1,0,0,0,80,82,1,0,0,0,81,75,1,0,0,0,81,82,1,0,0,0,82,
		24,1,0,0,0,83,85,2,48,57,0,84,83,1,0,0,0,85,86,1,0,0,0,86,84,1,0,0,0,86,
		87,1,0,0,0,87,26,1,0,0,0,88,89,7,1,0,0,89,28,1,0,0,0,90,91,7,2,0,0,91,
		30,1,0,0,0,92,94,7,3,0,0,93,92,1,0,0,0,94,95,1,0,0,0,95,93,1,0,0,0,95,
		96,1,0,0,0,96,97,1,0,0,0,97,98,6,15,0,0,98,32,1,0,0,0,11,0,51,55,59,64,
		68,73,79,81,86,95,1,6,0,0
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}