using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.Containers
{
    /// A diagnostic analyzer to check "params"
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoNotUseParams : DiagnosticAnalyzer
    {
        private static ImmutableDictionary<string, ImmutableArray<string>> OptimizableMethods =
            ImmutableDictionary.CreateRange(new Dictionary<string, ImmutableArray<string>>
                {
                    {"UnityEngine.Mathf", ImmutableArray.Create("Min", "Max")}
                }
            );

        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.DoNotUseParams); }
        }


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of a class declaration in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeSyntax, SyntaxKind.InvocationExpression);
        }


        /// Action to be executed at completion of semantic analysis of a class declaration in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.DoNotUseParams))
            {
                return;
            }

            // retrieve its type symbol
            var symbol = context.SemanticModel.GetSymbolInfo(context.Node).Symbol as IMethodSymbol;

            // retrive its method name and namespace
            var ns = symbol?.ContainingType?.ToString();
            var methodName = symbol?.Name;

            // check whether this method is what we are interested in
            if (!OptimizableMethods.ContainsKey(ns))
            {
                return;
            }

            if (!OptimizableMethods[ns].Contains(methodName))
            {
                return;
            }

            // retrive the parameters
            var parameters = symbol?.Parameters;
            if (parameters == null)
            {
                return;
            }

            foreach (var parameter in parameters)
            {
                if (parameter.IsParams)
                {
                    var diagnostic =
                        Diagnostic.Create(DiagnosticDescriptors.DoNotUseParams, context.Node.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                    return;
                }
            }
        }
    }
}