using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.Vectors
{
    /// A diagnostic analyzer to check "Vector.Distance"
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoNotUseVectorDistanceAnalyzer : DiagnosticAnalyzer
    {
        /// a list of all comparison operators
        private static ImmutableArray<SyntaxKind> ComparisonKinds = ImmutableArray.Create(
            SyntaxKind.EqualsExpression,
            SyntaxKind.NotEqualsExpression,
            SyntaxKind.GreaterThanExpression,
            SyntaxKind.LessThanExpression,
            SyntaxKind.GreaterThanOrEqualExpression,
            SyntaxKind.LessThanOrEqualExpression
        );


        /// all the vector class in UnityEngine
        private static ImmutableArray<string> VectorClasses = ImmutableArray.Create(
            "UnityEngine.Vector2",
            "UnityEngine.Vector2Int",
            "UnityEngine.Vector3",
            "UnityEngine.Vector3Int",
            "UnityEngine.Vector4"
        );


        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.DoNotUseVectorDistance); }
        }


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of a class declaration in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeSyntax, ComparisonKinds);
        }


        /// Action to be executed at completion of semantic analysis of a class declaration in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.DoNotUseVectorDistance))
            {
                return;
            }

            // retrieve the syntax
            var syntax = context.Node as BinaryExpressionSyntax;
            if (syntax == null)
            {
                return;
            }

            // check that the operator is a comparison operator
            if (!ComparisonKinds.Contains(syntax.Kind()))
            {
                return;
            }

            // find invocations under this syntax node
            var invocations = syntax.DescendantNodes().OfType<InvocationExpressionSyntax>();
            if (invocations != null)
            {
                // if there is only one "Vector.Distance" call found, the expression can be converted using "Vector.SqrMagnitude";
                // otherwise if there are multiple "Vector.Distance" calls found, the expression could be too complicated to be converted using "Vector.SqrMagnitude"
                SyntaxNode foundInvocation = null;

                foreach (var invocation in invocations)
                {
                    // retrieve the symbol for this invocation
                    SymbolInfo symbolInfo;
                    if (!context.TryGetSymbolInfo(invocation, out symbolInfo))
                    {
                        continue;
                    }

                    var methodSymbol = symbolInfo.Symbol as IMethodSymbol;
                    if (methodSymbol == null)
                    {
                        continue;
                    }

                    // check that it is "UnityEngine.VectorX.Distance"
                    if (methodSymbol.Name.Equals("Distance") &&
                        VectorClasses.Contains(methodSymbol.ContainingType.ToString()))
                    {
                        // make sure the diagnostic is only reported when there is only one "Vector.Distance" invocation in the comparison
                        if (foundInvocation != null)
                        {
                            return;
                        }

                        foundInvocation = invocation;
                    }
                }

                // if one and only one "Vector.Distance" invocation is found, we make the report
                if (foundInvocation != null)
                {
                    var diagnostic =
                        Diagnostic.Create(DiagnosticDescriptors.DoNotUseVectorDistance, foundInvocation.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}