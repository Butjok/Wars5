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
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete generic visitor for a parse tree produced
/// by <see cref="AiScriptParser"/>.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.9.1")]
[System.CLSCompliant(false)]
public interface IAiScriptVisitor<Result> : IParseTreeVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by <see cref="AiScriptParser.program"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitProgram([NotNull] AiScriptParser.ProgramContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>integer</c>
	/// labeled alternative in <see cref="AiScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitInteger([NotNull] AiScriptParser.IntegerContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>symbol</c>
	/// labeled alternative in <see cref="AiScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSymbol([NotNull] AiScriptParser.SymbolContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>string</c>
	/// labeled alternative in <see cref="AiScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitString([NotNull] AiScriptParser.StringContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>boolean</c>
	/// labeled alternative in <see cref="AiScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBoolean([NotNull] AiScriptParser.BooleanContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>list</c>
	/// labeled alternative in <see cref="AiScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitList([NotNull] AiScriptParser.ListContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>quote</c>
	/// labeled alternative in <see cref="AiScriptParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitQuote([NotNull] AiScriptParser.QuoteContext context);
}
