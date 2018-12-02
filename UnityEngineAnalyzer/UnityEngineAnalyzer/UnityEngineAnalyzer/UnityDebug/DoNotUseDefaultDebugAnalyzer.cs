using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.UnityDebug
{
    /// A diagnostic analyzer to check "UnityEngine.Debug".
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoNotUseDefaultDebugAnalyzer : DiagnosticAnalyzer
    {
        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.DoNotUseDefaultDebug); }
        }


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeActionCatchable(AnalyzeNode, SyntaxKind.InvocationExpression);
        }


        /// Action to be executed at completion of semantic analysis of an invocation expression in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.DoNotUseDefaultDebug))
            {
                return;
            }

            // retrieve the method symbol and its containing type symbol
            var symbol = context.SemanticModel.GetSymbolInfo(context.Node).Symbol as IMethodSymbol;
            var type = symbol?.ContainingType;

            // check whether the containing type is "UnityEngine.Debug" and exclude methods which contain "Assert"(already marked with "ConditionalAttribute")
            if (symbol?.Name?.Contains("Assert") == false &&
                type?.Name?.Equals("Debug") == true &&
                type?.ContainingNamespace?.ToString().Equals("UnityEngine") == true)
            {
                var diagnostic =
                    Diagnostic.Create(DiagnosticDescriptors.DoNotUseDefaultDebug, context.Node.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}