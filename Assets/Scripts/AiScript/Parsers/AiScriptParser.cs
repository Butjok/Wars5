//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.9.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from /Users/butjok/Documents/GitHub/Wars5/Assets/Scripts/AiScript/AiScript.g4 by ANTLR 4.9.1

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
using System.Diagnostics;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.9.1")]
[System.CLSCompliant(false)]
public partial class AiScriptParser : Parser {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		T__0=1, T__1=2, T__2=3, Whitespace=4, Comment=5, True=6, False=7, Integer=8, 
		Symbol=9, String=10;
	public const int
		RULE_program = 0, RULE_expression = 1;
	public static readonly string[] ruleNames = {
		"program", "expression"
	};

	private static readonly string[] _LiteralNames = {
		null, "'('", "')'", "'''", null, null, "'#t'", "'#f'"
	};
	private static readonly string[] _SymbolicNames = {
		null, null, null, null, "Whitespace", "Comment", "True", "False", "Integer", 
		"Symbol", "String"
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

	public override string GrammarFileName { get { return "AiScript.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string SerializedAtn { get { return new string(_serializedATN); } }

	static AiScriptParser() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}

		public AiScriptParser(ITokenStream input) : this(input, Console.Out, Console.Error) { }

		public AiScriptParser(ITokenStream input, TextWriter output, TextWriter errorOutput)
		: base(input, output, errorOutput)
	{
		Interpreter = new ParserATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	public partial class ProgramContext : ParserRuleContext {
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode Eof() { return GetToken(AiScriptParser.Eof, 0); }
		[System.Diagnostics.DebuggerNonUserCode] public ExpressionContext[] expression() {
			return GetRuleContexts<ExpressionContext>();
		}
		[System.Diagnostics.DebuggerNonUserCode] public ExpressionContext expression(int i) {
			return GetRuleContext<ExpressionContext>(i);
		}
		public ProgramContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_program; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			IAiScriptListener typedListener = listener as IAiScriptListener;
			if (typedListener != null) typedListener.EnterProgram(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			IAiScriptListener typedListener = listener as IAiScriptListener;
			if (typedListener != null) typedListener.ExitProgram(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IAiScriptVisitor<TResult> typedVisitor = visitor as IAiScriptVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitProgram(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public ProgramContext program() {
		ProgramContext _localctx = new ProgramContext(Context, State);
		EnterRule(_localctx, 0, RULE_program);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 7;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << T__0) | (1L << T__2) | (1L << True) | (1L << False) | (1L << Integer) | (1L << Symbol) | (1L << String))) != 0)) {
				{
				{
				State = 4;
				expression();
				}
				}
				State = 9;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
			}
			State = 10;
			Match(Eof);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class ExpressionContext : ParserRuleContext {
		public ExpressionContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_expression; } }
	 
		public ExpressionContext() { }
		public virtual void CopyFrom(ExpressionContext context) {
			base.CopyFrom(context);
		}
	}
	public partial class SymbolContext : ExpressionContext {
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode Symbol() { return GetToken(AiScriptParser.Symbol, 0); }
		public SymbolContext(ExpressionContext context) { CopyFrom(context); }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			IAiScriptListener typedListener = listener as IAiScriptListener;
			if (typedListener != null) typedListener.EnterSymbol(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			IAiScriptListener typedListener = listener as IAiScriptListener;
			if (typedListener != null) typedListener.ExitSymbol(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IAiScriptVisitor<TResult> typedVisitor = visitor as IAiScriptVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitSymbol(this);
			else return visitor.VisitChildren(this);
		}
	}
	public partial class BooleanContext : ExpressionContext {
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode True() { return GetToken(AiScriptParser.True, 0); }
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode False() { return GetToken(AiScriptParser.False, 0); }
		public BooleanContext(ExpressionContext context) { CopyFrom(context); }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			IAiScriptListener typedListener = listener as IAiScriptListener;
			if (typedListener != null) typedListener.EnterBoolean(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			IAiScriptListener typedListener = listener as IAiScriptListener;
			if (typedListener != null) typedListener.ExitBoolean(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IAiScriptVisitor<TResult> typedVisitor = visitor as IAiScriptVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitBoolean(this);
			else return visitor.VisitChildren(this);
		}
	}
	public partial class QuoteContext : ExpressionContext {
		[System.Diagnostics.DebuggerNonUserCode] public ExpressionContext expression() {
			return GetRuleContext<ExpressionContext>(0);
		}
		public QuoteContext(ExpressionContext context) { CopyFrom(context); }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			IAiScriptListener typedListener = listener as IAiScriptListener;
			if (typedListener != null) typedListener.EnterQuote(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			IAiScriptListener typedListener = listener as IAiScriptListener;
			if (typedListener != null) typedListener.ExitQuote(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IAiScriptVisitor<TResult> typedVisitor = visitor as IAiScriptVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitQuote(this);
			else return visitor.VisitChildren(this);
		}
	}
	public partial class StringContext : ExpressionContext {
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode String() { return GetToken(AiScriptParser.String, 0); }
		public StringContext(ExpressionContext context) { CopyFrom(context); }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			IAiScriptListener typedListener = listener as IAiScriptListener;
			if (typedListener != null) typedListener.EnterString(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			IAiScriptListener typedListener = listener as IAiScriptListener;
			if (typedListener != null) typedListener.ExitString(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IAiScriptVisitor<TResult> typedVisitor = visitor as IAiScriptVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitString(this);
			else return visitor.VisitChildren(this);
		}
	}
	public partial class IntegerContext : ExpressionContext {
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode Integer() { return GetToken(AiScriptParser.Integer, 0); }
		public IntegerContext(ExpressionContext context) { CopyFrom(context); }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			IAiScriptListener typedListener = listener as IAiScriptListener;
			if (typedListener != null) typedListener.EnterInteger(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			IAiScriptListener typedListener = listener as IAiScriptListener;
			if (typedListener != null) typedListener.ExitInteger(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IAiScriptVisitor<TResult> typedVisitor = visitor as IAiScriptVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitInteger(this);
			else return visitor.VisitChildren(this);
		}
	}
	public partial class ListContext : ExpressionContext {
		[System.Diagnostics.DebuggerNonUserCode] public ExpressionContext[] expression() {
			return GetRuleContexts<ExpressionContext>();
		}
		[System.Diagnostics.DebuggerNonUserCode] public ExpressionContext expression(int i) {
			return GetRuleContext<ExpressionContext>(i);
		}
		public ListContext(ExpressionContext context) { CopyFrom(context); }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			IAiScriptListener typedListener = listener as IAiScriptListener;
			if (typedListener != null) typedListener.EnterList(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			IAiScriptListener typedListener = listener as IAiScriptListener;
			if (typedListener != null) typedListener.ExitList(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IAiScriptVisitor<TResult> typedVisitor = visitor as IAiScriptVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitList(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public ExpressionContext expression() {
		ExpressionContext _localctx = new ExpressionContext(Context, State);
		EnterRule(_localctx, 2, RULE_expression);
		int _la;
		try {
			State = 26;
			ErrorHandler.Sync(this);
			switch (TokenStream.LA(1)) {
			case Integer:
				_localctx = new IntegerContext(_localctx);
				EnterOuterAlt(_localctx, 1);
				{
				State = 12;
				Match(Integer);
				}
				break;
			case Symbol:
				_localctx = new SymbolContext(_localctx);
				EnterOuterAlt(_localctx, 2);
				{
				State = 13;
				Match(Symbol);
				}
				break;
			case String:
				_localctx = new StringContext(_localctx);
				EnterOuterAlt(_localctx, 3);
				{
				State = 14;
				Match(String);
				}
				break;
			case True:
			case False:
				_localctx = new BooleanContext(_localctx);
				EnterOuterAlt(_localctx, 4);
				{
				State = 15;
				_la = TokenStream.LA(1);
				if ( !(_la==True || _la==False) ) {
				ErrorHandler.RecoverInline(this);
				}
				else {
					ErrorHandler.ReportMatch(this);
				    Consume();
				}
				}
				break;
			case T__0:
				_localctx = new ListContext(_localctx);
				EnterOuterAlt(_localctx, 5);
				{
				State = 16;
				Match(T__0);
				State = 20;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
				while ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << T__0) | (1L << T__2) | (1L << True) | (1L << False) | (1L << Integer) | (1L << Symbol) | (1L << String))) != 0)) {
					{
					{
					State = 17;
					expression();
					}
					}
					State = 22;
					ErrorHandler.Sync(this);
					_la = TokenStream.LA(1);
				}
				State = 23;
				Match(T__1);
				}
				break;
			case T__2:
				_localctx = new QuoteContext(_localctx);
				EnterOuterAlt(_localctx, 6);
				{
				State = 24;
				Match(T__2);
				State = 25;
				expression();
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	private static char[] _serializedATN = {
		'\x3', '\x608B', '\xA72A', '\x8133', '\xB9ED', '\x417C', '\x3BE7', '\x7786', 
		'\x5964', '\x3', '\f', '\x1F', '\x4', '\x2', '\t', '\x2', '\x4', '\x3', 
		'\t', '\x3', '\x3', '\x2', '\a', '\x2', '\b', '\n', '\x2', '\f', '\x2', 
		'\xE', '\x2', '\v', '\v', '\x2', '\x3', '\x2', '\x3', '\x2', '\x3', '\x3', 
		'\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', 
		'\a', '\x3', '\x15', '\n', '\x3', '\f', '\x3', '\xE', '\x3', '\x18', '\v', 
		'\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x5', '\x3', '\x1D', 
		'\n', '\x3', '\x3', '\x3', '\x2', '\x2', '\x4', '\x2', '\x4', '\x2', '\x3', 
		'\x3', '\x2', '\b', '\t', '\x2', '#', '\x2', '\t', '\x3', '\x2', '\x2', 
		'\x2', '\x4', '\x1C', '\x3', '\x2', '\x2', '\x2', '\x6', '\b', '\x5', 
		'\x4', '\x3', '\x2', '\a', '\x6', '\x3', '\x2', '\x2', '\x2', '\b', '\v', 
		'\x3', '\x2', '\x2', '\x2', '\t', '\a', '\x3', '\x2', '\x2', '\x2', '\t', 
		'\n', '\x3', '\x2', '\x2', '\x2', '\n', '\f', '\x3', '\x2', '\x2', '\x2', 
		'\v', '\t', '\x3', '\x2', '\x2', '\x2', '\f', '\r', '\a', '\x2', '\x2', 
		'\x3', '\r', '\x3', '\x3', '\x2', '\x2', '\x2', '\xE', '\x1D', '\a', '\n', 
		'\x2', '\x2', '\xF', '\x1D', '\a', '\v', '\x2', '\x2', '\x10', '\x1D', 
		'\a', '\f', '\x2', '\x2', '\x11', '\x1D', '\t', '\x2', '\x2', '\x2', '\x12', 
		'\x16', '\a', '\x3', '\x2', '\x2', '\x13', '\x15', '\x5', '\x4', '\x3', 
		'\x2', '\x14', '\x13', '\x3', '\x2', '\x2', '\x2', '\x15', '\x18', '\x3', 
		'\x2', '\x2', '\x2', '\x16', '\x14', '\x3', '\x2', '\x2', '\x2', '\x16', 
		'\x17', '\x3', '\x2', '\x2', '\x2', '\x17', '\x19', '\x3', '\x2', '\x2', 
		'\x2', '\x18', '\x16', '\x3', '\x2', '\x2', '\x2', '\x19', '\x1D', '\a', 
		'\x4', '\x2', '\x2', '\x1A', '\x1B', '\a', '\x5', '\x2', '\x2', '\x1B', 
		'\x1D', '\x5', '\x4', '\x3', '\x2', '\x1C', '\xE', '\x3', '\x2', '\x2', 
		'\x2', '\x1C', '\xF', '\x3', '\x2', '\x2', '\x2', '\x1C', '\x10', '\x3', 
		'\x2', '\x2', '\x2', '\x1C', '\x11', '\x3', '\x2', '\x2', '\x2', '\x1C', 
		'\x12', '\x3', '\x2', '\x2', '\x2', '\x1C', '\x1A', '\x3', '\x2', '\x2', 
		'\x2', '\x1D', '\x5', '\x3', '\x2', '\x2', '\x2', '\x5', '\t', '\x16', 
		'\x1C',
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}