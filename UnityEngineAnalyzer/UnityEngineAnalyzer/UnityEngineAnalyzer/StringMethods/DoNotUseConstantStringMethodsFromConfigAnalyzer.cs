using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.StringMethods
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoNotUseConstantStringMethodsFromConfigAnalyzer : DiagnosticAnalyzer
    {
        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.DoNotUseConstantStringMethodsFromConfig);

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
            UEASettingReader uEASettingReader = new UEASettingReader();
            UEASettings uEASettings = uEASettingReader.ReadConfigFile();
            List<string> noConstansMethods = uEASettings.noConstansFunctionRegexStrings;

            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.DoNotUseConstantStringMethodsFromConfig))
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
            bool notMatchMethod = true;
            foreach (var noConstansMethod in noConstansMethods)
            {
                if (noConstansMethod.Equals(methodFullDefinition))
                {
                    notMatchMethod = false;
                    break;
                }
            }

            if (notMatchMethod)
            {
                return;
            }


            bool hasContent = false;
            var argumentList = invocation.ArgumentList.ToFullString().Replace("(","").Replace(")","").Split(',');
            foreach(var argument in argumentList)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(argument, "\""))
                    hasContent = true;
            }
            
            if (hasContent)
            {
                // represents a diagnostic, such as a compiler error or a warning, along with the location where it occurred
                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.DoNotUseConstantStringMethodsFromConfig, invocation.GetLocation(), methodFullDefinition);

                // report a Diagnostic result
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
