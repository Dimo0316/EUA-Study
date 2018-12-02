using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.Containers
{
    /// A diagnostic analyzer to check "ArrayList"
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ArrayListAnalyzer : DiagnosticAnalyzer
    {
        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.ArrayList); }
        }


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of a class declaration in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeSyntax, SyntaxKind.ObjectCreationExpression);
        }
        
        /// Action to be executed at completion of semantic analysis of a class declaration in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.ArrayList))
            {
                return;
            }
            // retrieve its type symbol
            var root = context.SemanticModel.SyntaxTree.GetRoot();
            var symbol = context.SemanticModel.GetTypeInfo(context.Node).Type as INamedTypeSymbol;

            // check whether this type symbol is "System.Collections.ArrayList"
            if (symbol?.Name?.Equals("ArrayList") == true &&
                symbol?.ContainingNamespace?.ToString().Equals("System.Collections") == true)
            {
                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.ArrayList, context.Node.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}