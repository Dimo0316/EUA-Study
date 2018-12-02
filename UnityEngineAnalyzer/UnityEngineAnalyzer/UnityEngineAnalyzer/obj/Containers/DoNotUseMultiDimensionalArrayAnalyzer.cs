using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.Containers
{
    /// A diagnostic analyzer to check "int[,]"
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoNotUseMultiDimensionalArrayAnalyzer : DiagnosticAnalyzer
    {
        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.DoNotUseMultiDimensionalArray); }
        }


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of a class declaration in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeSyntax, SyntaxKind.ArrayType);
        }


        /// Action to be executed at completion of semantic analysis of a class declaration in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.DoNotUseMultiDimensionalArray))
            {
                return;
            }

            // retrieve the syntax
            var syntax = context.Node as ArrayTypeSyntax;
            if (syntax == null)
            {
                return;
            }

            // retrive the symbol
            var arraySymbol = context.SemanticModel.GetTypeInfo(syntax).Type as IArrayTypeSymbol;
            if (arraySymbol == null)
            {
                return;
            }

            // check if it is a multi-dimensional array
            if (arraySymbol.GetType().Name.Equals("MDArray"))
            {
                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.DoNotUseMultiDimensionalArray,
                    syntax.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}