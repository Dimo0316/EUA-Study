using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.String
{
    /// A diagnostic analyzer to check "unnecessory string.Format calls".
    [DiagnosticAnalyzer(LanguageNames.CSharp)] /// this is a diagnostic analyzer for C# code
    public sealed class UseStringCatenatingInsteadOfFormat : DiagnosticAnalyzer
    {
        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.UseStringCatenatingInsteadOfFormat);


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of an invocation expression in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeNode, SyntaxKind.InvocationExpression);
        }


        /// Action to be executed at completion of semantic analysis of an invocation expression in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.UseStringCatenatingInsteadOfFormat))
            {
                return;
            }

            // retrieve the invocation symbol and syntax
            var invocation = context.Node as InvocationExpressionSyntax;
            var symbol = context.SemanticModel.GetSymbolInfo(context.Node).Symbol;

            // check whether this expression is string.Format
            if (symbol?.Name?.Equals("Format") == true &&
                symbol?.ContainingType?.Name?.Equals("String") == true &&
                symbol?.ContainingNamespace?.ToString().Equals("System") == true ||
                symbol?.Name?.Equals("BuildFormatString") == true &&
                symbol?.ContainingType?.Name?.Equals("Utilities") == true &&
                symbol?.ContainingNamespace?.ToString().Equals("X2Interface") == true)
            {
                // check its first argument
                var firstArgumentExpression = invocation?.ArgumentList?.Arguments[0];
                var formatString = firstArgumentExpression.GetArgumentValue<string>();
                if (string.IsNullOrEmpty(formatString))
                {
                    return;
                }

                // check whether the format string contains only basic patterns (such as "{0}", "{1}")
                if (System.Text.RegularExpressions.Regex.IsMatch(formatString, @"{\d+[\:]?[A-Za-z][\d\#]*\.?[\d\#]*}"))
                {
                    return;
                }

                // report a Diagnostic result
                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.UseStringCatenatingInsteadOfFormat,
                    context.Node.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}