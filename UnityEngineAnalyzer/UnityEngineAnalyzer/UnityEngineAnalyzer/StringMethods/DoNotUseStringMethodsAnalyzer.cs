using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.StringMethods
{
    /// A diagnostic analyzer to check "whether SendMessage, SendMessageUpwards, BroadcastMessage, are called".
    [DiagnosticAnalyzer(LanguageNames.CSharp)] /// this is a diagnostic analyzer for C# code
    public sealed class DoNotUseStringMethodsAnalyzer : DiagnosticAnalyzer
    {
        /// any "string" methods we are looking for.
        private static readonly ImmutableHashSet<string> StringMethods =
            ImmutableHashSet.Create("SendMessage", "SendMessageUpwards", "BroadcastMessage");


        /// only "string" methods from these namespaces are ones we are looking for.
        private static readonly ImmutableHashSet<string> Namespaces =
            ImmutableHashSet.Create("UnityEngine.Component", "UnityEngine.GameObject", "UnityEngine.MonoBehaviour");


        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.DoNotUseStringMethods);


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            
            // register an action to be executed at completion of semantic analysis of a method in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeNode, SyntaxKind.InvocationExpression);
        }


        /// Action to be executed at completion of semantic analysis of an invocation expression in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.DoNotUseStringMethods))
            {
                return;
            }

            // early out
            var invocation = context.Node as InvocationExpressionSyntax;
            if (invocation == null)
            {
                return;
            }

            // check if any of the "string" methods are used
            var name = invocation.MethodName();
            if (!StringMethods.Contains(name))
            {
                return;
            }

            // check if the method is the one from UnityEngine
            var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
            var methodSymbol = symbolInfo.Symbol as IMethodSymbol;
            if (Namespaces.Any(ns => methodSymbol?.ToString().StartsWith(ns) ?? false))
            {
                // report a Diagnostic result
                var diagnostic =
                    Diagnostic.Create(DiagnosticDescriptors.DoNotUseStringMethods, invocation.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}