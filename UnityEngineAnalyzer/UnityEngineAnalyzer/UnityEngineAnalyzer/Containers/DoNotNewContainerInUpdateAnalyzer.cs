using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.Containers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoNotNewContainerInUpdateAnalyzer : DiagnosticAnalyzer
    {
        /// list of all Container types
        private static ImmutableDictionary<string, List<string>> ContainerTypes = ImmutableDictionary.CreateRange(
            new Dictionary<string, List<string>>
            {
                {
                    "System.Collections.Generic", new List<string>
                    {
                        "List",
                        "Dictionary",
                        "LinkedList",
                        "Queue",
                        "Stack",
                        "SortedList",
                        "SortedDictionary",
                    }
                },
                {
                    "System.Collections", new List<string>
                    {
                        "ArrayList",
                        "Hashtable",
                        "Queue",
                        "Stack",
                        "SortedList",
                    }
                }
            });


        /// <summary>
        ///     data structure of cache on invocations
        /// </summary>
        private static MarkableInvocationGraph markGraph = new MarkableInvocationGraph(Predicate);

        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.DoNotNewContainerInUpdate); }
        }


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of a class declaration in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeSyntax, SyntaxKind.ClassDeclaration);
        }

        private static SyntaxNode Predicate(SemanticModel sm, MethodDeclarationSyntax declare)
        {
            return declare.DescendantNodes().FirstOrDefault((node) =>
            {
                if (node.IsExcluded(DiagnosticIDs.DoNotNewContainerInUpdate))
                {
                    return false;
                }

                // search object creation syntax
                if (node is ObjectCreationExpressionSyntax)
                {
                    // check whether the type of it is a container
                    var symbol = sm?.GetTypeInfo(node).Type as INamedTypeSymbol;
                    var ns = symbol?.ContainingNamespace?.ToString();
                    var type = symbol?.Name;
                    if (ContainerTypes.ContainsKey(ns) && ContainerTypes[ns].Contains(type))
                    {
                        return true;
                    }
                }

                return false;
            });
        }


        /// Action to be executed at completion of semantic analysis of a class declaration in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.DoNotNewContainerInUpdate))
            {
                return;
            }

            // check dirty classes in the cache
            markGraph.CheckClassDirty(context);

            // check from each Update method in a MonoBehaviour script(if it is)
            var mono = new MonoBehaviourInfo(context);
            mono.ForEachUpdateMethod((method) =>
            {
                Stack<SyntaxNode> callStack = new Stack<SyntaxNode>();
                var ret = markGraph.SearchMethod(context, method, callStack);
                if (ret)
                {
                    var diagnostic = Diagnostic.Create(DiagnosticDescriptors.DoNotNewContainerInUpdate,
                        (callStack.FirstOrDefault() ?? method).GetLocation(), callStack.SyntaxStackToString());
                    context.ReportDiagnostic(diagnostic);
                }
            });
        }
    }
}