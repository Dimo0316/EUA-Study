using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.String
{
    /// A diagnostic analyzer to check "string.Format".
    [DiagnosticAnalyzer(LanguageNames.CSharp)] /// this is a diagnostic analyzer for C# code
    public sealed class DoNotUseStringFormatAnalyzer : DiagnosticAnalyzer
    {
        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.DoNotUseStringFormat);


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of an invocation expression in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeNode, SyntaxKind.InvocationExpression);
        }


        /// Action to be executed at completion of semantic analysis of an invocation expression in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.DoNotUseStringFormat))
            {
                return;
            }

            // retrieve the invocation symbol
            var symbol = context.SemanticModel.GetSymbolInfo(context.Node).Symbol;

            // check whether this expression is string.Format
            if (symbol?.Name?.Equals("Format") == true &&
                symbol?.ContainingType?.Name?.Equals("String") == true &&
                symbol?.ContainingNamespace?.ToString().Equals("System") == true)
            {
                // report a Diagnostic result
                var diagnostic =
                    Diagnostic.Create(DiagnosticDescriptors.DoNotUseStringFormat, context.Node.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}