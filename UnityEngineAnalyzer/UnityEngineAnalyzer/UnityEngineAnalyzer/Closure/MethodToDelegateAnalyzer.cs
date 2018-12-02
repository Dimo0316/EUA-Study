using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.Closure
{
    /// A diagnostic analyzer to check "method to delegate".
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MethodToDelegateAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        ///     data structure of cache on invocations
        /// </summary>
        static MarkableInvocationGraph graph = new MarkableInvocationGraph(Predicate);

        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.MethodToDelegate); }
        }


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeActionCatchable(AnalyzeNode, SyntaxKind.ClassDeclaration);
        }

        static SyntaxNode Predicate(SemanticModel sm, MethodDeclarationSyntax declare)
        {
            return declare.DescendantNodes().OfType<IdentifierNameSyntax>().FirstOrDefault((node) =>
            {
                // check for exclusion
                if (node.IsExcluded(DiagnosticIDs.MethodToDelegate))
                {
                    return false;
                }

                // retrieve the method symbol of this identifier
                var methodSymbol = sm.GetSymbolInfo(node).Symbol as IMethodSymbol;
                if (methodSymbol == null)
                {
                    return false;
                }

                // check whether it is converted into a delegate
                var converted = sm.GetTypeInfo(node).ConvertedType;
                if (converted?.TypeKind == TypeKind.Delegate)
                {
                    return true;
                }

                return false;
            });
        }


        /// Action to be executed at completion of semantic analysis of a method in the target project.
        /// <param name="context">context for a symbol action.</param>
        static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcluded(DiagnosticIDs.MethodToDelegate))
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
                    var diagnostic = Diagnostic.Create(DiagnosticDescriptors.MethodToDelegate,
                        (callstack.FirstOrDefault() ?? method).GetLocation(), callstack.SyntaxStackToString());

                    // report a Diagnostic result
                    context.ReportDiagnostic(diagnostic);
                }
            });
        }
    }
}