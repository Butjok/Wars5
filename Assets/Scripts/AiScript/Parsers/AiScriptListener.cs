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

using Antlr4.Runtime.Misc;
using IParseTreeListener = Antlr4.Runtime.Tree.IParseTreeListener;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete listener for a parse tree produced by
/// <see cref="AiScriptParser"/>.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.9.1")]
[System.CLSCompliant(false)]
public interface IAiScriptListener : IParseTreeListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="AiScriptParser.program"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterProgram([NotNull] AiScriptParser.ProgramContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="AiScriptParser.program"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitProgram([NotNull] AiScriptParser.ProgramContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>integer</c>
	/// labeled alternative in <see cref="AiScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterInteger([NotNull] AiScriptParser.IntegerContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>integer</c>
	/// labeled alternative in <see cref="AiScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitInteger([NotNull] AiScriptParser.IntegerContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>symbol</c>
	/// labeled alternative in <see cref="AiScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterSymbol([NotNull] AiScriptParser.SymbolContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>symbol</c>
	/// labeled alternative in <see cref="AiScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitSymbol([NotNull] AiScriptParser.SymbolContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>string</c>
	/// labeled alternative in <see cref="AiScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterString([NotNull] AiScriptParser.StringContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>string</c>
	/// labeled alternative in <see cref="AiScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitString([NotNull] AiScriptParser.StringContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>boolean</c>
	/// labeled alternative in <see cref="AiScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBoolean([NotNull] AiScriptParser.BooleanContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>boolean</c>
	/// labeled alternative in <see cref="AiScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBoolean([NotNull] AiScriptParser.BooleanContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>list</c>
	/// labeled alternative in <see cref="AiScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterList([NotNull] AiScriptParser.ListContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>list</c>
	/// labeled alternative in <see cref="AiScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitList([NotNull] AiScriptParser.ListContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>quote</c>
	/// labeled alternative in <see cref="AiScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterQuote([NotNull] AiScriptParser.QuoteContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>quote</c>
	/// labeled alternative in <see cref="AiScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitQuote([NotNull] AiScriptParser.QuoteContext context);
}
