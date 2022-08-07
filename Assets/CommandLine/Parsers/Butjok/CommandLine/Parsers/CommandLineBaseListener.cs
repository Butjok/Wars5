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

using Antlr4.Runtime.Misc;
using IErrorNode = Antlr4.Runtime.Tree.IErrorNode;
using ITerminalNode = Antlr4.Runtime.Tree.ITerminalNode;
using IToken = Antlr4.Runtime.IToken;
using ParserRuleContext = Antlr4.Runtime.ParserRuleContext;

/// <summary>
/// This class provides an empty implementation of <see cref="ICommandLineListener"/>,
/// which can be extended to create a listener which only needs to handle a subset
/// of the available methods.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.9.1")]
[System.Diagnostics.DebuggerNonUserCode]
[System.CLSCompliant(false)]
public partial class CommandLineBaseListener : ICommandLineListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommandLineParser.input"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterInput([NotNull] CommandLineParser.InputContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommandLineParser.input"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitInput([NotNull] CommandLineParser.InputContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>junction</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterJunction([NotNull] CommandLineParser.JunctionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>junction</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitJunction([NotNull] CommandLineParser.JunctionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>float2</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterFloat2([NotNull] CommandLineParser.Float2Context context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>float2</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitFloat2([NotNull] CommandLineParser.Float2Context context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>float3</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterFloat3([NotNull] CommandLineParser.Float3Context context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>float3</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitFloat3([NotNull] CommandLineParser.Float3Context context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>string</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterString([NotNull] CommandLineParser.StringContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>string</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitString([NotNull] CommandLineParser.StringContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>color</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterColor([NotNull] CommandLineParser.ColorContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>color</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitColor([NotNull] CommandLineParser.ColorContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>integer</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterInteger([NotNull] CommandLineParser.IntegerContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>integer</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitInteger([NotNull] CommandLineParser.IntegerContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>real</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterReal([NotNull] CommandLineParser.RealContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>real</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitReal([NotNull] CommandLineParser.RealContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>parenthesis</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterParenthesis([NotNull] CommandLineParser.ParenthesisContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>parenthesis</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitParenthesis([NotNull] CommandLineParser.ParenthesisContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>command</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterCommand([NotNull] CommandLineParser.CommandContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>command</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitCommand([NotNull] CommandLineParser.CommandContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>summation</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterSummation([NotNull] CommandLineParser.SummationContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>summation</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitSummation([NotNull] CommandLineParser.SummationContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>int2</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterInt2([NotNull] CommandLineParser.Int2Context context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>int2</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitInt2([NotNull] CommandLineParser.Int2Context context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>boolean</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterBoolean([NotNull] CommandLineParser.BooleanContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>boolean</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitBoolean([NotNull] CommandLineParser.BooleanContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>null</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterNull([NotNull] CommandLineParser.NullContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>null</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitNull([NotNull] CommandLineParser.NullContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>int3</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterInt3([NotNull] CommandLineParser.Int3Context context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>int3</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitInt3([NotNull] CommandLineParser.Int3Context context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>multiplication</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterMultiplication([NotNull] CommandLineParser.MultiplicationContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>multiplication</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitMultiplication([NotNull] CommandLineParser.MultiplicationContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>unaryExpression</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterUnaryExpression([NotNull] CommandLineParser.UnaryExpressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>unaryExpression</c>
	/// labeled alternative in <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitUnaryExpression([NotNull] CommandLineParser.UnaryExpressionContext context) { }

	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void EnterEveryRule([NotNull] ParserRuleContext context) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void ExitEveryRule([NotNull] ParserRuleContext context) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void VisitTerminal([NotNull] ITerminalNode node) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void VisitErrorNode([NotNull] IErrorNode node) { }
}
} // namespace Butjok.CommandLine.Parsers