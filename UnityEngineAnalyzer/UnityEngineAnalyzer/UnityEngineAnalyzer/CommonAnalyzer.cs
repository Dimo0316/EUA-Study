using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CommonAnalyzer : DiagnosticAnalyzer
    {
        public static SyntaxTreeAnalysisContext treeContext;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.Common, DiagnosticDescriptors.Test); }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxTreeAction((t) =>
            {
                if (t.Tree != treeContext.Tree)
                {
                    treeContext = t;
                }
            });
            context.RegisterSyntaxNodeAction((t) =>
            {
                var classdeclare = t.Node as ClassDeclarationSyntax;
                if (classdeclare.Identifier.ToString() == "UEATest")
                {
                    t.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.Test, t.Node.GetLocation(),
                        t.SemanticModel?.Compilation?.AssemblyName ??
                        "[unknown]" + (t.SemanticModel?.Compilation?.Assembly?.Name)));
                }
            }, SyntaxKind.ClassDeclaration);
        }

        public static void CommonReport(string message)
        {
            if (treeContext.Tree != null)
            {
                treeContext.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.Common, null, message));
            }
        }
    }
}