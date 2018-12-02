using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.AOT
{
    /// A diagnostic analyzer to check "whether using System.Linq is declaraed".
    [DiagnosticAnalyzer(LanguageNames.CSharp)] /// this is a diagnostic analyzer for C# code
    public sealed class DoNotUseLinqAnalyzer : DiagnosticAnalyzer
    {
        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.DoNotUseLinq);


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of a using directive in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeNode, SyntaxKind.QueryExpression,
                SyntaxKind.InvocationExpression);
        }


        /// Action to be executed at completion of semantic analysis of a using directive in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.DoNotUseLinq))
            {
                return;
            }

            if (context.Node is QueryExpressionSyntax)
            {
                // report a Diagnostic result
                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.DoNotUseLinq, context.Node.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
            else if (context.Node is InvocationExpressionSyntax)
            {
                var symbol = context.SemanticModel.GetSymbolInfo(context.Node).Symbol;

                // and check if it is System.Linq
                if (symbol?.ContainingNamespace?.ToString().Equals("System.Linq") == true)
                {
                    // report a Diagnostic result
                    var diagnostic = Diagnostic.Create(DiagnosticDescriptors.DoNotUseLinq, context.Node.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}