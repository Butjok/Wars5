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


using Antlr4.Runtime.Misc;
using IErrorNode = Antlr4.Runtime.Tree.IErrorNode;
using ITerminalNode = Antlr4.Runtime.Tree.ITerminalNode;
using IToken = Antlr4.Runtime.IToken;
using ParserRuleContext = Antlr4.Runtime.ParserRuleContext;

/// <summary>
/// This class provides an empty implementation of <see cref="IArithmeticListener"/>,
/// which can be extended to create a listener which only needs to handle a subset
/// of the available methods.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.11.1")]
[System.Diagnostics.DebuggerNonUserCode]
[System.CLSCompliant(false)]
public partial class ArithmeticBaseListener : IArithmeticListener {
	/// <summary>
	/// Enter a parse tree produced by the <c>baseExpression</c>
	/// labeled alternative in <see cref="ArithmeticParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterBaseExpression([NotNull] ArithmeticParser.BaseExpressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>baseExpression</c>
	/// labeled alternative in <see cref="ArithmeticParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitBaseExpression([NotNull] ArithmeticParser.BaseExpressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>power</c>
	/// labeled alternative in <see cref="ArithmeticParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterPower([NotNull] ArithmeticParser.PowerContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>power</c>
	/// labeled alternative in <see cref="ArithmeticParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitPower([NotNull] ArithmeticParser.PowerContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>multiplication</c>
	/// labeled alternative in <see cref="ArithmeticParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterMultiplication([NotNull] ArithmeticParser.MultiplicationContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>multiplication</c>
	/// labeled alternative in <see cref="ArithmeticParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitMultiplication([NotNull] ArithmeticParser.MultiplicationContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>grouping</c>
	/// labeled alternative in <see cref="ArithmeticParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterGrouping([NotNull] ArithmeticParser.GroupingContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>grouping</c>
	/// labeled alternative in <see cref="ArithmeticParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitGrouping([NotNull] ArithmeticParser.GroupingContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>summation</c>
	/// labeled alternative in <see cref="ArithmeticParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterSummation([NotNull] ArithmeticParser.SummationContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>summation</c>
	/// labeled alternative in <see cref="ArithmeticParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitSummation([NotNull] ArithmeticParser.SummationContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="ArithmeticParser.atom"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterAtom([NotNull] ArithmeticParser.AtomContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ArithmeticParser.atom"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitAtom([NotNull] ArithmeticParser.AtomContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="ArithmeticParser.number"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterNumber([NotNull] ArithmeticParser.NumberContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ArithmeticParser.number"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitNumber([NotNull] ArithmeticParser.NumberContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="ArithmeticParser.variable"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterVariable([NotNull] ArithmeticParser.VariableContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ArithmeticParser.variable"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitVariable([NotNull] ArithmeticParser.VariableContext context) { }

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
