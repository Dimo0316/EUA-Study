using System.Collections.Generic;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.ForbiddenMethods
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ForbiddenMethodsAnalyzer : DiagnosticAnalyzer
    {
        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.ForbiddenMethods);
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of a method in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeNode, SyntaxKind.InvocationExpression);
        }

        /// Action to be executed at completion of semantic analysis of an invocation expression in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            UEASettingReader uEASettingReader = new UEASettingReader();
            UEASettings uEASettings = uEASettingReader.ReadConfigFile();
            List<string> forbiddenMethods = uEASettings.prohibitedFunctionRegexStrings;

            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.ForbiddenMethods))
            {
                return;
            }

            // early out
            var invocation = context.Node as InvocationExpressionSyntax;
            if (invocation == null)
            {
                return;
            }
            
            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol;
            var symbolInfo = methodSymbol as IMethodSymbol;
            var methodFullDefinition = symbolInfo.OriginalDefinition.ToDisplayString().Split('(')[0];
            // check if any forbidden methods are used
            bool isForbidden = false;
            foreach(var forbiddenMethod in forbiddenMethods)
            {
                if (forbiddenMethod.Equals(methodFullDefinition))
                {
                    isForbidden = true;
                    break;
                }
            }


            if(isForbidden)
            {
                // represents a diagnostic, such as a compiler error or a warning, along with the location where it occurred
                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.ForbiddenMethods, invocation.GetLocation(), methodFullDefinition);

                // report a Diagnostic result
                context.ReportDiagnostic(diagnostic);
            }
            
        }
    }
}
