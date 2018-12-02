using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.OnGUI
{
    /// A diagnostic analyzer to check "whether any OnGUI method in MonoBehaviour derived classes".
    [DiagnosticAnalyzer(LanguageNames.CSharp)] /// this is a diagnostic analyzer for C# code
    public sealed class DoNotUseOnGUIAnalyzer : DiagnosticAnalyzer
    {
        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.DoNotUseOnGUI);


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of a method in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeSymbol, SyntaxKind.MethodDeclaration);
        }


        /// Action to be executed at completion of semantic analysis of a method in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.DoNotUseOnGUI))
            {
                return;
            }

            // retrieve the symbol
            var node = context.Node;
            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(node) as IMethodSymbol;
            if (methodSymbol == null)
            {
                return;
            }

            // check if the method is named OnGUI
            if (!methodSymbol.Name.Equals("OnGUI"))
            {
                return;
            }

            // check that it is contained in a class extended by UnityEngine.MonoBehaviour
            if (IsInheritFromMonoBehaviour(methodSymbol.ContainingType))
            {
                // represents a diagnostic, such as a compiler error or a warning, along with the location where it occurred
                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.DoNotUseOnGUI, methodSymbol.Locations[0],
                    methodSymbol.ContainingType.Name);

                // report a Diagnostic result
                context.ReportDiagnostic(diagnostic);
            }
        }


        /// check whether a type symbol inherits from MonoBehaviour
        private static bool IsInheritFromMonoBehaviour(INamedTypeSymbol type)
        {
            var basetype = type?.BaseType;
            if (basetype != null)
            {
                if (basetype?.ContainingNamespace?.ToString().Equals("UnityEngine") == true &&
                    basetype?.Name?.Equals("MonoBehaviour") == true)
                {
                    return true;
                }
                else
                {
                    return IsInheritFromMonoBehaviour(basetype);
                }
            }
            else
            {
                return false;
            }
        }
    }
}