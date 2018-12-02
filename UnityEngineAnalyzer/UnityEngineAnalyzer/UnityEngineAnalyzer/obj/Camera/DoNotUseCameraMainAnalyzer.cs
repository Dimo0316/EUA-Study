using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.Camera
{
    /// A diagnostic analyzer to check "Camera.main"
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoNotUseCameraMainAnalyzer : DiagnosticAnalyzer
    {
        /// dictionary for invocation marking state
        private static MarkableInvocationGraph markGraph = new MarkableInvocationGraph(Predicate);

        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.DoNotUseCameraMain); }
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
                if (node.IsExcluded(DiagnosticIDs.DoNotUseCameraMain))
                {
                    return false;
                }

                if (node.Kind() == SyntaxKind.SimpleMemberAccessExpression)
                {
                    // retrive the symbol
                    var symbolinfo = sm?.GetSymbolInfo(node);
                    IPropertySymbol propertySymbol = symbolinfo?.Symbol as IPropertySymbol;
                    if (propertySymbol == null)
                    {
                        return false;
                    }

                    // check if it equals to "UnityEngine.Camera.main"
                    if (propertySymbol.Name.Equals("main") && propertySymbol.ContainingType.Name.Equals("Camera") &&
                        propertySymbol.ContainingNamespace.Name.Equals("UnityEngine"))
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
            if (context.IsExcluded(DiagnosticIDs.DoNotUseCameraMain))
            {
                return;
            }

            markGraph.CheckClassDirty(context);

            // foreach monobehaviour update methods
            var mono = new MonoBehaviourInfo(context);
            mono.ForEachUpdateMethod((method) =>
            {
                Stack<SyntaxNode> callStack = new Stack<SyntaxNode>();
                var ret = markGraph.SearchMethod(context, method, callStack);

                // check if it equals to "UnityEngine.Camera.main"
                if (ret)
                {
                    var diagnostic = Diagnostic.Create(DiagnosticDescriptors.DoNotUseCameraMain,
                        (callStack.FirstOrDefault() ?? method).GetLocation(), callStack.SyntaxStackToString());
                    context.ReportDiagnostic(diagnostic);
                }
            });
        }
    }
}