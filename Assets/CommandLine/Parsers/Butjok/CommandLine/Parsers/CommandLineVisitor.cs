//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.11.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from /Users/butjok/Documents/GitHub/Wars5/Assets/CommandLine/CommandLine.g4 by ANTLR 4.11.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace Butjok.CommandLine.Parsers {
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete generic visitor for a parse tree produced
/// by <see cref="CommandLineParser"/>.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.11.1")]
[System.CLSCompliant(false)]
public interface ICommandLineVisitor<Result> : IParseTreeVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by <see cref="CommandLineParser.input"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitInput([NotNull] CommandLineParser.InputContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>junction</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitJunction([NotNull] CommandLineParser.JunctionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>float2</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFloat2([NotNull] CommandLineParser.Float2Context context);
	/// <summary>
	/// Visit a parse tree produced by the <c>float3</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFloat3([NotNull] CommandLineParser.Float3Context context);
	/// <summary>
	/// Visit a parse tree produced by the <c>string</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitString([NotNull] CommandLineParser.StringContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>color</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitColor([NotNull] CommandLineParser.ColorContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>integer</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitInteger([NotNull] CommandLineParser.IntegerContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>real</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitReal([NotNull] CommandLineParser.RealContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>parenthesis</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParenthesis([NotNull] CommandLineParser.ParenthesisContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>command</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCommand([NotNull] CommandLineParser.CommandContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>enum</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEnum([NotNull] CommandLineParser.EnumContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>summation</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSummation([NotNull] CommandLineParser.SummationContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>int2</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitInt2([NotNull] CommandLineParser.Int2Context context);
	/// <summary>
	/// Visit a parse tree produced by the <c>boolean</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBoolean([NotNull] CommandLineParser.BooleanContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>null</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitNull([NotNull] CommandLineParser.NullContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>int3</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitInt3([NotNull] CommandLineParser.Int3Context context);
	/// <summary>
	/// Visit a parse tree produced by the <c>multiplication</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMultiplication([NotNull] CommandLineParser.MultiplicationContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>unaryExpression</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitUnaryExpression([NotNull] CommandLineParser.UnaryExpressionContext context);
}
} // namespace Butjok.CommandLine.Parsers
