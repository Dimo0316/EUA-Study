using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.Variable
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LocalFieldNeverUsedAnalyzer : DiagnosticAnalyzer
    {
  
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DiagnosticDescriptors.LocalFieldNeverUsed);

        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of a class declaration in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeClassSyntax, SyntaxKind.ClassDeclaration);
        }

        /// <summary>
        /// Used for Analyze PrivateFieldNeverUsed Problem
        /// </summary>
        /// <param name="context"></param>
        private void AnalyzeClassSyntax(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.LocalFieldNeverUsed))
            {
                return;
            }

            int x = 10;
            // cache private field symbol
            var symbolHashSet = new HashSet<ISymbol>();
            var root = context.SemanticModel.SyntaxTree.GetRoot();

            // get all variable expression except the delaration
            var tokens = root.DescendantNodes().OfType<IdentifierNameSyntax>();

            // get all private field symbol
            foreach (var vdn in context.Node.DescendantNodes()
                .OfType<VariableDeclaratorSyntax>())
            {
                var nodeSymbol = context.SemanticModel.GetDeclaredSymbol(vdn) as ILocalSymbol;

                if (nodeSymbol != null && nodeSymbol.Kind == SymbolKind.Local)
                {
                    symbolHashSet.Add(nodeSymbol);
                }
            }

            // check all variable expression
            foreach (var token in tokens)
            {
                var tokenSymbol = context.SemanticModel.GetSymbolInfo(token).Symbol;
                if (symbolHashSet.Contains(tokenSymbol))
                {
                    var parent = token.Parent as AssignmentExpressionSyntax;
                    if (parent != null)
                    {
                        // exclude the assignment expression which the variable is left value
                        if (parent.Left == token)
                        {
                            continue;
                        }
                    }

                    symbolHashSet.Remove(tokenSymbol);
                }
            }

            // report variable which is not used in this doc
            if (symbolHashSet.Count != 0)
            {
                foreach (var symbol in symbolHashSet)
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.LocalFieldNeverUsed, symbol.Locations[0], symbol.Name));
                }
            }
        }
    }
}