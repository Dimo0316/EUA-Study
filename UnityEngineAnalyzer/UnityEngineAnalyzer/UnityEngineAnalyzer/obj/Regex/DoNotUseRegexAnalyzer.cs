using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.Regex
{
    /// A diagnostic analyzer to check "regular expression".
    [DiagnosticAnalyzer(LanguageNames.CSharp)] /// this is a diagnostic analyzer for C# code
    public sealed class DoNotUseRegexAnalyzer : DiagnosticAnalyzer
    {
        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.DoNotUseRegex);


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of a method in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeNode, SyntaxKind.InvocationExpression);
        }


        /// Action to be executed at completion of semantic analysis of a method in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.DoNotUseRegex))
            {
                return;
            }

            //  Check invocations
            if (context.Node is InvocationExpressionSyntax)
            {
                var symbol = context.SemanticModel.GetSymbolInfo(context.Node).Symbol;
                if (symbol != null && symbol.ContainingNamespace?.ToString().Equals("System.Text.RegularExpressions") ==
                    true)
                {
                    // represents a diagnostic, such as a compiler error or a warning, along with the location where it occurred
                    var diagnostic = Diagnostic.Create(DiagnosticDescriptors.DoNotUseRegex, context.Node.GetLocation());

                    // report a Diagnostic result
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}