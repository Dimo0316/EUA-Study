using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.Preprocessor
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PreprocessorDirectiveUnityEditorAndDebugAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.PreprocessorDirectiveUnityEditorAndDebug);

        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of a class declaration in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeClassSyntax, SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeClassSyntax(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.PreprocessorDirectiveUnityEditorAndDebug))
            {
                return;
            }

            // check root
            var root = context.SemanticModel.SyntaxTree.GetRoot() as CompilationUnitSyntax;
            if (root == null)
            {
                return;
            }

            // check if in UnityEditor directory
            var filePath = root.GetLocation().ToString();
            var regex = new System.Text.RegularExpressions.Regex(@"\\Editor\\");
            if (regex.IsMatch(filePath))
            {
                return;
            }

            // cache symbol in preprocessor macro
            var symbolHashSet = new HashSet<ISymbol>();
            var tokens = root.DescendantNodes().OfType<IdentifierNameSyntax>();

            // check variable declared in macro
            foreach (var vdn in context.Node.DescendantNodes()
                .OfType<VariableDeclaratorSyntax>())
            {
                if (vdn.IsInUnityEditorProcessor())
                {
                    var nodeSymbol = context.SemanticModel.GetDeclaredSymbol(vdn) as IFieldSymbol;

                    symbolHashSet.Add(nodeSymbol);
                }
            }

            // report diagnostic
            foreach (var token in tokens)
            {
                var tokenSymbol = context.SemanticModel.GetSymbolInfo(token).Symbol;
                if (symbolHashSet.Contains(tokenSymbol))
                {
                    if (!token.IsInUnityEditorProcessor())
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            DiagnosticDescriptors.PreprocessorDirectiveUnityEditorAndDebug, token.GetLocation(), tokenSymbol.Name, tokenSymbol.Locations[0]));
                    }
                }
            }
        }
    }
}