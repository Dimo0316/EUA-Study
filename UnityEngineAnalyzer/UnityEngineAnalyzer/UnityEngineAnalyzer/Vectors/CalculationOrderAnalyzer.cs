using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.Vectors
{
    /// A diagnostic analyzer to check "calculation order"
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CalculationOrderAnalyzer : DiagnosticAnalyzer
    {
        /// a list of all the calculation operators
        private static ImmutableArray<SyntaxKind> CalculationOperatorKinds = ImmutableArray.Create(
            SyntaxKind.AddExpression,
            SyntaxKind.SubtractExpression,
            SyntaxKind.MultiplyExpression,
            SyntaxKind.DivideExpression
        );


        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.CalculationOrder); }
        }


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of a class declaration in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeSyntax, CalculationOperatorKinds);
        }


        /// Action to be executed at completion of semantic analysis of a class declaration in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.CalculationOrder))
            {
                return;
            }

            // retrieve the syntax
            var syntax = context.Node as BinaryExpressionSyntax;
            if (syntax == null || !CalculationOperatorKinds.Contains(syntax.Kind()))
            {
                return;
            }

            // check its parent node, if its parent node is also a BinaryExpressionSyntax which shares the same priority, we should check the order
            if (syntax.Parent is BinaryExpressionSyntax)
            {
                var parent = syntax.Parent as BinaryExpressionSyntax;
                if (IsSamePriority(syntax, parent) && (parent.Left == syntax || parent.Right == syntax))
                {
                    // retrieve the three expressions
                    var innerLeft = syntax.Left;
                    var innerRight = syntax.Right;
                    var outter = parent.Left == syntax ? parent.Right : parent.Left;
                    if (innerLeft == null || innerRight == null || outter == null ||
                        IsString(context, innerLeft) || IsString(context, innerRight) || IsString(context, outter))
                    {
                        return;
                    }

                    // retrive the converted type symbol of the three expressions
                    var innerLeftType = context.SemanticModel.GetTypeInfo(innerLeft).Type;
                    var innerRightType = context.SemanticModel.GetTypeInfo(innerRight).Type;
                    var outterType = context.SemanticModel.GetTypeInfo(outter).Type;
                    if (innerLeftType == null || innerRightType == null || outterType == null)
                    {
                        return;
                    }

                    // if an inner expression has higher cost, and inner expression has different cost level, it should be moved outside
                    bool leftHigherThanOut = HasHigherCostThan(innerLeftType, outterType);
                    bool rightHigherThanOut = HasHigherCostThan(innerRightType, outterType);
                    bool innerEquative = !HasHigherCostThan(innerLeftType, innerRightType) &&
                                         !HasHigherCostThan(innerRightType, innerLeftType);
                    if ((leftHigherThanOut || rightHigherThanOut) && !innerEquative)
                    {
                        var diagnostic =
                            Diagnostic.Create(DiagnosticDescriptors.CalculationOrder, parent.GetLocation());
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }


        /// Check whether a given syntax is a string
        private static bool IsString(SyntaxNodeAnalysisContext context, ExpressionSyntax syntax)
        {
            if (syntax == null)
            {
                return false;
            }

            var type = context.SemanticModel.GetTypeInfo(syntax).ConvertedType;
            if (type?.ContainingNamespace?.ToString().Equals("System") == true && type?.Name?.Equals("String") == true)
            {
                return true;
            }

            return false;
        }


        /// Check whether the given two BinarySyntaxNode share the same priority
        private static bool IsSamePriority(BinaryExpressionSyntax op1, BinaryExpressionSyntax op2)
        {
            if ((op1.Kind() == SyntaxKind.AddExpression || op1.Kind() == SyntaxKind.SubtractExpression) &&
                (op2.Kind() == SyntaxKind.AddExpression || op2.Kind() == SyntaxKind.SubtractExpression) ||
                (op1.Kind() == SyntaxKind.MultiplyExpression || op1.Kind() == SyntaxKind.DivideExpression) &&
                (op2.Kind() == SyntaxKind.MultiplyExpression || op2.Kind() == SyntaxKind.DivideExpression))
            {
                return true;
            }

            return false;
        }


        /// Check whether the first type has higher cost than the second type
        /// int/uint
        /// < long/ ulong
        /// < float
        /// < double
        /// < others
        private static bool HasHigherCostThan(ITypeSymbol first, ITypeSymbol second)
        {
            return GetCostLevel(first) > GetCostLevel(second);
        }


        /// Return a value for level of cost of a type
        /// int/uint
        /// < long/ ulong
        /// < float
        /// < double
        /// < others
        private static int GetCostLevel(ITypeSymbol type)
        {
            if (type?.Kind == SymbolKind.PointerType)
            {
                return 0;
            }

            if (type?.ContainingNamespace?.ToString().Equals("System") == true)
            {
                switch (type.Name)
                {
                    case "Boolean":
                    case "Byte":
                    case "Char":
                    case "SByte":
                    case "Int16":
                    case "UInt16":
                    case "Int32":
                    case "UInt32":
                    case "Int64":
                    case "UInt64":
                        return 0;
                    case "Single":
                        return 1;
                    case "Double":
                        return 2;
                    default:
                        return 3;
                }
            }

            return 3;
        }
    }
}