using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.Coroutines
{
    /// A diagnostic analyzer to check "yield return 0".
    [DiagnosticAnalyzer(LanguageNames.CSharp)] /// this is a diagnostic analyzer for C# code
    public sealed class DoNotYieldReturnZeroAnalyzer : DiagnosticAnalyzer
    {
        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.DoNotYieldReturnZero);


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of an invocation expression in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeNode, SyntaxKind.YieldReturnStatement);
        }


        /// Action to be executed at completion of semantic analysis of an invocation expression in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.DoNotYieldReturnZero))
            {
                return;
            }

            // retrieve yield expression
            var yieldSyntax = context.Node as YieldStatementSyntax;
            if (yieldSyntax == null)
            {
                return;
            }

            var yieldExpression = yieldSyntax.Expression;
            if (yieldExpression == null)
            {
                return;
            }

            // retrieve type info of the expression
            var type = context.SemanticModel.GetTypeInfo(yieldExpression).Type;
            var convertedType = context.SemanticModel.GetTypeInfo(yieldExpression).ConvertedType;
            if (type == null || convertedType == null)
            {
                return;
            }

            // check if it is "yield return 0" converting to "yield return null"
            if ((yieldExpression is LiteralExpressionSyntax &&
                (yieldExpression as LiteralExpressionSyntax).Token.ValueText.Equals("0") &&
                convertedType.Name.Equals("Object") && convertedType.ContainingNamespace.ToString().Equals("System"))
                || yieldExpression is ObjectCreationExpressionSyntax)
            {
                // report a Diagnostic result
                var diagnostic =
                    Diagnostic.Create(DiagnosticDescriptors.DoNotYieldReturnZero, yieldSyntax.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}