using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.Containers
{
    /// A diagnostic analyzer to check "cache container api"
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CacheContainerApiAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        ///     data structure of cache on invocations
        /// </summary>
        static MarkableInvocationGraph graph = new MarkableInvocationGraph(Predicate);

        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.CacheContainerAPI); }
        }


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of a class declaration in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeSyntax, SyntaxKind.ClassDeclaration);
        }

        static SyntaxNode Predicate(SemanticModel sm, MethodDeclarationSyntax declare)
        {
            return declare.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault((node) =>
            {
                if (node.IsExcluded(DiagnosticIDs.CacheContainerAPI))
                {
                    return false;
                }

                // we only look for those which return a value, so we exclude left values of an assignment
                if (node.IsAssignmentLeftValue())
                {
                    return false;
                }

                // check whether it is under "UnityEngine" namespace, and whether it returns an array
                var symbol = sm.GetSymbolInfo(node).Symbol;
                if (symbol?.ContainingNamespace?.ToString().StartsWith("UnityEngine") == true &&
                    ((symbol as IMethodSymbol)?.ReturnType ?? (symbol as IPropertySymbol)?.Type)?.TypeKind ==
                    TypeKind.Array)
                {
                    return true;
                }

                return false;
            });
        }


        /// Action to be executed at completion of semantic analysis of a class declaration in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.CacheContainerAPI))
            {
                return;
            }

            // check dirty classes in the cache
            graph.CheckClassDirty(context);

            // check from each Update method in a MonoBehaviour script(if it is)
            context.ForEachMonoBehaviourUpdateMethod((method) =>
            {
                Stack<SyntaxNode> callstack = new Stack<SyntaxNode>();
                if (graph.SearchMethod(context, method, callstack))
                {
                    var diagnostic = Diagnostic.Create(DiagnosticDescriptors.CacheContainerAPI,
                        (callstack.FirstOrDefault() ?? method).GetLocation(), callstack.SyntaxStackToString());
                    context.ReportDiagnostic(diagnostic);
                }
            });
        }
    }
}