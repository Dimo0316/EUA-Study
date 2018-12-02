using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.String
{
    /// A diagnostic analyzer to check "catenating strings with "==" operator".
    [DiagnosticAnalyzer(LanguageNames.CSharp)] /// this is a diagnostic analyzer for C# code
    public sealed class DoNotUseStringCompareMethodsAnalyzer : DiagnosticAnalyzer
    {
        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.DoNotUseStringCompareMethods);


        // forbidden string methods
        private static ImmutableArray<string> BlackListStringMethods => ImmutableArray.Create(
            "StartsWith",
            "EndsWith"
        );


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
            if (context.IsExcluded(DiagnosticIDs.DoNotUseStringCompareMethods))
            {
                return;
            }

            // retrieve add expression
            var symbol = context.SemanticModel.GetSymbolInfo(context.Node).Symbol;

            // check whether this expression is operating on string
            if (BlackListStringMethods.Contains(symbol?.Name) &&
                symbol?.ContainingType?.Name?.Equals("String") == true &&
                symbol?.ContainingNamespace?.ToString().Equals("System") == true)
            {
                // report a Diagnostic result
                var diagnostic =
                    Diagnostic.Create(DiagnosticDescriptors.DoNotUseStringCompareMethods, context.Node.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}