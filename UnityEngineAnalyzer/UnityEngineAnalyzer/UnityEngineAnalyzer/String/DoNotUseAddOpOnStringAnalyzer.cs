using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityEngineAnalyzer.String
{
    /// A diagnostic analyzer to check "catenating strings with "+" operator".
    [DiagnosticAnalyzer(LanguageNames.CSharp)] /// this is a diagnostic analyzer for C# code
    public sealed class DoNotUseAddOpOnStringAnalyzer : DiagnosticAnalyzer
    {
        /// returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.DoNotUseAddOpOnString);


        /// Called once at session start to register actions in the analysis context.
        /// <param name="context">context for initializing an analyzer.</param>
        public override void Initialize(AnalysisContext context)
        {
            // register an action to be executed at completion of semantic analysis of an invocation expression in the target project
            context.RegisterSyntaxNodeActionCatchable(AnalyzeNode, SyntaxKind.AddExpression,
                SyntaxKind.InvocationExpression);
        }


        /// Action to be executed at completion of semantic analysis of an invocation expression in the target project.
        /// <param name="context">context for a symbol action.</param>
        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            // check for exclusion
            if (context.IsExcluded(DiagnosticIDs.DoNotUseAddOpOnString))
            {
                return;
            }

            // check for "+" operation
            if (context.Node is BinaryExpressionSyntax)
            {
                // retrieve add expression
                var addExpression = context.Node as BinaryExpressionSyntax;
                if (addExpression == null || addExpression.Kind() != SyntaxKind.AddExpression)
                {
                    return;
                }

                // check whether this expression is operating on string
                if (IsString(context, addExpression.Left) || IsString(context, addExpression.Right))
                {
                    // report a Diagnostic result
                    var diagnostic = Diagnostic.Create(DiagnosticDescriptors.DoNotUseAddOpOnString,
                        addExpression.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }

            // check for "string.Concat()"
            if (context.Node is InvocationExpressionSyntax)
            {
                // retrieve the method symbol and its containing type symbol
                var method = context.SemanticModel.GetSymbolInfo(context.Node).Symbol as IMethodSymbol;
                var type = method?.ContainingType;

                // check whether this method is "System.String.Concat"
                if (method?.Name?.Equals("Concat") == true &&
                    type?.Name?.Equals("String") == true &&
                    type?.ContainingNamespace?.ToString().Equals("System") == true)
                {
                    // report a Diagnostic result
                    var diagnostic = Diagnostic.Create(DiagnosticDescriptors.DoNotUseAddOpOnString,
                        context.Node.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }


        /// Check whether a given syntax is a string
        private static bool IsString(SyntaxNodeAnalysisContext context, ExpressionSyntax syntax)
        {
            if (syntax == null)
            {
                return false;
            }

            var type = context.SemanticModel.GetTypeInfo(syntax).ConvertedType;
            if (type?.ContainingNamespace?.ToString().Equals("System") == true && type?.Name?.Equals("String") == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}