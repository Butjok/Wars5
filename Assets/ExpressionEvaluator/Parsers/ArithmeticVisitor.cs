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
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete generic visitor for a parse tree produced
/// by <see cref="ArithmeticParser"/>.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.11.1")]
[System.CLSCompliant(false)]
public interface IArithmeticVisitor<Result> : IParseTreeVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by the <c>baseExpression</c>
	/// labeled alternative in <see cref="ArithmeticParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBaseExpression([NotNull] ArithmeticParser.BaseExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>power</c>
	/// labeled alternative in <see cref="ArithmeticParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPower([NotNull] ArithmeticParser.PowerContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>multiplication</c>
	/// labeled alternative in <see cref="ArithmeticParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMultiplication([NotNull] ArithmeticParser.MultiplicationContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>grouping</c>
	/// labeled alternative in <see cref="ArithmeticParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitGrouping([NotNull] ArithmeticParser.GroupingContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>summation</c>
	/// labeled alternative in <see cref="ArithmeticParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSummation([NotNull] ArithmeticParser.SummationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ArithmeticParser.atom"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAtom([NotNull] ArithmeticParser.AtomContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ArithmeticParser.number"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitNumber([NotNull] ArithmeticParser.NumberContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ArithmeticParser.variable"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVariable([NotNull] ArithmeticParser.VariableContext context);
}