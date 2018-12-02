using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.Closure
{
    /// A diagnostic analyzer to check "lambda closures".
    /// checks:
    /// 1. lambda closure             (   (int para)=>{ closure }   )
    /// 2. anonymous closure          (   delegate(int para){ closure }   )
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoNotUseLambdaAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        ///     data structure of cache on invocations
        /// </summary>
        static MarkableInvocationGraph graph = new MarkableInvocationGraph(Predicate);

        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.DoNotUseLambda); }
        }


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeActionCatchable(AnalyzeNode, SyntaxKind.ClassDeclaration);
        }

        static SyntaxNode Predicate(SemanticModel sm, MethodDeclarationSyntax declare)
        {
            return declare.DescendantNodes().FirstOrDefault((node) =>
            {
                if (node.Kind() == SyntaxKind.ParenthesizedLambdaExpression ||
                    node.Kind() == SyntaxKind.SimpleLambdaExpression ||
                    node.Kind() == SyntaxKind.AnonymousMethodExpression)
                {
                    // check for exclusion
                    if (node.IsExcluded())
                    {
                        return false;
                    }

                    // we make a dataflow analysis here in order to do some exclusion
                    var dataflow = sm.AnalyzeDataFlow(node);
                    if (dataflow.DataFlowsIn.Length > 0)
                    {
                        // if the data flowing in contains something other than "this" or local variables declared in the method,
                        // we catch it
                        foreach (var data in dataflow.DataFlowsIn)
                        {
                            var d = sm.AnalyzeDataFlow(declare.Body);
                            if ((data.Kind == SymbolKind.Parameter && (data as IParameterSymbol)?.IsThis != true) ||
                                data.Kind == SymbolKind.Local)
                            {
                                return false;
                            }
                        }

                        return true;
                    }
                }

                return false;
            });
        }


        /// Action to be executed at completion of semantic analysis of a method in the target project.
        /// <param name="context">context for a symbol action.</param>
        static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcluded(DiagnosticIDs.DoNotUseLambda))
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
                    // represents a diagnostic, such as a compiler error or a warning, along with the location where it occurred
                    var diagnostic = Diagnostic.Create(DiagnosticDescriptors.DoNotUseLambda,
                        (callstack.FirstOrDefault() ?? method).GetLocation(), callstack.SyntaxStackToString());

                    // report a Diagnostic result
                    context.ReportDiagnostic(diagnostic);
                }
            });
        }
    }
}