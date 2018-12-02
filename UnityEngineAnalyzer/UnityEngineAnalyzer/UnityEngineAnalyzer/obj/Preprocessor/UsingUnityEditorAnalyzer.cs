using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.Preprocessor
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UsingUnityEditorAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DiagnosticDescriptors.UsingUnityEditor);

        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of a class declaration in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeClassSyntax, SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeClassSyntax(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.UsingUnityEditor))
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
            filePath = filePath.Replace('\\', '/');
            var regex = new System.Text.RegularExpressions.Regex("Assets(/(\\w)*)*/Editor/");
            if (regex.IsMatch(filePath))
            {
                return;
            }

            bool flag = false;
            regex = new System.Text.RegularExpressions.Regex("UnityEditor");
            // check namespace
            foreach (var ns in root.Usings)
            {
                if (regex.IsMatch(ns.Name.ToString()))
                {
                    flag = true;
                    if (!ns.IsInUnityEditorProcessor())
                    {
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.UsingUnityEditor, context.Node.GetLocation(), ns.ToString()));
                    }
                }
            }

            // check if not using UnityEditor
            if (!flag)
            {
                return;
            }

            // check all method in UnityEditor
            var tokens = root.DescendantNodes().OfType<IdentifierNameSyntax>();

            // report diagnostic
            foreach (var token in tokens)
            {
                var tokenSymbol = context.SemanticModel.GetSymbolInfo(token).Symbol;
                if (!regex.IsMatch(tokenSymbol.ContainingNamespace.Name)) continue;
                if (!token.IsInUnityEditorProcessor())
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.UsingUnityEditor, token.GetLocation(), token.ToString()));
                }
            }
        }
    }
}