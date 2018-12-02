using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.Coroutines
{
    /// A diagnostic analyzer to check "whether any StartCoroutine method is called from UnityEngine namespace".
    [DiagnosticAnalyzer(LanguageNames.CSharp)] /// this is a diagnostic analyzer for C# code
    public sealed class DoNotUseCoroutinesAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        ///     data structure of cache on invocations
        /// </summary>
        private static MarkableInvocationGraph graph = new MarkableInvocationGraph(Predicate);

        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.DoNotUseCoroutines);


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of an invocation expression in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeNode, SyntaxKind.ClassDeclaration);
        }

        private static SyntaxNode Predicate(SemanticModel sm, MethodDeclarationSyntax declare)
        {
            return declare.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault((node) =>
            {
                if (node.IsExcluded())
                {
                    return false;
                }

                var invocation = node as InvocationExpressionSyntax;
                var methodSymbol = sm.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

                // check that the method is StartCoroutine from UnityEngine
                if (methodSymbol?.Name?.Equals("StartCoroutine") == true &&
                    methodSymbol?.ContainingSymbol?.ToString().Equals("UnityEngine.MonoBehaviour") == true)
                {
                    return true;
                }

                return false;
            });
        }


        /// Action to be executed at completion of semantic analysis of an invocation expression in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.DoNotUseCoroutines))
            {
                return;
            }

            // check dirty classes in the cache
            graph.CheckClassDirty(context);

            // check from each Update method in a MonoBehaviour script(if it is)
            context.ForEachMonoBehaviourUpdateMethod((method) =>
            {
                Stack<SyntaxNode> callStack = new Stack<SyntaxNode>();
                var ret = graph.SearchMethod(context, method, callStack);
                if (ret)
                {
                    // report a Diagnostic result
                    var diagnostic = Diagnostic.Create(DiagnosticDescriptors.DoNotUseCoroutines,
                        (callStack.FirstOrDefault() ?? method).GetLocation(), callStack.SyntaxStackToString());
                    context.ReportDiagnostic(diagnostic);
                }
            });
        }
    }
}